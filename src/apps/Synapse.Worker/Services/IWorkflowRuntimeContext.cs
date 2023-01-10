/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

namespace Synapse.Worker.Services
{

    /// <summary>
    /// Defines the fundamentals of a service used to represent a workflow's runtime context
    /// </summary>
    public interface IWorkflowRuntimeContext
    {

        /// <summary>
        /// Gets the service used a Synapse API helper facade
        /// </summary>
        IWorkflowFacade Workflow { get; }

        /// <summary>
        /// Gets an <see cref="IDictionary{TKey, TValue}"/> containing temporary runtime data that will not be persisted
        /// </summary>
        IDictionary<string, object> Data { get; }

        /// <summary>
        /// Initializes the <see cref="IWorkflowRuntimeContext"/>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task InitializeAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Evaluates a runtime expression against an object
        /// </summary>
        /// <param name="runtimeExpression">The runtime expression to evaluate</param>
        /// <param name="data">The data to evaluate the expression against</param>
        /// <param name="authorization">The current <see cref="AuthorizationInfo"/>, if any</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The evaluation result</returns>
        Task<object?> EvaluateAsync(string runtimeExpression, object? data, AuthorizationInfo? authorization, CancellationToken cancellationToken = default);

        /// <summary>
        /// Evaluates a runtime expression against an object
        /// </summary>
        /// <param name="runtimeExpression">The runtime expression to evaluate</param>
        /// <param name="data">The data to evaluate the expression against</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The evaluation result</returns>
        Task<object?> EvaluateAsync(string runtimeExpression, object? data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Reads the secret with the specified name
        /// </summary>
        /// <typeparam name="T">The type of the secret to get</typeparam>
        /// <param name="secret">The name of the secret to get</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The secret with the specified name</returns>
        Task<T> GetSecretAsync<T>(string secret, CancellationToken cancellationToken = default);

        /// <summary>
        /// Publishes the specified <see cref="V1Event"/>
        /// </summary>
        /// <param name="e">The <see cref="V1Event"/> to publish</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task PublishEventAsync(V1Event e, CancellationToken cancellationToken = default);

    }

}
