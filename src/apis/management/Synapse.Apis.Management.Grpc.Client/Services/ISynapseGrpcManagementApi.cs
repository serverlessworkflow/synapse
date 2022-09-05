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
using Synapse.Integration.Commands.Correlations;
using Synapse.Integration.Commands.WorkflowInstances;
using Synapse.Integration.Commands.Workflows;

namespace Synapse.Apis.Management.Grpc
{

    /// <summary>
    /// Defines the GRPC port of the Synapse API client
    /// </summary>
    [ServiceContract]
    public interface ISynapseGrpcManagementApi
    {

        #region Workflows

        /// <summary>
        /// Creates a new workflow
        /// </summary>
        /// <param name="command">The object that describes the command to execute</param>
        /// <param name="context">The current</param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult<V1Workflow>> CreateWorkflowAsync(V1CreateWorkflowCommand command, CallContext context = default);

        /// <summary>
        /// Uploads a new workflow
        /// </summary>
        /// <param name="command">The object that describes the command to execute</param>
        /// <param name="context">A <see cref="CancellationToken"/></param>
        /// <returns>A new object that describes the result of the operation</returns>
        //[OperationContract] //todo: uncomment
        Task<GrpcApiResult<V1Workflow>> UploadWorkflowAsync(V1UploadWorkflowCommand command, CallContext context = default); //todo: implement

        /// <summary>
        /// Gets the workflow with the specified id
        /// </summary>
        /// <param name="id">The id of the <see cref="V1Workflow"/> to get</param>
        /// <param name="context">The current <see cref="CallContext" /></param>
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
        /// Gets the id of the <see cref="V1Workflow"/> to archive
        /// </summary>
        /// <param name="id">The id of the <see cref="V1Workflow"/> to archive</param>
        /// <param name="version">The version of the <see cref="V1Workflow"/> to archive</param>
        /// <param name="context">The current <see cref="CallContext" /></param>
        /// <returns>A new <see cref="Stream"/> containing the archived <see cref="V1WorkflowInstance"/></returns>
        [OperationContract]
        Task<GrpcApiResult<byte[]>> ArchiveWorkflowAsync(string id, string? version = null, CallContext context = default);

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
        /// Suspends the execution of the <see cref="V1WorkflowInstance"/> with the specified id
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowInstance"/> to suspend the exection of</param>
        /// <param name="context">The current <see cref="CallContext" /></param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult> SuspendWorkflowInstanceAsync(string id, CallContext context = default);

        /// <summary>
        /// Resumes the execution of the <see cref="V1WorkflowInstance"/> with the specified id
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowInstance"/> to resume the execution of</param>
        /// <param name="context">The current <see cref="CallContext" /></param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult> ResumeWorkflowInstanceAsync(string id, CallContext context = default);

        /// <summary>
        /// Cancels the execution of the <see cref="V1WorkflowInstance"/> with the specified id
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowInstance"/> to cancel the execution of</param>
        /// <param name="context">The current <see cref="CallContext" /></param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult> CancelWorkflowInstanceAsync(string id, CallContext context = default);

        /// <summary>
        /// Gets the logs of the <see cref="V1WorkflowInstance"/> with the specified id
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowInstance"/> to get the logs of</param>
        /// <param name="context">The current <see cref="CallContext" /></param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult<string>> GetWorkflowInstanceLogsAsync(string id, CallContext context = default);

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowInstance"/> to archive
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowInstance"/> to archive</param>
        /// <param name="context">The current <see cref="CallContext"/></param>
        /// <returns>A new <see cref="Stream"/> containing the archived <see cref="V1WorkflowInstance"/></returns>
        [OperationContract]
        Task<GrpcApiResult<byte[]>> ArchiveWorkflowInstanceAsync(string id, CallContext context = default);

        /// <summary>
        /// Deletes the workflow instance with the specified id
        /// </summary>
        /// <param name="id">The id of the workflow instance to delete</param>
        /// <param name="context">The current <see cref="CallContext" /></param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult> DeleteWorkflowInstanceAsync(string id, CallContext context = default);

        #endregion

        #region Correlations

        /// <summary>
        /// Creates a new <see cref="V1Correlation"/>
        /// </summary>
        /// <param name="command">The object that describes the command to execute</param>
        /// <param name="context">The current <see cref="CallContext"/></param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult<V1Correlation>> CreateCorrelationAsync(V1CreateCorrelationCommand command, CallContext context = default);

        /// <summary>
        /// Gets the <see cref="V1Correlation"/> with the specified id
        /// </summary>
        /// <param name="id">The id of the <see cref="V1Correlation"/> to get</param>
        /// <param name="context">The current <see cref="CallContext"/></param>
        /// <returns>A new object that describes the result of the operation</returns>

        [OperationContract]
        Task<GrpcApiResult<V1Correlation>> GetCorrelationByIdAsync(string id, CallContext context = default);

        /// <summary>
        /// Lists existing <see cref="V1Correlation"/>s
        /// </summary>
        /// <param name="query">The OData query string</param>
        /// <param name="context">The current <see cref="CallContext"/></param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult<List<V1Correlation>>> GetCorrelationsAsync(string query, CallContext context = default);

        /// <summary>
        /// Deletes the <see cref="V1Correlation"/> with the specified id
        /// </summary>
        /// <param name="id">The id of the <see cref="V1Correlation"/> to delete</param>
        /// <param name="context">The current <see cref="CallContext"/></param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult> DeleteCorrelationAsync(string id, CallContext context = default);

        #endregion

        #region OperationReports

        /// <summary>
        /// Gets the <see cref="V1OperationalReport"/>
        /// </summary>
        /// <param name="request">The date to get the report for. Defaults to today</param>
        /// <param name="context">The current <see cref="CallContext"/></param>
        /// <returns>The <see cref="V1OperationalReport"/></returns>
        [OperationContract]
        Task<GrpcApiResult<V1OperationalReport>> GetOperationalReportAsync(GrpcApiRequest<DateTime> request, CallContext context = default);

        #endregion

    }

}
