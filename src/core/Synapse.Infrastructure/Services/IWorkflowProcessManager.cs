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

using Microsoft.Extensions.Hosting;

namespace Synapse.Infrastructure.Services
{

    /// <summary>
    /// Defines the fundamentals of a service used to manage <see cref="IWorkflowProcess"/>es
    /// </summary>
    public interface IWorkflowProcessManager
        : IHostedService, IDisposable
    {

        /// <summary>
        /// Starts a new <see cref="IWorkflowProcess"/> for the specified <see cref="V1WorkflowInstance"/>
        /// </summary>
        /// <param name="workflow">The instanciated <see cref="V1Workflow"/> to start a new <see cref="IWorkflowProcess"/> for</param>
        /// <param name="workflowInstance">The <see cref="V1WorkflowInstance"/> to start a new <see cref="IWorkflowProcess"/> for</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IWorkflowProcess"/></returns>
        Task<IWorkflowProcess> StartProcessAsync(V1Workflow workflow, V1WorkflowInstance workflowInstance, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the <see cref="IWorkflowProcess"/> with the specified id
        /// </summary>
        /// <param name="id">The id of the <see cref="IWorkflowProcess"/> to get</param>
        /// <returns>The <see cref="IWorkflowProcess"/> with the specified id</returns>
        IWorkflowProcess GetProcessById(string id);
    
    }

}
