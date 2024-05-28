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

using Neuroglia.Data.Expressions.Services;
using Neuroglia.Eventing.CloudEvents;
using Synapse.Api.Client.Services;

namespace Synapse.Runner.Services;

/// <summary>
/// Defines the fundamentals of the context of a <see cref="WorkflowInstance"/>'s execution
/// </summary>
public interface IWorkflowExecutionContext
{

    /// <summary>
    /// Gets the current <see cref="WorkflowInstance"/>
    /// </summary>
    WorkflowInstance Instance { get; }

    /// <summary>
    /// Gets the <see cref="WorkflowDefinition"/> of the current <see cref="WorkflowInstance"/>
    /// </summary>
    WorkflowDefinition Definition { get; }

    /// <summary>
    /// Gets the current <see cref="IServiceProvider"/>
    /// </summary>
    IServiceProvider Services { get; }

    /// <summary>
    /// Gets the current <see cref="IExpressionEvaluator"/>
    /// </summary>
    IExpressionEvaluator Expressions { get; }

    /// <summary>
    /// Gets the Synapse API used to manage <see cref="Document"/>s
    /// </summary>
    IDocumentApiClient Documents { get; }

    /// <summary>
    /// Gets/sets the workflow's context data
    /// </summary>
    IDictionary<string, object> ContextData { get; }

    /// <summary>
    /// Gets/sets the workflow's arguments
    /// </summary>
    IDictionary<string, object> Arguments { get; }

    /// <summary>
    /// Gets/sets the workflow's output data, if any, in case the workflow ran to completion
    /// </summary>
    object? Output { get; }

    /// <summary>
    /// Continues execution with the provided <see cref="TaskDefinition"/>
    /// </summary>
    /// <param name="task">The <see cref="TaskDefinition"/> to continue with</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task ContinueWith(TaskDefinition task, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new <see cref="TaskInstance"/>
    /// </summary>
    /// <param name="definition">The <see cref="TaskDefinition"/> of the <see cref="TaskInstance"/> to create</param>
    /// <param name="path">The path used to reference the <see cref="TaskDefinition"/> of the <see cref="TaskInstance"/> to create</param>
    /// <param name="input">The input data, if any</param>
    /// <param name="parent">The parent of the <see cref="TaskInstance"/> to create, if any</param>
    /// <param name="isExtension">Indicates whether or not the task is part of an extension</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The updated <see cref="TaskInstance"/></returns>
    Task<TaskInstance> CreateTaskAsync(TaskDefinition definition, string path, object input, ITaskExecutionContext? parent = null, bool isExtension = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the workflow's tasks
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IAsyncEnumerable{T}"/> to asynchronously enumerate the tasks the workflow owns</returns>
    IAsyncEnumerable<TaskInstance> GetTasksAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the subtasks the specified task is made out of
    /// </summary>
    /// <param name="task">The <see cref="TaskInstance"/> to enumerate the subtasks of</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IAsyncEnumerable{T}"/> to asynchronously enumerate the subtasks the specified task is made out of</returns>
    IAsyncEnumerable<TaskInstance> GetTasksAsync(TaskInstance task, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initializes the workflow
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Initializes the specified task
    /// </summary>
    /// <param name="task">The task to initialize</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The updated <see cref="TaskInstance"/></returns>
    Task<TaskInstance> InitializeAsync(TaskInstance task, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the workflow
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task ExecuteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the specified task
    /// </summary>
    /// <param name="task">The task to execute</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The updated <see cref="TaskInstance"/></returns>
    Task<TaskInstance> ExecuteAsync(TaskInstance task, CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts consuming, or begins correlating, the defined events
    /// </summary>
    /// <param name="listener">The definition of the event subscription to create</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IEnumerable{T}"/> containing all consumable <see cref="CloudEvent"/>s, if any</returns>
    Task<IEnumerable<CloudEvent>> ConsumeOrBeginCorrelateAsync(ListenerDefinition listener, CancellationToken cancellationToken = default);

    /// <summary>
    /// Suspends the workflow
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task SuspendAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Suspends the specified task
    /// </summary>
    /// <param name="task">The task to suspend</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The updated <see cref="TaskInstance"/></returns>
    Task<TaskInstance> SuspendAsync(TaskInstance task, CancellationToken cancellationToken = default);

    /// <summary>
    /// Skips the specified task
    /// </summary>
    /// <param name="task">The task to skip</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The updated <see cref="TaskInstance"/></returns>
    Task<TaskInstance> SkipAsync(TaskInstance task, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retries the specified task
    /// </summary>
    /// <param name="task">The task to retry</param>
    /// <param name="cause">The <see cref="Error"/> that caused the retry attempt</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The updated <see cref="TaskInstance"/></returns>
    Task<TaskInstance> RetryAsync(TaskInstance task, Error cause, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the error that has faulted the workflow's execution
    /// </summary>
    /// <param name="error">The <see cref="Error"/> that has faulted the workflow</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task SetErrorAsync(Error error, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the error that has faulted the specified task's execution
    /// </summary>
    /// <param name="task">The task to set the error for</param>
    /// <param name="error">The <see cref="Error"/> that has faulted the task</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The updated <see cref="TaskInstance"/></returns>
    Task<TaskInstance> SetErrorAsync(TaskInstance task, Error error, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the workflow's result
    /// </summary>
    /// <param name="result">The workflow's result, if any</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task SetResultAsync(object? result, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the specified task's result
    /// </summary>
    /// <param name="task">The task to set the result of</param>
    /// <param name="result">The task's result, if any</param>
    /// <param name="then">The <see cref="FlowDirective"/> to perform next</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The updated <see cref="TaskInstance"/></returns>
    Task<TaskInstance> SetResultAsync(TaskInstance task, object? result, string? then = FlowDirective.Continue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels the workflow's execution
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task CancelAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels the execution of the specified task
    /// </summary>
    /// <param name="task">The task to cancel the execution of</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The updated <see cref="TaskInstance"/></returns>
    Task<TaskInstance> CancelAsync(TaskInstance task, CancellationToken cancellationToken = default);

}