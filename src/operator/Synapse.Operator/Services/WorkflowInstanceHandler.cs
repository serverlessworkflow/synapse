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

namespace Synapse.Operator.Services;

/// <summary>
/// Represents the service used to handle a specific <see cref="Resources.WorkflowInstance"/>
/// </summary>
/// <param name="logger">The service used to perform logging</param>
/// <param name="resources">The service used to manage <see cref="IResource"/>s</param>
/// <param name="runtime">The service used to create and run <see cref="IWorkflowProcess"/>es</param>
/// <param name="jsonSerializer">The service used to serialize/deserialize objects to/from JSON</param>
/// <param name="workflowInstance">The service used to monitor the resource of the workflow instance to handle</param>
public class WorkflowInstanceHandler(ILogger<WorkflowInstanceHandler> logger, IResourceRepository resources, IWorkflowRuntime runtime, IJsonSerializer jsonSerializer, IResourceMonitor<WorkflowInstance> workflowInstance)
    : IDisposable, IAsyncDisposable
{

    bool _disposed;

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger Logger { get; } = logger;

    /// <summary>
    /// Gets the service used to manage resources
    /// </summary>
    protected IResourceRepository Resources { get; } = resources;

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
    /// Gets the handler's subscription
    /// </summary>
    protected IDisposable? Subscription { get; set; }

    /// <summary>
    /// Gets the handled workflow instance's process, if any
    /// </summary>
    protected IWorkflowProcess? Process { get; set; }

    /// <summary>
    /// Handles the <see cref="Resources.WorkflowInstance"/>
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public virtual async Task HandleAsync(CancellationToken cancellationToken = default)
    {
        this.Subscription = this.WorkflowInstance
            .Where(e => e.Type == ResourceWatchEventType.Updated)
            .Select(e => e.Resource.Status?.Correlation?.Contexts)
            .Scan((Previous: (EquatableDictionary<string, CorrelationContext>?)null, Current: (EquatableDictionary<string, CorrelationContext>?)null), (accumulator, current) => (accumulator.Current ?? [], current))
            .Where(v => v.Current?.Count > v.Previous?.Count) //ensures we are not handling changes in a circular loop: if length of current is smaller than previous, it means a context has been processed
            .SubscribeAsync(async e => await OnCorrelationContextsChangedAsync(e, cancellationToken).ConfigureAwait(false));
        if (string.IsNullOrWhiteSpace(this.WorkflowInstance.Resource.Status?.Phase)
            || this.WorkflowInstance.Resource.Status.Phase == WorkflowInstanceStatusPhase.Pending
            || this.WorkflowInstance.Resource.Status.Phase == WorkflowInstanceStatusPhase.Running)
        {
            var workflow = await this.Resources.GetAsync<Workflow>(this.WorkflowInstance.Resource.Spec.Definition.Name, this.WorkflowInstance.Resource.Spec.Definition.Namespace, cancellationToken).ConfigureAwait(false) ?? throw new NullReferenceException($"Failed to find the workflow with name '{this.WorkflowInstance.Resource.Spec.Definition.Name}.{this.WorkflowInstance.Resource.Spec.Definition.Namespace}'");
            this.Process = await this.Runtime.CreateProcessAsync(workflow, this.WorkflowInstance.Resource, cancellationToken).ConfigureAwait(false);
            await this.Process.StartAsync(cancellationToken).ConfigureAwait(false);
        }
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
        if(this.WorkflowInstance.Resource.Status?.Phase == WorkflowInstanceStatusPhase.Waiting)
        {
            var workflow = await this.Resources.GetAsync<Workflow>(this.WorkflowInstance.Resource.Spec.Definition.Name, this.WorkflowInstance.Resource.Spec.Definition.Namespace, cancellationToken).ConfigureAwait(false) ?? throw new NullReferenceException($"Failed to find the workflow with name '{this.WorkflowInstance.Resource.Spec.Definition.Namespace}.{this.WorkflowInstance.Resource.Spec.Definition.Name}'");
            this.Process = await this.Runtime.CreateProcessAsync(workflow, this.WorkflowInstance.Resource, cancellationToken).ConfigureAwait(false);
            await this.Process.StartAsync(cancellationToken).ConfigureAwait(false);
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
        this.Subscription?.Dispose();
        if (this.Process != null) await this.Process.DisposeAsync().ConfigureAwait(false);
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
        this.Subscription?.Dispose();
        this.Process?.Dispose();
        this._disposed = true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

}