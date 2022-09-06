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

using Neuroglia.Serialization;
using ProtoBuf.Grpc;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Apis.Runtime.Grpc.Models;
using Synapse.Integration.Commands.WorkflowActivities;
using Synapse.Integration.Commands.WorkflowInstances;
using Synapse.Integration.Models;
using Synapse.Integration.Queries.WorkflowActivities;
using System.ServiceModel;

namespace Synapse.Apis.Runtime.Grpc
{

    /// <summary>
    /// Defines the fundamentals of the GRPC-based <see cref="ISynapseRuntimeApi"/>
    /// </summary>
    [ServiceContract]
    public interface ISynapseGrpcRuntimeApi
    {

        /// <summary>
        /// Connects to the Synapse Runtime API
        /// </summary>
        /// <param name="workflowInstanceId">The id of the workflow instance id the client runtime executes</param>
        /// <param name="context">The current <see cref="CallContext"/></param>
        /// <returns>A new <see cref="IAsyncEnumerable{T}"/></returns>
        [OperationContract]
        IAsyncEnumerable<V1RuntimeSignal> Connect(string workflowInstanceId, CallContext context = default);

        /// <summary>
        /// Starts the specified workflow instance
        /// </summary>
        /// <param name="workflowInstanceId">The id of the workflow instance to start</param>
        /// <param name="context">The current server call context</param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult<V1WorkflowInstance>> StartAsync(string workflowInstanceId, CallContext context = default);

        /// <summary>
        /// Attempts to consume a pending event
        /// </summary>
        /// <param name="command">The object that describes the command to execute</param>
        /// <param name="context">The current <see cref="CallContext"/></param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult<V1Event?>> ConsumeOrBeginCorrelateEventAsync(V1ConsumeWorkflowInstancePendingEventCommand command, CallContext context = default);

        /// <summary>
        /// Attempts to correlate an event to a workflow instance
        /// </summary>
        /// <param name="command">The object that describes the command to execute</param>
        /// <param name="context">The current <see cref="CallContext"/></param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult<bool>> TryCorrelateAsync(V1TryCorrelateWorkflowInstanceCommand command, CallContext context = default);

        /// <summary>
        /// Sets a correlation mapping for the specified workflow instance
        /// </summary>
        /// <param name="command">The object that describes the command to execute</param>
        /// <param name="context">The current <see cref="CallContext"/></param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult<V1WorkflowInstance>> SetCorrelationMappingAsync(V1SetWorkflowInstanceCorrelationMappingCommand command, CallContext context = default);

        /// <summary>
        /// Gets the activities (including non-operative ones) of the specified workflow instance
        /// </summary>
        /// <param name="query">The id of the workflow instance to get the activities of</param>
        /// <param name="context">The current server call context</param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult<List<V1WorkflowActivity>>> GetActivitiesAsync(V1GetWorkflowActivitiesQuery query, CallContext context = default);

        /// <summary>
        /// Creates a new workflow activity
        /// </summary>
        /// <param name="command">An object that describes the command to execute</param>
        /// <param name="context">The current server call context</param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult<V1WorkflowActivity>> CreateActivityAsync(V1CreateWorkflowActivityCommand command, CallContext context = default);

        /// <summary>
        /// Starts the specified workflow activity
        /// </summary>
        /// <param name="activityId">The id of the workflow activity to start</param>
        /// <param name="context">The current server call context</param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult<V1WorkflowActivity>> StartActivityAsync(string activityId, CallContext context = default);

        /// <summary>
        /// Suspends the execution of the specified workflow activity
        /// </summary>
        /// <param name="activityId">The id of the workflow activity to suspend the execution of</param>
        /// <param name="context">The current server call context</param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult<V1WorkflowActivity>> SuspendActivityAsync(string activityId, CallContext context = default);

        /// <summary>
        /// Skips the specified activity
        /// </summary>
        /// <param name="activityId">The workflow activity to skip</param>
        /// <param name="context">The current server call context</param>
        /// <returns>The skipped activity</returns>
        [OperationContract]
        Task<GrpcApiResult<V1WorkflowActivity>> SkipActivityAsync(string activityId, CallContext context = default);

