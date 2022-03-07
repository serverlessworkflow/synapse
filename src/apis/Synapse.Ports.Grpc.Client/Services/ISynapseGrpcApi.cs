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
using Synapse.Integration.Commands.WorkflowInstances;
using Synapse.Integration.Commands.Workflows;
using Synapse.Integration.Models;
using Synapse.Ports.Grpc.Models;
using System.ServiceModel;

namespace Synapse.Ports.Grpc.Services
{

    /// <summary>
    /// Defines the GRPC port of the <see cref="ISynapseApiClient"/>
    /// </summary>
    [ServiceContract]
    public interface ISynapseGrpcApi
    {

        #region Workflows

        /// <summary>
        /// Creates a new workflow
        /// </summary>
        /// <param name="command">The object that describes the command to execute</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<V1GrpcApiResult<V1WorkflowDto>> CreateWorkflowAsync(V1CreateWorkflowCommandDto command, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the workflow with the specified id
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowDto"/> to get</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<V1GrpcApiResult<V1WorkflowDto>> GetWorkflowByIdAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Queries workflows
        /// </summary>
        /// <param name="query">The ODATA query to execute</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<V1GrpcApiResult<List<V1WorkflowDto>>> GetWorkflowsAsync(string? query = null, CancellationToken cancellationToken = default);


        /// <summary>
        /// Deletes the workflow with the specified id
        /// </summary>
        /// <param name="id">The id of the workflow to delete</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<V1GrpcApiResult> DeleteWorkflowAsync(string id, CancellationToken cancellationToken = default);

        #endregion

        #region WorkflowInstances

        /// <summary>
        /// Creates a new workflow instance
        /// </summary>
        /// <param name="command">The object that describes the command to execute</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<V1GrpcApiResult<V1WorkflowInstanceDto>> CreateWorkflowInstanceAsync(V1CreateWorkflowInstanceCommandDto command, CancellationToken cancellationToken = default);


        /// <summary>
        /// Starts a workflow instance
        /// </summary>
        /// <param name="id">The id of the workflow instance to start</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<V1GrpcApiResult<V1WorkflowInstanceDto>> StartWorkflowInstanceAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the workflow instance with the specified id
        /// </summary>
        /// <param name="id">The id of the workflow instance to get</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<V1GrpcApiResult<V1WorkflowInstanceDto>> GetWorkflowInstanceByIdAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Queries workflow instances
        /// </summary>
        /// <param name="query">The ODATA query to execute</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<V1GrpcApiResult<List<V1WorkflowInstanceDto>>> GetWorkflowInstancesAsync(string? query = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the workflow instance with the specified id
        /// </summary>
        /// <param name="id">The id of the workflow instance to delete</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<V1GrpcApiResult> DeleteWorkflowInstanceAsync(string id, CancellationToken cancellationToken = default);

        #endregion

    }

}
