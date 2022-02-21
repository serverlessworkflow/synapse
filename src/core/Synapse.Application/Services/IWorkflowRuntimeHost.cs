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

namespace Synapse.Application.Services
{

    /// <summary>
    /// Defines the fundamentals of a service used to host workflow runtimes
    /// </summary>
    public interface IWorkflowRuntimeHost
        : IDisposable, IAsyncDisposable
    {

        /// <summary>
        /// Schedules the execution of the specified <see cref="V1WorkflowInstance"/>
        /// </summary>
        /// <param name="workflowInstance">The <see cref="V1WorkflowInstance"/> to schedule the execution of</param>
        /// <param name="at">The date and time for which to schedule the <see cref="V1WorkflowInstance"/></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>An id used to identify the <see cref="V1WorkflowInstance"/>'s runtime</returns>
        Task<string> ScheduleAsync(V1WorkflowInstance workflowInstance, DateTimeOffset at, CancellationToken cancellationToken = default);

        /// <summary>
        /// Starts the execution of the specified <see cref="V1WorkflowInstance"/>
        /// </summary>
        /// <param name="workflowInstance">The <see cref="V1WorkflowInstance"/> to start the execution of</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>An id used to identify the <see cref="V1WorkflowInstance"/>'s runtime</returns>
        Task<string> StartAsync(V1WorkflowInstance workflowInstance, CancellationToken cancellationToken = default);

    }

}
