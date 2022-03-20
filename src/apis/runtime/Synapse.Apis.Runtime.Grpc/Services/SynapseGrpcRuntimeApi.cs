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

using Neuroglia;
using Neuroglia.Mapping;
using Neuroglia.Mediation;
using Neuroglia.Serialization;
using ProtoBuf.Grpc;
using Synapse.Apis.Runtime.Grpc.Models;
using Synapse.Application.Services;
using Synapse.Integration.Commands.WorkflowInstances;
using Synapse.Integration.Models;
using System.Threading.Channels;

namespace Synapse.Apis.Runtime.Grpc
{

    /// <summary>
    /// Represents the default implementation of the <see cref="ISynapseGrpcRuntimeApi"/> interface
    /// </summary>
    public class SynapseGrpcRuntimeApi
        : ISynapseGrpcRuntimeApi
    {

        /// <inheritdoc/>
        public SynapseGrpcRuntimeApi(IMediator mediator, IMapper mapper, IWorkflowRuntimeProxyFactory runtimeProxyFactory, IWorkflowRuntimeProxyManager runtimeProxyManager)
        {
            Mediator = mediator;
            Mapper = mapper;
            RuntimeProxyFactory = runtimeProxyFactory;
            RuntimeProxyManager = runtimeProxyManager;
        }

        /// <summary>
        /// Gets the service used to mediate calls
        /// </summary>
        protected IMediator Mediator { get; }

        /// <summary>
        /// Gets the service used to map objects
        /// </summary>
        protected IMapper Mapper { get; }

        /// <summary>
        /// Gets the service used to create <see cref="IWorkflowRuntimeProxy"/> instances
        /// </summary>
        protected IWorkflowRuntimeProxyFactory RuntimeProxyFactory { get; }

        /// <summary>
        /// Gets the service used to manage <see cref="IWorkflowRuntimeProxy"/>
        /// </summary>
        protected IWorkflowRuntimeProxyManager RuntimeProxyManager { get; }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<V1RuntimeSignal> Connect(string runtimeId, CallContext context = default)
        {
            var stream = Channel.CreateUnbounded<V1RuntimeSignal>();
            var streamWriter = new AsyncStreamWriter<V1RuntimeSignal>(stream.Writer);
            var runtime = RuntimeProxyManager.Register(RuntimeProxyFactory.CreateProxy(runtimeId, streamWriter));
            await foreach (var message in stream.Reader.ReadAllAsync(context.CancellationToken))
            {
                yield return message;
            }
            runtime.Dispose();
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult<V1WorkflowInstance>> StartAsync(string workflowInstanceId, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new Application.Commands.WorkflowInstances.V1MarkWorkflowInstanceAsStartedCommand(workflowInstanceId), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult<V1Event?>> ConsumeOrBeginCorrelateEventAsync(V1ConsumeWorkflowInstancePendingEventCommand command, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(Mapper.Map<Application.Commands.WorkflowInstances.V1ConsumeOrBeginCorrelateEventCommand>(command), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult<bool>> TryCorrelateAsync(V1TryCorrelateWorkflowInstanceCommand command, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(Mapper.Map<Application.Commands.WorkflowInstances.V1TryCorrelateWorkflowInstanceCommand>(command), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult> SetCorrelationMappingAsync(V1SetWorkflowInstanceCorrelationMappingCommand command, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(Mapper.Map<Application.Commands.WorkflowInstances.V1SetWorkflowInstanceCorrelationMappingCommand>(command), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult<V1WorkflowActivity>> CreateActivityAsync(Integration.Commands.WorkflowActivities.V1CreateWorkflowActivityCommand command, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(Mapper.Map<Application.Commands.WorkflowActivities.V1CreateWorkflowActivityCommand>(command), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult<List<V1WorkflowActivity>>> GetActivitiesAsync(Integration.Queries.WorkflowActivities.V1GetWorkflowActivitiesQuery query, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(Mapper.Map<Application.Queries.WorkflowActivities.V1GetWorkflowActivitiesQuery>(query), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult<V1WorkflowActivity>> StartActivityAsync(string activityId, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new Application.Commands.WorkflowActivities.V1StartWorkflowActivityCommand(activityId), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult<V1WorkflowActivity>> SuspendActivityAsync(string activityId, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new Application.Commands.WorkflowActivities.V1SuspendWorkflowActivityCommand(activityId), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult<V1WorkflowActivity>> SkipActivityAsync(string activityId, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new Application.Commands.WorkflowActivities.V1SkipWorkflowActivityCommand(activityId), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult<V1WorkflowActivity>> FaultActivityAsync(Integration.Commands.WorkflowActivities.V1FaultWorkflowActivityCommand command, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(Mapper.Map<Application.Commands.WorkflowActivities.V1FaultWorkflowActivityCommand>(command), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult<V1WorkflowActivity>> CancelActivityAsync(string activityId, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new Application.Commands.WorkflowActivities.V1CancelWorkflowActivityCommand(activityId), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult<V1WorkflowActivity>> SetActivityOutputAsync(Integration.Commands.WorkflowActivities.V1SetWorkflowActivityOutputCommand command, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(Mapper.Map<Application.Commands.WorkflowActivities.V1SetWorkflowActivityOutputCommand>(command), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult<Dynamic>> GetActivityStateDataAsync(string activityId, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new Application.Queries.WorkflowActivities.V1GetActivityParentStateDataQuery(activityId), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult<V1WorkflowInstance>> FaultAsync(Integration.Commands.WorkflowInstances.V1FaultWorkflowInstanceCommand command, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(Mapper.Map<Application.Commands.WorkflowInstances.V1FaultWorkflowInstanceCommand>(command), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult<V1WorkflowInstance>> CancelAsync(string workflowInstanceId, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new Application.Commands.WorkflowInstances.V1CancelWorkflowInstanceCommand(workflowInstanceId), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult<V1WorkflowInstance>> SetOutputAsync(Integration.Commands.WorkflowInstances.V1SetWorkflowInstanceOutputCommand command, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(Mapper.Map<Application.Commands.WorkflowInstances.V1SetWorkflowInstanceOutputCommand>(command), context.CancellationToken));
        }

        public Task<GrpcApiResult<V1WorkflowInstance>> SuspendAsync(string workflowInstanceId, CallContext context = default)
        {
            throw new NotImplementedException();
        }

    }

}
