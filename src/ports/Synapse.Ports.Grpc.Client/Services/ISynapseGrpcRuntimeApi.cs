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
using Synapse.Integration.Commands.WorkflowActivities;
using Synapse.Integration.Commands.WorkflowInstances;
using Synapse.Integration.Models;
using Synapse.Integration.Queries.WorkflowActivities;
using Synapse.Integration.Services;
using Synapse.Ports.Grpc.Models;
using System.ServiceModel;

namespace Synapse.Ports.Grpc.Services
{

    /// <summary>
    /// Defines the GRPC port of the <see cref="ISynapseRuntimeApi"/>
    /// </summary>
    [ServiceContract]
    public interface ISynapseGrpcRuntimeApi
    {

        /// <summary>
        /// Starts the specified workflow instance
        /// </summary>
        /// <param name="workflowInstanceId">The id of the workflow instance to start</param>
        /// <param name="context">The current server call context</param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<V1GrpcApiResult<V1WorkflowInstanceDto>> StartAsync(string workflowInstanceId, CallContext context = default);

        /// <summary>
        /// Gets the activities (including non-operative ones) of the specified workflow instance
        /// </summary>
        /// <param name="workflowInstanceId">The id of the workflow instance to get the activities of</param>
        /// <param name="context">The current server call context</param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<V1GrpcApiResult<List<V1WorkflowActivityDto>>> GetActivitiesAsync(V1GetWorkflowActivitiesQueryDto query, CallContext context = default);

        /// <summary>
        /// Creates a new workflow activity
        /// </summary>
        /// <param name="command">An object that describes the command to execute</param>
        /// <param name="context">The current server call context</param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<V1GrpcApiResult<V1WorkflowActivityDto>> CreateActivityAsync(V1CreateWorkflowActivityCommandDto command, CallContext context = default);

        /// <summary>
        /// Starts the specified workflow activity
        /// </summary>
        /// <param name="activityId">The id of the workflow activity to start</param>
        /// <param name="context">The current server call context</param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<V1GrpcApiResult<V1WorkflowActivityDto>> StartActivityAsync(string activityId, CallContext context = default);

        /// <summary>
        /// Suspends the execution of the specified workflow activity
        /// </summary>
        /// <param name="activityId">The id of the workflow activity to suspend the execution of</param>
        /// <param name="context">The current server call context</param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<V1GrpcApiResult<V1WorkflowActivityDto>> SuspendActivityAsync(string activityId, CallContext context = default);

        /// <summary>
        /// Skips the specified activity
        /// </summary>
        /// <param name="activityId">The workflow activity to skip</param>
        /// <param name="context">The current server call context</param>
        /// <returns>The skipped activity</returns>
        [OperationContract]
        Task<V1GrpcApiResult<V1WorkflowActivityDto>> SkipActivityAsync(string activityId, CallContext context = default);

        /// <summary>
        /// Faults the specified workflow activity
        /// </summary>
        /// <param name="command">An object that describes the command to execute</param>
        /// <param name="context">The current server call context</param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<V1GrpcApiResult<V1WorkflowActivityDto>> FaultActivityAsync(V1FaultWorkflowActivityCommandDto command, CallContext context = default);

        /// <summary>
        /// Cancels the specified workflow activity
        /// </summary>
        /// <param name="activityId">The id of the workflow activity to cancel</param>
        /// <param name="context">The current server call context</param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<V1GrpcApiResult<V1WorkflowActivityDto>> CancelActivityAsync(string activityId, CallContext context = default);

        /// <summary>
        /// Completes and sets the output of the specified workflow activity
        /// </summary>
        /// <param name="command">The id of the workflow activity to complete and set the output of</param>
        /// <param name="context">The current server call context</param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<V1GrpcApiResult<V1WorkflowActivityDto>> SetActivityOutputAsync(V1SetWorkflowActivityOutputCommandDto command, CallContext context = default);

        /// <summary>
        /// Faults the specified workflow instance
        /// </summary>
        /// <param name="command">An object that describes the command to execute</param>
        /// <param name="context">The current server call context</param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<V1GrpcApiResult<V1WorkflowInstanceDto>> FaultAsync(V1FaultWorkflowInstanceCommandDto command, CallContext context = default);

        /// <summary>
        /// Cancels the execution of the specified workflow instance
        /// </summary>
        /// <param name="workflowInstanceId">The id of the workflow instance to cancel the execution of</param>
        /// <param name="context">The current server call context</param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<V1GrpcApiResult<V1WorkflowInstanceDto>> CancelAsync(string workflowInstanceId, CallContext context = default);

        /// <summary>
        /// Completes and sets the output of the specified workflow instance
        /// </summary>
        /// <param name="command">The id of the workflow instance to complete and set the output of</param>
        /// <param name="context">The current server call context</param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<V1GrpcApiResult<V1WorkflowInstanceDto>> SetOutputAsync(V1SetWorkflowInstanceOutputCommandDto command, CallContext context = default);

    }

}