        /// <summary>
        /// Sets the specified <see cref="V1WorkflowActivity"/>'s metadata
        /// </summary>
        /// <param name="command">The object that describes the command to execute</param>
        /// <param name="context">The current server call context</param>
        /// <returns>The updated <see cref="V1WorkflowActivity"/></returns>
        [OperationContract]
        Task<GrpcApiResult<V1WorkflowActivity>> SetActivityMetadataAsync(V1SetWorkflowActivityMetadataCommand command, CallContext context = default);

        /// <summary>
        /// Faults the specified workflow activity
        /// </summary>
        /// <param name="command">An object that describes the command to execute</param>
        /// <param name="context">The current server call context</param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult<V1WorkflowActivity>> FaultActivityAsync(V1FaultWorkflowActivityCommand command, CallContext context = default);

        /// <summary>
        /// Compensates the specified workflow activity
        /// </summary>
        /// <param name="command">An object that describes the command to execute</param>
        /// <param name="context">The current server call context</param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult<V1WorkflowActivity>> CompensateActivityAsync(V1CompensateActivityCommand command, CallContext context = default);

        /// <summary>
        /// Marks the specified workflow activity as compensated
        /// </summary>
        /// <param name="command">An object that describes the command to execute</param>
        /// <param name="context">The current server call context</param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult<V1WorkflowActivity>> MarkActivityAsCompensatedAsync(V1MarkActivityAsCompensatedCommand command, CallContext context = default);

        /// <summary>
        /// Cancels the specified workflow activity
        /// </summary>
        /// <param name="activityId">The id of the workflow activity to cancel</param>
        /// <param name="context">The current server call context</param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult<V1WorkflowActivity>> CancelActivityAsync(string activityId, CallContext context = default);

        /// <summary>
        /// Completes and sets the output of the specified workflow activity
        /// </summary>
        /// <param name="command">The id of the workflow activity to complete and set the output of</param>
        /// <param name="context">The current server call context</param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult<V1WorkflowActivity>> SetActivityOutputAsync(V1SetWorkflowActivityOutputCommand command, CallContext context = default);

        /// <summary>
        /// Gets the data of the <see cref="StateDefinition"/> the specified <see cref="V1WorkflowActivity"/> belongs to
        /// </summary>
        /// <param name="activityId">The id of the activity to get the parent state's data for</param>
        /// <param name="context">The current server call context</param>
        /// <returns>The data of the <see cref="StateDefinition"/> the specified <see cref="V1WorkflowActivity"/> belongs to</returns>
        [OperationContract]
        Task<GrpcApiResult<Dynamic>> GetActivityStateDataAsync(string activityId, CallContext context = default);

        /// <summary>
        /// Starts a subflow
        /// </summary>
        /// <param name="command">The object that describes the command to execute</param>
        /// <param name="context">The current server call context</param>
        /// <returns>The newly created workflow instance</returns>
        [OperationContract]
        Task<GrpcApiResult<V1WorkflowInstance>> StartSubflowAsync(V1CreateWorkflowInstanceCommand command, CallContext context = default);

        /// <summary>
        /// Faults the specified workflow instance
        /// </summary>
        /// <param name="command">An object that describes the command to execute</param>
        /// <param name="context">The current server call context</param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult<V1WorkflowInstance>> FaultAsync(V1FaultWorkflowInstanceCommand command, CallContext context = default);

        /// <summary>
        /// Suspends the execution of the specified workflow instance
        /// </summary>
        /// <param name="workflowInstanceId">The id of the workflow instance to suspend the execution of</param>
        /// <param name="context">The current server call context</param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult<V1WorkflowInstance>> SuspendAsync(string workflowInstanceId, CallContext context = default);

        /// <summary>
        /// Cancels the execution of the specified workflow instance
        /// </summary>
        /// <param name="workflowInstanceId">The id of the workflow instance to cancel the execution of</param>
        /// <param name="context">The current server call context</param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult<V1WorkflowInstance>> CancelAsync(string workflowInstanceId, CallContext context = default);

        /// <summary>
        /// Completes and sets the output of the specified workflow instance
        /// </summary>
        /// <param name="command">The id of the workflow instance to complete and set the output of</param>
        /// <param name="context">The current server call context</param>
        /// <returns>A new object that describes the result of the operation</returns>
        [OperationContract]
        Task<GrpcApiResult<V1WorkflowInstance>> SetOutputAsync(V1SetWorkflowInstanceOutputCommand command, CallContext context = default);


    }

}
