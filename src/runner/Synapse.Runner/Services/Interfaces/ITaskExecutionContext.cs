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
/// Defines the fundamentals of the context of a <see cref="TaskInstance"/>'s execution
/// </summary>
public interface ITaskExecutionContext
{

    /// <summary>
    /// Gets the workflow the task to execute belongs to
    /// </summary>
    IWorkflowExecutionContext Workflow { get; }

    /// <summary>
    /// Gets the <see cref="TaskDefinition"/> of the <see cref="TaskInstance"/> to execute
    /// </summary>
    TaskDefinition Definition { get; }

    /// <summary>
    /// Gets the <see cref="TaskInstance"/> to execute
    /// </summary>
    TaskInstance Instance { get; }

    /// <summary>
    /// Gets/sets the task's input data
    /// </summary>
    object Input { get; }

    /// <summary>
    /// Gets/sets the task's context data, if any
    /// </summary>
    IDictionary<string, object> ContextData { get; }

    /// <summary>
    /// Gets/sets the task's arguments
    /// </summary>
    IDictionary<string, object> Arguments { get; }

    /// <summary>
    /// Gets/sets the task's output data, if any, in case the task ran to completion
    /// </summary>
    object? Output { get; }

    /// <summary>
    /// Initializes the <see cref="TaskInstance"/>
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the <see cref="TaskInstance"/>
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task ExecuteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins correlating events
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The resulting <see cref="CorrelationContext"/></returns>
    Task<CorrelationContext> CorrelateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Suspends the <see cref="TaskInstance"/>
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task SuspendAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Skips the <see cref="TaskInstance"/>
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task SkipAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retries the <see cref="TaskInstance"/>
    /// </summary>
    /// <param name="cause">The <see cref="Error"/> to retry the <see cref="TaskInstance"/> for</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task RetryAsync(Error cause, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets an <see cref="Error"/> that has occurred during the <see cref="TaskInstance"/>'s execution
    /// </summary>
    /// <param name="error">The <see cref="Error"/> that has occurred</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task SetErrorAsync(Error error, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the task's context data
    /// </summary>
    /// <param name="context">The updated context data</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task SetContextDataAsync(IDictionary<string, object> context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the <see cref="TaskInstance"/>'s result, if any
    /// </summary>
    /// <param name="result">The <see cref="TaskInstance"/>'s result, if any</param>
    /// <param name="then">The <see cref="FlowDirective"/> to perform next</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task SetResultAsync(object? result, string? then = FlowDirective.Continue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels the <see cref="TaskInstance"/>
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task CancelAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the <see cref="TaskInstance">subtasks</see> the <see cref="TaskInstance"/> is made out of
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IAsyncEnumerable{T}"/> used to enumerate <see cref="TaskInstance">subtasks</see></returns>
    IAsyncEnumerable<TaskInstance> GetSubTasksAsync(CancellationToken cancellationToken = default);

}

/// <summary>
/// Defines the fundamentals of the context of a <see cref="TaskInstance"/>'s execution
/// </summary>
/// <typeparam name="TDefinition">The type of <see cref="TaskInstance"/> to run</typeparam>
public interface ITaskExecutionContext<TDefinition>
    : ITaskExecutionContext
    where TDefinition : TaskDefinition
{

    /// <summary>
    /// Gets the <see cref="TaskDefinition"/> of the <see cref="TaskInstance"/> to execute
    /// </summary>
    new TDefinition Definition { get; }

}
