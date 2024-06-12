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

namespace Synapse.Runner.Services;

/// <summary>
/// Represents the default implementation of the <see cref="ITaskExecutionContext"/> interface
/// </summary>
/// <typeparam name="TDefinition">The type of the <see cref="TaskDefinition"/> of the <see cref="TaskInstance"/> to execute</typeparam>
/// <param name="workflow">The <see cref="IWorkflowExecutionContext"/> the <see cref="ITaskExecutionContext"/> belongs to</param>
/// <param name="instance">The <see cref="TaskInstance"/> to run</param>
/// <param name="definition">The definition of the <see cref="TaskInstance"/> to run</param>
/// <param name="input">The input of the <see cref="TaskInstance"/> to run</param>
/// <param name="contextData">A name/value mapping of the current context data</param>
/// <param name="arguments">A name/value mapping of the task's arguments</param>
public class TaskExecutionContext<TDefinition>(IWorkflowExecutionContext workflow, TaskInstance instance, TDefinition definition, object input, IDictionary<string, object> contextData, IDictionary<string, object> arguments)
    : ITaskExecutionContext<TDefinition>
    where TDefinition : TaskDefinition
{

    /// <inheritdoc/>
    public virtual IWorkflowExecutionContext Workflow => workflow;

    /// <inheritdoc/>
    public virtual TDefinition Definition => definition;

    /// <inheritdoc/>
    public virtual TaskInstance Instance { get; set; } = instance;

    /// <inheritdoc/>
    public virtual object Input => input;

    /// <inheritdoc/>
    public virtual IDictionary<string, object> ContextData { get; protected set; } = contextData;

    /// <inheritdoc/>
    public virtual IDictionary<string, object> Arguments { get; protected set; } = arguments;

    /// <inheritdoc/>
    public virtual object? Output { get; protected set; }

    /// <inheritdoc/>
    TaskDefinition ITaskExecutionContext.Definition => this.Definition;

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TaskInstance> GetSubTasksAsync(CancellationToken cancellationToken = default) => this.Workflow.GetTasksAsync(this.Instance, cancellationToken);

    /// <inheritdoc/>
    public virtual async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        this.Instance = await this.Workflow.InitializeAsync(this.Instance, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        this.Instance = await this.Workflow.StartAsync(this.Instance, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual Task<CorrelationContext> CorrelateAsync(CancellationToken cancellationToken = default) => this.Workflow.CorrelateAsync(this, cancellationToken);

    /// <inheritdoc/>
    public virtual async Task SkipAsync(CancellationToken cancellationToken = default)
    {
        this.Instance = await this.Workflow.SkipAsync(this.Instance, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task SuspendAsync(CancellationToken cancellationToken = default)
    {
        this.Instance = await this.Workflow.SuspendAsync(this.Instance, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task RetryAsync(Error cause, CancellationToken cancellationToken = default)
    {
        this.Instance = await this.Workflow.RetryAsync(this.Instance, cause, cancellationToken).ConfigureAwait(false);
    } 

    /// <inheritdoc/>
    public virtual async Task SetErrorAsync(Error error, CancellationToken cancellationToken = default)
    {
        this.Instance = await this.Workflow.SetErrorAsync(this.Instance, error, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task SetResultAsync(object? result, string? then = FlowDirective.Continue, CancellationToken cancellationToken = default)
    {
        this.Output = result;
        this.Instance = await this.Workflow.SetResultAsync(this.Instance, this.Output, then, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task SetContextDataAsync(IDictionary<string, object> context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (string.IsNullOrWhiteSpace(this.Instance.ContextReference)) throw new NullReferenceException($"The context reference of the task '{this.Instance.Reference}' must be set");
        await this.Workflow.SetWorkflowDataAsync(this.Instance.ContextReference, context, cancellationToken).ConfigureAwait(false);
        this.ContextData = context;
    }

    /// <inheritdoc/>
    public virtual async Task CancelAsync(CancellationToken cancellationToken = default)
    {
        this.Instance = await this.Workflow.CancelAsync(this.Instance, cancellationToken);
    }

}
