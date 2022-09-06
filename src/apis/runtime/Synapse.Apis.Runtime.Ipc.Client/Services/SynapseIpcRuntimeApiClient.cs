using Microsoft.Extensions.Logging;
using Neuroglia;
using Neuroglia.Mapping;
using Neuroglia.Mediation;
using Neuroglia.Serialization;
using Synapse.Infrastructure.Services;
using Synapse.Integration.Commands.WorkflowActivities;
using Synapse.Integration.Commands.WorkflowInstances;
using Synapse.Integration.Models;
using System.Threading.Channels;

namespace Synapse.Apis.Runtime.Ipc
{

    /// <summary>
    /// Represents an Intra-Process Communication (IPC) client for the Synapse Runtime API
    /// </summary>
    public class SynapseIpcRuntimeApiClient
        : ISynapseRuntimeApi
    {

        /// <summary>
        /// Initializes a new <see cref="SynapseIpcRuntimeApiClient"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="runtimeProxyFactory">The service used to create <see cref="IWorkflowRuntimeProxy"/> instances</param>
        /// <param name="runtimeProxyManager">The service used to manage <see cref="IWorkflowRuntimeProxy"/></param>
        protected SynapseIpcRuntimeApiClient(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, 
            IWorkflowRuntimeProxyFactory runtimeProxyFactory, IWorkflowRuntimeProxyManager runtimeProxyManager)
        {
            this.Logger = loggerFactory.CreateLogger(this.GetType());
            this.Mediator = mediator;
            this.Mapper = mapper;
            this.RuntimeProxyFactory = runtimeProxyFactory;
            this.RuntimeProxyManager = runtimeProxyManager;
        }

        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

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
        public virtual async IAsyncEnumerable<V1RuntimeSignal> Connect(string runtimeId)
        {
            if (string.IsNullOrWhiteSpace(runtimeId))
                throw new ArgumentNullException(nameof(runtimeId));
            var stream = Channel.CreateUnbounded<V1RuntimeSignal>();
            var streamWriter = new AsyncStreamWriter<V1RuntimeSignal>(stream.Writer);
            var runtime = RuntimeProxyManager.Register(RuntimeProxyFactory.CreateProxy(runtimeId, streamWriter));
            await foreach (var message in stream.Reader.ReadAllAsync())
            {
                yield return message;
            }
            runtime.Dispose();
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowInstance> StartAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(workflowInstanceId))
                throw new ArgumentNullException(nameof(workflowInstanceId));
            return await this.Mediator.ExecuteAndUnwrapAsync(new Application.Commands.WorkflowInstances.V1MarkWorkflowInstanceAsStartedCommand(workflowInstanceId), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1Event?> ConsumeOrBeginCorrelateEventAsync(V1ConsumeWorkflowInstancePendingEventCommand command, CancellationToken cancellationToken = default)
        {
            if(command == null)
                throw new ArgumentNullException(nameof(command));
            return await this.Mediator.ExecuteAndUnwrapAsync(this.Mapper.Map<Application.Commands.WorkflowInstances.V1ConsumeOrBeginCorrelateEventCommand>(command), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowActivity>> GetActivitiesAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(workflowInstanceId))
                throw new ArgumentNullException(nameof(workflowInstanceId));
            return await this.Mediator.ExecuteAndUnwrapAsync(new Application.Queries.WorkflowActivities.V1GetWorkflowActivitiesQuery(workflowInstanceId, true), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowActivity>> GetOperativeActivitiesAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(workflowInstanceId))
                throw new ArgumentNullException(nameof(workflowInstanceId));
            return await this.Mediator.ExecuteAndUnwrapAsync(new Application.Queries.WorkflowActivities.V1GetWorkflowActivitiesQuery(workflowInstanceId), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowActivity>> GetActivitiesAsync(string workflowInstanceId, string activityId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(workflowInstanceId))
                throw new ArgumentNullException(nameof(workflowInstanceId));
            if (string.IsNullOrWhiteSpace(activityId))
                throw new ArgumentNullException(nameof(activityId));
            return await this.Mediator.ExecuteAndUnwrapAsync(new Application.Queries.WorkflowActivities.V1GetWorkflowActivitiesQuery(workflowInstanceId, true, activityId), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowActivity>> GetOperativeActivitiesAsync(string workflowInstanceId, string activityId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(workflowInstanceId))
                throw new ArgumentNullException(nameof(workflowInstanceId));
            if (string.IsNullOrWhiteSpace(activityId))
                throw new ArgumentNullException(nameof(activityId));
            return await this.Mediator.ExecuteAndUnwrapAsync(new Application.Queries.WorkflowActivities.V1GetWorkflowActivitiesQuery(workflowInstanceId, false, activityId), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> CreateActivityAsync(V1CreateWorkflowActivityCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            return await this.Mediator.ExecuteAndUnwrapAsync(this.Mapper.Map<Application.Commands.WorkflowActivities.V1CreateWorkflowActivityCommand>(command), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> StartActivityAsync(string activityId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(activityId))
                throw new ArgumentNullException(nameof(activityId));
            return await this.Mediator.ExecuteAndUnwrapAsync(new Application.Commands.WorkflowActivities.V1StartWorkflowActivityCommand(activityId), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> SuspendActivityAsync(string activityId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(activityId))
                throw new ArgumentNullException(nameof(activityId));
            return await this.Mediator.ExecuteAndUnwrapAsync(new Application.Commands.WorkflowActivities.V1SuspendWorkflowActivityCommand(activityId), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> SkipActivityAsync(string activityId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(activityId))
                throw new ArgumentNullException(nameof(activityId));
            return await this.Mediator.ExecuteAndUnwrapAsync(new Application.Commands.WorkflowActivities.V1SkipWorkflowActivityCommand(activityId), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> SetActivityMetadataAsync(V1SetWorkflowActivityMetadataCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            return await this.Mediator.ExecuteAndUnwrapAsync(this.Mapper.Map<Application.Commands.WorkflowActivities.V1SetWorkflowActivityMetadataCommand>(command), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> FaultActivityAsync(V1FaultWorkflowActivityCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            return await this.Mediator.ExecuteAndUnwrapAsync(this.Mapper.Map<Application.Commands.WorkflowActivities.V1FaultWorkflowActivityCommand>(command), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> CancelActivityAsync(string activityId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(activityId))
                throw new ArgumentNullException(nameof(activityId));
            return await this.Mediator.ExecuteAndUnwrapAsync(new Application.Commands.WorkflowActivities.V1CancelWorkflowActivityCommand(activityId), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> SetActivityOutputAsync(V1SetWorkflowActivityOutputCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            return await this.Mediator.ExecuteAndUnwrapAsync(this.Mapper.Map<Application.Commands.WorkflowActivities.V1SetWorkflowActivityOutputCommand>(command), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<Dynamic> GetActivityStateDataAsync(string activityId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(activityId))
                throw new ArgumentNullException(nameof(activityId));
            return await this.Mediator.ExecuteAndUnwrapAsync(new Application.Queries.WorkflowActivities.V1GetActivityParentStateDataQuery(activityId), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<bool> TryCorrelateAsync(V1TryCorrelateWorkflowInstanceCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            return await this.Mediator.ExecuteAndUnwrapAsync(this.Mapper.Map<Application.Commands.WorkflowInstances.V1TryCorrelateWorkflowInstanceCommand>(command), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task SetCorrelationMappingAsync(V1SetWorkflowInstanceCorrelationMappingCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            await this.Mediator.ExecuteAndUnwrapAsync(this.Mapper.Map<Application.Commands.WorkflowInstances.V1SetWorkflowInstanceCorrelationMappingCommand>(command), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowInstance> StartSubflowAsync(V1CreateWorkflowInstanceCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            return await this.Mediator.ExecuteAndUnwrapAsync(this.Mapper.Map<Application.Commands.WorkflowInstances.V1CreateWorkflowInstanceCommand>(command), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowInstance> FaultAsync(V1FaultWorkflowInstanceCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            return await this.Mediator.ExecuteAndUnwrapAsync(this.Mapper.Map<Application.Commands.WorkflowInstances.V1FaultWorkflowInstanceCommand>(command), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> CompensateActivityAsync(V1CompensateActivityCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            return await this.Mediator.ExecuteAndUnwrapAsync(this.Mapper.Map<Application.Commands.WorkflowActivities.V1CompensateActivityCommand>(command), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> MarkActivityAsCompensatedAsync(V1MarkActivityAsCompensatedCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            return await this.Mediator.ExecuteAndUnwrapAsync(this.Mapper.Map<Application.Commands.WorkflowActivities.V1MarkActivityAsCompensatedCommand>(command), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowInstance> SuspendAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(workflowInstanceId))
                throw new ArgumentNullException(nameof(workflowInstanceId));
            return await this.Mediator.ExecuteAndUnwrapAsync(new Application.Commands.WorkflowInstances.V1MarkWorkflowInstanceAsSuspendedCommand(workflowInstanceId), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowInstance> CancelAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(workflowInstanceId))
                throw new ArgumentNullException(nameof(workflowInstanceId));
            return await this.Mediator.ExecuteAndUnwrapAsync(new Application.Commands.WorkflowInstances.V1MarkWorkflowInstanceAsCancelledCommand(workflowInstanceId), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowInstance> SetOutputAsync(V1SetWorkflowInstanceOutputCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            return await this.Mediator.ExecuteAndUnwrapAsync(this.Mapper.Map<Application.Commands.WorkflowInstances.V1SetWorkflowInstanceOutputCommand>(command), cancellationToken);
        }

    }

}
