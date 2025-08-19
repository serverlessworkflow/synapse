// Copyright © 2024-Present The Synapse Authors
//
// Licensed under the Apache License, Version 2.0 (the "License"),
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Neuroglia.Data.Infrastructure.Services;
using System.Text;

namespace Synapse.Operator.Services;

/// <summary>
/// Represents the service used to handle a specific <see cref="Resources.WorkflowInstance"/>
/// </summary>
/// <param name="logger">The service used to perform logging</param>
/// <param name="options">The service used to access the current <see cref="OperatorOptions"/></param>
/// <param name="resources">The service used to manage <see cref="IResource"/>s</param>
/// <param name="logs">The service used the manage the logs produced by Synapse runners</param>
/// <param name="runtime">The service used to create and run <see cref="IWorkflowProcess"/>es</param>
/// <param name="jsonSerializer">The service used to serialize/deserialize objects to/from JSON</param>
/// <param name="workflowInstance">The service used to monitor the resource of the workflow instance to handle</param>
public class WorkflowInstanceHandler(ILogger<WorkflowInstanceHandler> logger, IOptions<OperatorOptions> options, IResourceRepository resources, ITextDocumentRepository logs, IWorkflowRuntime runtime, IJsonSerializer jsonSerializer, IResourceMonitor<WorkflowInstance> workflowInstance)
    : IDisposable, IAsyncDisposable
{

    bool _persistingLogs;
    bool _disposed;
    bool _suspended;

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger Logger { get; } = logger;

    /// <summary>
    /// Gets the options used to configure the current operator
    /// </summary>
    protected OperatorOptions Options { get; } = options.Value;

    /// <summary>
    /// Gets the service used to manage resources
    /// </summary>
    protected IResourceRepository Resources { get; } = resources;

    /// <summary>
    /// Gets the service used the manage the logs produced by Synapse runners
    /// </summary>
    protected ITextDocumentRepository Logs { get; } = logs;

    /// <summary>
    /// Gets the service used to create and run <see cref="IWorkflowProcess"/>es
    /// </summary>
    protected IWorkflowRuntime Runtime { get; } = runtime;

    /// <summary>
    /// Gets the service used to serialize/deserialize objects to/from JSON
    /// </summary>
    protected IJsonSerializer JsonSerializer { get; } = jsonSerializer;

    /// <summary>
    /// Gets the service used to monitor the resource of the workflow instance to handle
    /// </summary>
    protected IResourceMonitor<WorkflowInstance> WorkflowInstance { get; } = workflowInstance;

    /// <summary>
    /// Gets the handler's subscription used to monitor changes to the handled <see cref="Resources.WorkflowInstance"/>'s correlation contexts
    /// </summary>
    protected IDisposable? CorrelationContextSubscription { get; set; }

    /// <summary>
    /// Gets the <see cref="IDisposable"/> that represents the subscription to the logs produced by the  <see cref="WorkflowInstanceHandler"/>'s <see cref="IWorkflowProcess"/> 
    /// </summary>
    protected IDisposable? LogSubscription { get; set; }

    /// <summary>
    /// Gets the handled workflow instance's process, if any
    /// </summary>
    protected IWorkflowProcess? Process { get; set; }

    /// <summary>
    /// Gets the <see cref="Timer"/> used to periodically persist batches of logs
    /// </summary>
    protected Timer? LogBatchTimer { get; set; }

    /// <summary>
    /// Gets a <see cref="ConcurrentQueue{T}"/> used to enqueue logs before persisting them in batches
    /// </summary>
    protected ConcurrentQueue<string> LogBatchQueue = new();

    /// <summary>
    /// Gets the <see cref="WorkflowInstanceHandler"/>'s <see cref="System.Threading.CancellationTokenSource"/>
    /// </summary>
    protected CancellationTokenSource CancellationTokenSource { get; } = new();

    /// <summary>
    /// Handles the <see cref="Resources.WorkflowInstance"/>
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public virtual async Task HandleAsync(CancellationToken cancellationToken = default)
    {
        this.CorrelationContextSubscription = this.WorkflowInstance
            .Where(e => e.Type == ResourceWatchEventType.Updated)
            .Select(e => e.Resource.Status?.Correlation?.Contexts)
            .Scan((Previous: (EquatableDictionary<string, CorrelationContext>?)null, Current: (EquatableDictionary<string, CorrelationContext>?)null), (accumulator, current) => (accumulator.Current ?? [], current))
            .Where(v => v.Current?.Count > v.Previous?.Count) //ensures we are not handling changes in a circular loop: if length of current is smaller than previous, it means a context has been processed
            .SubscribeAsync(async e => await OnCorrelationContextsChangedAsync(e, cancellationToken).ConfigureAwait(false));
        this.WorkflowInstance.Where(e => e.Type == ResourceWatchEventType.Updated)
            .Select(e => e.Resource.Status?.Phase)
            .DistinctUntilChanged()
            .Where(ShouldResumeExecution)
            .SubscribeAsync(async _ => await this.StartProcessAsync(cancellationToken).ConfigureAwait(false));
        if (string.IsNullOrWhiteSpace(this.WorkflowInstance.Resource.Status?.Phase)
            || this.WorkflowInstance.Resource.Status.Phase == WorkflowInstanceStatusPhase.Pending
            || this.WorkflowInstance.Resource.Status.Phase == WorkflowInstanceStatusPhase.Running)
            await this.StartProcessAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates and starts a new <see cref="IWorkflowProcess"/>
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task StartProcessAsync(CancellationToken cancellationToken)
    {
        this.LogSubscription?.Dispose();
        var workflow = await this.GetWorkflowAsync(cancellationToken).ConfigureAwait(false);
        var serviceAccount = await this.GetServiceAccountAsync(cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(WorkflowInstance.Resource.Status?.ProcessId)) await Runtime.DeleteProcessAsync(WorkflowInstance.Resource.Status.ProcessId, cancellationToken).ConfigureAwait(false);
        this.Process = await this.Runtime.CreateProcessAsync(workflow, this.WorkflowInstance.Resource, serviceAccount, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(this.Process.Id)) await UpdateWorkflowInstanceStatusAsync(status =>
        {
            status.ProcessId = this.Process.Id;
        }, cancellationToken).ConfigureAwait(false);
        await this.Process.StartAsync(cancellationToken).ConfigureAwait(false);
        this.LogSubscription = this.Process.StandardOutput?.Subscribe(this.LogBatchQueue.Enqueue);
        this.LogBatchTimer ??= new(async _ => await this.OnPersistLogBatchAsync(), null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Gets the specified <see cref="Workflow"/>
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The specified <see cref="Workflow"/></returns>
    protected virtual async Task<Workflow> GetWorkflowAsync(CancellationToken cancellationToken)
    {
        return await this.Resources.GetAsync<Workflow>(this.WorkflowInstance.Resource.Spec.Definition.Name, this.WorkflowInstance.Resource.Spec.Definition.Namespace, cancellationToken).ConfigureAwait(false) 
            ?? throw new NullReferenceException($"Failed to find the workflow with name '{this.WorkflowInstance.Resource.Spec.Definition.Namespace}.{this.WorkflowInstance.Resource.Spec.Definition.Name}'");
    }

    /// <summary>
    /// Gets the current <see cref="ServiceAccount"/>
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The current <see cref="ServiceAccount"/></returns>
    protected virtual async Task<ServiceAccount> GetServiceAccountAsync(CancellationToken cancellationToken)
    {
        return await this.Resources.GetAsync<ServiceAccount>(this.Options.Name, this.Options.Namespace, cancellationToken).ConfigureAwait(false)
            ?? await this.Resources.GetAsync<ServiceAccount>(ServiceAccount.DefaultServiceAccountName, Namespace.DefaultNamespaceName, cancellationToken).ConfigureAwait(false)
            ?? throw new NullReferenceException($"Failed to find the default {nameof(ServiceAccount)} resource. Make sure the resource database is properly initialized.");
    }

    /// <summary>
    /// Determines whether or not the handler should resume the execution of the handled workflow instance
    /// </summary>
    /// <param name="statusPhase">The handled workflow instance's current status phase</param>
    /// <returns>A boolean indicating whether or not the handler should resume the execution of the handled workflow instance</returns>
    protected virtual bool ShouldResumeExecution(string? statusPhase)
    {
        if (statusPhase == WorkflowInstanceStatusPhase.Suspended)
        {
            this._suspended = true;
            return false;
        }
        return this._suspended && statusPhase == WorkflowInstanceStatusPhase.Running;
    }

    /// <summary>
    /// Handles changes to the the handled workflow instance's correlation contexts
    /// </summary>
    /// <param name="value">The context composite value, which contains the previous and the current state</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task OnCorrelationContextsChangedAsync((EquatableDictionary<string, CorrelationContext>? Previous, EquatableDictionary<string, CorrelationContext>? Current) value, CancellationToken cancellationToken)
    {
        var patch = JsonPatchUtility.CreateJsonPatchFromDiff(value.Previous, value.Current);
        if (patch.Operations.All(o => o.Op == Json.Patch.OperationType.Remove)) return;
        if (this.WorkflowInstance.Resource.Status?.Phase == WorkflowInstanceStatusPhase.Waiting) await this.StartProcessAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Persists log batches
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task OnPersistLogBatchAsync()
    {
        if (this._persistingLogs || this.LogBatchQueue.Count < 1) return;
        this._persistingLogs = true;
        var stringBuilder = new StringBuilder();
        while (this.LogBatchQueue.TryDequeue(out var log)) stringBuilder.AppendLine(log);
        var logs = stringBuilder.ToString();
        await this.Logs.AppendAsync($"{this.WorkflowInstance.Resource.GetName()}.{this.WorkflowInstance.Resource.GetNamespace()}", logs, this.CancellationTokenSource.Token).ConfigureAwait(false);
        this._persistingLogs = false;
    }

    /// <summary>
    /// Updates the status of the handled <see cref="Resources.WorkflowInstance"/>
    /// </summary>
    /// <param name="statusUpdate">An <see cref="Action{T}"/> used to update the <see cref="Resources.WorkflowInstance"/>'s status</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task UpdateWorkflowInstanceStatusAsync(Action<WorkflowInstanceStatus> statusUpdate, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(statusUpdate);
        var maxRetries = 3;
        for (var attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                var original = this.WorkflowInstance.Resource;
                var updated = original.Clone()!;
                updated.Status ??= new();
                statusUpdate(updated.Status);
                var patch = JsonPatchUtility.CreateJsonPatchFromDiff(original, updated);
                await this.Resources.PatchStatusAsync<WorkflowInstance>(new Patch(PatchType.JsonPatch, patch), updated.GetName(), updated.GetNamespace(), original.Metadata.ResourceVersion, false, cancellationToken).ConfigureAwait(false);
            }
            catch (ConcurrencyException) when (attempt + 1 < maxRetries)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100 * (attempt + 1)), cancellationToken).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Disposes of the <see cref="WorkflowInstanceHandler"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="WorkflowInstanceHandler"/> is being disposed of</param>
    /// <returns>A new awaitable <see cref="ValueTask"/></returns>
    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (!disposing || this._disposed) return;
        await this.WorkflowInstance.DisposeAsync().ConfigureAwait(false);
        this.CorrelationContextSubscription?.Dispose();
        this.LogSubscription?.Dispose();
        if (this.Process != null) await this.Process.DisposeAsync().ConfigureAwait(false);
        if (this.LogBatchTimer != null) await this.LogBatchTimer.DisposeAsync().ConfigureAwait(false);
        this.CancellationTokenSource.Dispose();
        this._disposed = true;
        await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await this.DisposeAsync(true).ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of the <see cref="WorkflowInstanceHandler"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="WorkflowInstanceHandler"/> is being disposed of</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing || this._disposed) return;
        this.WorkflowInstance.Dispose();
        this.CorrelationContextSubscription?.Dispose();
        this.LogSubscription?.Dispose();
        this.Process?.Dispose();
        this.LogBatchTimer?.Dispose();
        this.CancellationTokenSource.Dispose();
        this._disposed = true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

}