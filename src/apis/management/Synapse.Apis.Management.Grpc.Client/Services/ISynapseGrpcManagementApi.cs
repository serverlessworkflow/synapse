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
using ProtoBuf.Grpc;
using Synapse.Integration.Commands.WorkflowInstances;
using Synapse.Integration.Commands.Workflows;

namespace Synapse.Apis.Management.Grpc
{

    /// <summary>
    /// Defines the GRPC port of the <see cref="ISynapseApiClient"/>
    /// </summary>
    [ServiceContract]
    public interface ISynapseGrpcManagementApi
    {

        #region Workflows

        /// <summary>
        /// Creates a new workflow
        /// </summary>
        /// <param name="command">The object that describes the command to execute</param>
        /// <param name="context">The current <see</param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult<V1Workflow>> CreateWorkflowAsync(V1CreateWorkflowCommand command, CallContext context = default);

        /// <summary>
        /// Gets the workflow with the specified id
        /// </summary>
        /// <param name="id">The id of the <see cref="V1Workflow"/> to get</param>
        /// <param name="cancellationToken">The current <see cref="CallContext" /></param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult<V1Workflow>> GetWorkflowByIdAsync(string id, CallContext context = default);

        /// <summary>
        /// Queries workflows
        /// </summary>
        /// <param name="query">The ODATA query to execute</param>
        /// <param name="context">The current <see cref="CallContext" /></param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult<List<V1Workflow>>> GetWorkflowsAsync(string? query = null, CallContext context = default);


        /// <summary>
        /// Deletes the workflow with the specified id
        /// </summary>
        /// <param name="id">The id of the workflow to delete</param>
        /// <param name="context">The current <see cref="CallContext" /></param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult> DeleteWorkflowAsync(string id, CallContext context = default);

        #endregion

        #region WorkflowInstances

        /// <summary>
        /// Creates a new workflow instance
        /// </summary>
        /// <param name="command">The object that describes the command to execute</param>
        /// <param name="context">The current <see cref="CallContext" /></param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult<V1WorkflowInstance>> CreateWorkflowInstanceAsync(V1CreateWorkflowInstanceCommand command, CallContext context = default);


        /// <summary>
        /// Starts a workflow instance
        /// </summary>
        /// <param name="id">The id of the workflow instance to start</param>
        /// <param name="context">The current <see cref="CallContext" /></param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult<V1WorkflowInstance>> StartWorkflowInstanceAsync(string id, CallContext context = default);

        /// <summary>
        /// Gets the workflow instance with the specified id
        /// </summary>
        /// <param name="id">The id of the workflow instance to get</param>
        /// <param name="context">The current <see cref="CallContext" /></param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult<V1WorkflowInstance>> GetWorkflowInstanceByIdAsync(string id, CallContext context = default);

        /// <summary>
        /// Queries workflow instances
        /// </summary>
        /// <param name="query">The ODATA query to execute</param>
        /// <param name="context">The current <see cref="CallContext" /></param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult<List<V1WorkflowInstance>>> GetWorkflowInstancesAsync(string? query = null, CallContext context = default);

        /// <summary>
        /// Deletes the workflow instance with the specified id
        /// </summary>
        /// <param name="id">The id of the workflow instance to delete</param>
        /// <param name="context">The current <see cref="CallContext" /></param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult> DeleteWorkflowInstanceAsync(string id, CallContext context = default);

        #endregion

    }

}
