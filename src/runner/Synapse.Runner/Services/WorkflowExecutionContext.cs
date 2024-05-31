// Copyright © 2024-Present Neuroglia SRL. All rights reserved.
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

using Json.Patch;
using Json.Pointer;
using Neuroglia;
using Neuroglia.Data;
using Neuroglia.Data.Expressions;
using Neuroglia.Data.Expressions.Services;
using Neuroglia.Data.Infrastructure.ResourceOriented;
using Neuroglia.Eventing.CloudEvents;
using Synapse.Api.Client.Services;

namespace Synapse.Runner.Services;

/// <summary>
/// Represents the default in-memory implementation of the <see cref="IWorkflowExecutionContext"/>
/// </summary>
/// <param name="services">The current <see cref="IServiceProvider"/></param>
/// <param name="expressionEvaluator">The service used to evaluate runtime expressions</param>
/// <param name="jsonSerializer">The service used to serialize/deserialize objects to/from JSON</param>
/// <param name="cloudFlowsApi">The service used to interact with the Synapse API</param>
/// <param name="definition">The <see cref="WorkflowDefinition"/> of the <see cref="WorkflowInstance"/> to execute</param>
/// <param name="instance">The <see cref="WorkflowInstance"/> to execute</param>
public class WorkflowExecutionContext(IServiceProvider services, IExpressionEvaluator expressionEvaluator, IJsonSerializer jsonSerializer, ISynapseApiClient cloudFlowsApi, WorkflowDefinition definition, WorkflowInstance instance)
    : IWorkflowExecutionContext
{

    /// <inheritdoc/>
    public IServiceProvider Services { get; } = services;

    /// <inheritdoc/>
    public IExpressionEvaluator Expressions { get; } = expressionEvaluator;

    /// <summary>
    /// Gets the service used to serialize/deserialize objects to/from JSON
    /// </summary>
    protected IJsonSerializer JsonSerializer { get; } = jsonSerializer;

    /// <summary>
    /// Gets the service used to interact with the Synapse API
    /// </summary>
    protected ISynapseApiClient Api { get; } = cloudFlowsApi;

    /// <inheritdoc/>
    public WorkflowDefinition Definition { get; } = definition;

    /// <inheritdoc/>
    public WorkflowInstance Instance { get; set; } = instance;

    /// <inheritdoc/>
    public IDocumentApiClient Documents => this.Api.WorkflowData;

    /// <summary>
    /// Gets the object used to asynchronously lock the <see cref="TaskExecutor{TDefinition}"/>
    /// </summary>
    protected AsyncLock Lock { get; } = new();

    /// <inheritdoc/>
    public IDictionary<string, object> ContextData { get; protected set; } = new Dictionary<string, object>();

    /// <inheritdoc/>
    public IDictionary<string, object> Arguments { get; protected set; } = new Dictionary<string, object>();

    /// <inheritdoc/>
    public object? Output { get; protected set; }

    /// <inheritdoc/>
    public virtual Task ContinueWith(TaskDefinition task, CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <inheritdoc/>
    public virtual async Task<TaskInstance> CreateTaskAsync(TaskDefinition definition, string path, object input, IDictionary<string, object>? context = null, ITaskExecutionContext? parent = null, bool isExtension = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);
        using var @lock = await this.Lock.LockAsync(cancellationToken).ConfigureAwait(false);
        input ??= new();
        var reference = this.Definition.BuildReferenceTo(definition, path, parent?.Instance.Reference);
        var contextReference = string.Empty;
        if (context == null)
        {
            if(parent?.ContextData != null)
            {
                contextReference = parent.Instance.ContextReference;
                context = parent.ContextData;
            }
            else if(!string.IsNullOrWhiteSpace(this.Instance.Status?.ContextReference))
            {
                contextReference = this.Instance.Status?.ContextReference;
                var contextDocument = await this.Api.WorkflowData.GetAsync(this.Instance.Status!.ContextReference, cancellationToken).ConfigureAwait(false);
                context = contextDocument?.Content.ConvertTo<IDictionary<string, object>>() ?? new Dictionary<string, object>();
            }
            else throw new NullReferenceException($"Failed to find the data document with id '{this.Instance.Status!.ContextReference}'");
        }
        else
        {
            var contextDocument = await this.Api.WorkflowData.CreateAsync($"{reference}/input", context, cancellationToken).ConfigureAwait(false);
            contextReference = contextDocument.Id;
        }
        var filteredInput = input;
        var evaluationArguments = new Dictionary<string, object>()
        {
            { RuntimeExpressions.Arguments.Workflow, Instance },
            { RuntimeExpressions.Arguments.Context, context }
        };
        if (definition.Input?.From is string fromExpression) filteredInput = (await this.Expressions.EvaluateAsync<object>(fromExpression, input, evaluationArguments, cancellationToken).ConfigureAwait(false))!;
        else if (definition.Input?.From != null) filteredInput = (await this.Expressions.EvaluateAsync<object>(definition.Input.From, input, evaluationArguments, cancellationToken).ConfigureAwait(false))!;
        var inputDocument = await this.Api.WorkflowData.CreateAsync($"{reference}/input", filteredInput, cancellationToken).ConfigureAwait(false);
        var task = new TaskInstance()
        {
            Name = path.Split('/', StringSplitOptions.RemoveEmptyEntries).Last(),
            Reference = reference,
            IsExtension = isExtension,
            ParentId = parent?.Instance.Id,
            InputReference = inputDocument.Id,
            ContextReference = contextReference
        };
        var pointer = JsonPointer.Create<WorkflowInstance>(w => w.Status!.Tasks!).ToCamelCase();
        if (this.Instance.Status?.Tasks != null) pointer = JsonPointer.Parse($"{pointer}/-");
        var patchNode = this.Instance.Status?.Tasks == null ? this.JsonSerializer.SerializeToNode(new TaskInstance[] { task }) : this.JsonSerializer.SerializeToNode(task);
        var jsonPatch = new JsonPatch(PatchOperation.Add(pointer, patchNode));
        this.Instance = await this.Api.WorkflowInstances.PatchStatusAsync(this.Instance.GetName(), this.Instance.GetNamespace()!, new Patch(PatchType.JsonPatch, jsonPatch), cancellationToken).ConfigureAwait(false);
        return task;
    }

    /// <inheritdoc/>
    public virtual async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var document = await this.Api.WorkflowData.CreateAsync(this.Instance.GetQualifiedName(), this.Instance.Spec.Input ?? [], cancellationToken).ConfigureAwait(false);
        var jsonPatch = new JsonPatch(PatchOperation.Add(JsonPointer.Create<WorkflowInstance>(w => w.Status!).ToCamelCase(), this.JsonSerializer.SerializeToNode(new WorkflowInstanceStatus()
        {
            ContextReference = document.Id
        })));
        this.Instance = await this.Api.WorkflowInstances.PatchStatusAsync(this.Instance.GetName(), Instance.GetNamespace()!, new Patch(PatchType.JsonPatch, jsonPatch), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual Task<TaskInstance> InitializeAsync(TaskInstance task, CancellationToken cancellationToken = default)
    {
        //task = this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
        //task.Status = TaskInstanceStatus.Initializing;
        return Task.FromResult(task);
    }

    /// <inheritdoc/>
    public virtual async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var @lock = await this.Lock.LockAsync(cancellationToken).ConfigureAwait(false);
        var originalInstance = this.Instance.Clone();
        this.Instance.Status ??= new();
        this.Instance.Status.Phase = WorkflowInstanceStatusPhase.Running;
        this.Instance.Status.StartedAt ??= DateTimeOffset.UtcNow;
        this.Instance.Status.Runs ??= [];
        this.Instance.Status.Runs.Add(new() { StartedAt = DateTimeOffset.Now });
        this.Instance.Status.ContextReference ??= (await this.Documents.CreateAsync(this.Instance.GetQualifiedName(), this.ContextData, cancellationToken).ConfigureAwait(false)).Id;
        var jsonPatch = JsonPatchUtility.CreateJsonPatchFromDiff(originalInstance, this.Instance);
        this.Instance = await this.Api.WorkflowInstances.PatchStatusAsync(this.Instance.GetName(), this.Instance.GetNamespace()!, new Patch(PatchType.JsonPatch, jsonPatch), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task<TaskInstance> ExecuteAsync(TaskInstance task, CancellationToken cancellationToken = default)
    {
        using var @lock = await this.Lock.LockAsync(cancellationToken).ConfigureAwait(false);
        var originalInstance = this.Instance.Clone();
        task = this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
        task.Status = TaskInstanceStatus.Running;
        task.StartedAt ??= DateTimeOffset.Now;
        task.Runs ??= [];
        task.Runs.Add(new() { StartedAt = DateTimeOffset.Now });
        var jsonPatch = JsonPatchUtility.CreateJsonPatchFromDiff(originalInstance, this.Instance);
        this.Instance = await this.Api.WorkflowInstances.PatchStatusAsync(this.Instance.GetName(), this.Instance.GetNamespace()!, new Patch(PatchType.JsonPatch, jsonPatch), cancellationToken).ConfigureAwait(false);
        return this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
    }

    /// <inheritdoc/>
    public virtual Task<IEnumerable<CloudEvent>> ConsumeOrBeginCorrelateAsync(ListenerDefinition listener, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
        //ArgumentNullException.ThrowIfNull(listener);
        //todo: urgent: implement
        //return [];
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TaskInstance> GetTasksAsync(CancellationToken cancellationToken = default) => (this.Instance.Status?.Tasks ?? []).ToAsyncEnumerable();

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TaskInstance> GetTasksAsync(TaskInstance task, CancellationToken cancellationToken = default) => this.GetTasksAsync(cancellationToken).Where(t => t.ParentId == task.Id);

    /// <inheritdoc/>
    public virtual async Task<TaskInstance> RetryAsync(TaskInstance task, Error cause, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(cause);
        using var @lock = await this.Lock.LockAsync(cancellationToken).ConfigureAwait(false);
        var originalInstance = this.Instance.Clone();
        task = this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
        task.Retries ??= [];
        task.Retries.Add(new RetryAttempt()
        {
            Number = (uint)task.Retries.Count + 1,
            Cause = cause
        });
        task.Status = TaskInstanceStatus.Running;
        task.StartedAt ??= DateTimeOffset.Now;
        task.Runs ??= [];
        task.Runs.Add(new() { StartedAt = DateTimeOffset.Now });
        var jsonPatch = JsonPatchUtility.CreateJsonPatchFromDiff(originalInstance, this.Instance);
        this.Instance = await this.Api.WorkflowInstances.PatchStatusAsync(this.Instance.GetName(), this.Instance.GetNamespace()!, new Patch(PatchType.JsonPatch, jsonPatch), cancellationToken).ConfigureAwait(false);
        return this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
    }

    /// <inheritdoc/>
    public virtual async Task SetErrorAsync(Error error, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(error);
        using var @lock = await this.Lock.LockAsync(cancellationToken).ConfigureAwait(false);
        var originalInstance = this.Instance.Clone();
        this.Instance.Status ??= new();
        this.Instance.Status.Phase = WorkflowInstanceStatusPhase.Faulted;
        this.Instance.Status.EndedAt = DateTimeOffset.Now;
        this.Instance.Status.Error = error;
        var run = this.Instance.Status.Runs?.LastOrDefault();
        if (run != null) run.EndedAt = DateTimeOffset.Now;
        var jsonPatch = JsonPatchUtility.CreateJsonPatchFromDiff(originalInstance, this.Instance);
        this.Instance = await this.Api.WorkflowInstances.PatchStatusAsync(this.Instance.GetName(), this.Instance.GetNamespace()!, new Patch(PatchType.JsonPatch, jsonPatch), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task<TaskInstance> SetErrorAsync(TaskInstance task, Error error, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(error);
        using var @lock = await this.Lock.LockAsync(cancellationToken).ConfigureAwait(false);
        var originalInstance = this.Instance.Clone();
        task = this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
        task.Status = TaskInstanceStatus.Faulted;
        task.EndedAt = DateTimeOffset.Now;
        task.Error = error;
        var run = task.Runs?.LastOrDefault();
        if(run != null)
        {
            run.EndedAt = DateTimeOffset.Now;
            run.Outcome = task.Status;
        }
        var jsonPatch = JsonPatchUtility.CreateJsonPatchFromDiff(originalInstance, this.Instance);
        this.Instance = await this.Api.WorkflowInstances.PatchStatusAsync(this.Instance.GetName(), this.Instance.GetNamespace()!, new Patch(PatchType.JsonPatch, jsonPatch), cancellationToken).ConfigureAwait(false);
        return this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
    }

    /// <inheritdoc/>
    public virtual async Task SetResultAsync(object? result, CancellationToken cancellationToken = default)
    {
        using var @lock = await this.Lock.LockAsync(cancellationToken).ConfigureAwait(false);
        var originalInstance = this.Instance.Clone();
        result ??= new();
        this.Instance.Status ??= new();
        this.Instance.Status.Phase = WorkflowInstanceStatusPhase.Completed;
        this.Instance.Status.EndedAt = DateTimeOffset.Now;
        var document = await this.Api.WorkflowData.CreateAsync($"{this.Instance.GetQualifiedName()}/output", result, cancellationToken).ConfigureAwait(false);
        this.Instance.Status.OutputReference = document.Id;
        this.Output = document.Content;
        var run = this.Instance.Status.Runs?.LastOrDefault();
        if (run != null) run.EndedAt = DateTimeOffset.Now;
        var jsonPatch = JsonPatchUtility.CreateJsonPatchFromDiff(originalInstance, this.Instance);
        this.Instance = await this.Api.WorkflowInstances.PatchStatusAsync(this.Instance.GetName(), this.Instance.GetNamespace()!, new Patch(PatchType.JsonPatch, jsonPatch), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task<TaskInstance> SetResultAsync(TaskInstance task, object? result, string? then = FlowDirective.Continue, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        using var @lock = await this.Lock.LockAsync(cancellationToken).ConfigureAwait(false);
        var originalInstance = this.Instance.Clone();
        result ??= new();
        task = this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
        task.Status = TaskInstanceStatus.Completed;
        task.EndedAt = DateTimeOffset.Now;
        var document = await this.Api.WorkflowData.CreateAsync($"{this.Instance.GetQualifiedName()}/{task.Reference}/output", result, cancellationToken).ConfigureAwait(false);
        task.OutputReference = document.Id;
        task.Next = then;
        var run = task.Runs!.Last();
        run.EndedAt = DateTimeOffset.Now;
        run.Outcome = task.Status;
        var jsonPatch = JsonPatchUtility.CreateJsonPatchFromDiff(originalInstance, this.Instance);
        this.Instance = await this.Api.WorkflowInstances.PatchStatusAsync(this.Instance.GetName(), this.Instance.GetNamespace()!, new Patch(PatchType.JsonPatch, jsonPatch), cancellationToken).ConfigureAwait(false);
        return this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
    }

    /// <inheritdoc/>
    public virtual async Task SetWorkflowDataAsync(string reference, object data, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reference);
        await this.Api.WorkflowData.UpdateAsync(reference, data, cancellationToken).ConfigureAwait(false);
        if (this.Instance.Status?.ContextReference == reference) this.ContextData = data.ConvertTo<IDictionary<string, object>>()!;
    }

    /// <inheritdoc/>
    public virtual async Task<TaskInstance> SkipAsync(TaskInstance task, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        using var @lock = await this.Lock.LockAsync(cancellationToken).ConfigureAwait(false);
        var originalInstance = this.Instance.Clone();
        task = this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
        task.Status = TaskInstanceStatus.Skipped;
        var run = task.Runs!.Last();
        run.EndedAt = DateTimeOffset.Now;
        run.Outcome = task.Status;
        var jsonPatch = JsonPatchUtility.CreateJsonPatchFromDiff(originalInstance, this.Instance);
        this.Instance = await this.Api.WorkflowInstances.PatchStatusAsync(this.Instance.GetName(), this.Instance.GetNamespace()!, new Patch(PatchType.JsonPatch, jsonPatch), cancellationToken).ConfigureAwait(false);
        return this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
    }

    /// <inheritdoc/>
    public virtual async Task SuspendAsync(CancellationToken cancellationToken = default)
    {
        using var @lock = await this.Lock.LockAsync(cancellationToken).ConfigureAwait(false);
        var originalInstance = this.Instance.Clone();
        this.Instance.Status ??= new();
        this.Instance.Status.Phase = WorkflowInstanceStatusPhase.Suspended;
        var run = this.Instance.Status.Runs?.LastOrDefault();
        if (run != null) run.EndedAt = DateTimeOffset.Now;
        var jsonPatch = JsonPatchUtility.CreateJsonPatchFromDiff(originalInstance, this.Instance);
        this.Instance = await this.Api.WorkflowInstances.PatchStatusAsync(this.Instance.GetName(), this.Instance.GetNamespace()!, new Patch(PatchType.JsonPatch, jsonPatch), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task<TaskInstance> SuspendAsync(TaskInstance task, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        using var @lock = await this.Lock.LockAsync(cancellationToken).ConfigureAwait(false);
        var originalInstance = this.Instance.Clone();
        task = this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
        task.Status = TaskInstanceStatus.Suspended;
        var run = task.Runs!.Last();
        run.EndedAt = DateTimeOffset.Now;
        run.Outcome = task.Status;
        var jsonPatch = JsonPatchUtility.CreateJsonPatchFromDiff(originalInstance, this.Instance);
        this.Instance = await this.Api.WorkflowInstances.PatchStatusAsync(this.Instance.GetName(), this.Instance.GetNamespace()!, new Patch(PatchType.JsonPatch, jsonPatch), cancellationToken).ConfigureAwait(false);
        return this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
    }

    /// <inheritdoc/>
    public virtual async Task CancelAsync(CancellationToken cancellationToken = default)
    {
        using var @lock = await this.Lock.LockAsync(cancellationToken).ConfigureAwait(false);
        var originalInstance = this.Instance.Clone();
        this.Instance.Status ??= new();
        this.Instance.Status.Phase = WorkflowInstanceStatusPhase.Cancelled;
        this.Instance.Status.EndedAt = DateTimeOffset.Now;
        var run = this.Instance.Status.Runs?.LastOrDefault();
        if (run != null) run.EndedAt = DateTimeOffset.Now;
        var jsonPatch = JsonPatchUtility.CreateJsonPatchFromDiff(originalInstance, this.Instance);
        this.Instance = await this.Api.WorkflowInstances.PatchStatusAsync(this.Instance.GetName(), this.Instance.GetNamespace()!, new Patch(PatchType.JsonPatch, jsonPatch), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task<TaskInstance> CancelAsync(TaskInstance task, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        using var @lock = await this.Lock.LockAsync(cancellationToken).ConfigureAwait(false);
        var originalInstance = this.Instance.Clone();
        task = this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
        task.Status = TaskInstanceStatus.Cancelled;
        task.EndedAt = DateTimeOffset.Now;
        var run = task.Runs!.Last();
        run.EndedAt = DateTimeOffset.Now;
        run.Outcome = task.Status;
        var jsonPatch = JsonPatchUtility.CreateJsonPatchFromDiff(originalInstance, this.Instance);
        this.Instance = await this.Api.WorkflowInstances.PatchStatusAsync(this.Instance.GetName(), this.Instance.GetNamespace()!, new Patch(PatchType.JsonPatch, jsonPatch), cancellationToken).ConfigureAwait(false);
        return this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
    }

}
