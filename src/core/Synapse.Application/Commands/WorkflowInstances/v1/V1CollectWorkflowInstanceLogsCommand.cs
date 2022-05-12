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

namespace Synapse.Application.Commands.WorkflowInstances
{
    /// <summary>
    /// Represents the <see cref="ICommand"/> used to retrieve the logs of a <see cref="V1WorkflowInstance"/>'s execution
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.WorkflowInstances.V1CollectWorkflowInstanceLogsCommand))]
    public class V1CollectWorkflowInstanceLogsCommand
        : Command<string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstance"/>
        /// </summary>
        protected V1CollectWorkflowInstanceLogsCommand()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstance"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowInstance"/> to get the logs of</param>
        public V1CollectWorkflowInstanceLogsCommand(string id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstance"/>
        /// </summary>
        /// <param name="workflowInstance">The <see cref="V1WorkflowInstance"/> to get the logs of</param>
        public V1CollectWorkflowInstanceLogsCommand(V1WorkflowInstance workflowInstance)
        {
            if (workflowInstance == null)
                throw new ArgumentNullException(nameof(workflowInstance));
            this.Instance = workflowInstance;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowInstance"/> to get the logs of
        /// </summary>
        public virtual string? Id { get; protected set; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowInstance"/> to get the logs of
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [YamlDotNet.Serialization.YamlIgnore]
        public virtual V1WorkflowInstance? Instance { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1CollectWorkflowInstanceLogsCommand"/>s
    /// </summary>
    public class V1CollectWorkflowInstanceLogsCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1CollectWorkflowInstanceLogsCommand, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1CollectWorkflowInstanceLogsCommandHandler"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="workflowInstances">The <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s</param>
        /// <param name="workflowRuntimeHost">The service used to host <see cref="V1WorkflowInstance"/> runtime</param>
        public V1CollectWorkflowInstanceLogsCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<V1WorkflowInstance> workflowInstances, IWorkflowRuntimeHost workflowRuntimeHost)
            : base(loggerFactory, mediator, mapper)
        {
            this.WorkflowInstances = workflowInstances;
            this.WorkflowRuntimeHost = workflowRuntimeHost;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s
        /// </summary>
        protected IRepository<V1WorkflowInstance> WorkflowInstances { get; }

        /// <summary>
        /// Gets the service used to host <see cref="V1WorkflowInstance"/> runtime
        /// </summary>
        protected IWorkflowRuntimeHost WorkflowRuntimeHost { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<string>> HandleAsync(V1CollectWorkflowInstanceLogsCommand command, CancellationToken cancellationToken = default)
        {
            var instance = command.Instance;
            if (instance == null)
                instance = await this.WorkflowInstances.FindAsync(command.Id, cancellationToken);
            if (instance == null)
                throw DomainException.NullReference(typeof(V1WorkflowInstance), command.Id);
            var activeSession = instance.ActiveSession;
            if (activeSession == null)
                throw new DomainException($"The workflow instance with id '{command.Id}' does not have an active session");
            var runtimeIdentifier = activeSession.RuntimeIdentifier;
            var logs = await this.WorkflowRuntimeHost.GetRuntimeLogsAsync(runtimeIdentifier, cancellationToken);
            return this.Ok(logs);
        }

    }

}
