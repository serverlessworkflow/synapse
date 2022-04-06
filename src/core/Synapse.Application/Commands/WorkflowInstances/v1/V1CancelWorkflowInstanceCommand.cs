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
    /// Represents the <see cref="ICommand"/> used to cancel the execution of an existing <see cref="V1WorkflowInstance"/>
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.WorkflowInstances.V1CancelWorkflowInstanceCommand))]
    public class V1CancelWorkflowInstanceCommand
        : Command<Integration.Models.V1WorkflowInstance>
    {

        /// <summary>
        /// Initializes a new <see cref="V1CancelWorkflowInstanceCommand"/>
        /// </summary>
        protected V1CancelWorkflowInstanceCommand()
        {
            this.WorkflowInstanceId = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1CancelWorkflowInstanceCommand"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowInstance"/> to cancel</param>
        public V1CancelWorkflowInstanceCommand(string id)
        {
            this.WorkflowInstanceId = id;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowInstance"/> to cancel
        /// </summary>
        public virtual string WorkflowInstanceId { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1CancelWorkflowInstanceCommand"/>s
    /// </summary>
    public class V1CancelWorkflowInstanceCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1CancelWorkflowInstanceCommand, Integration.Models.V1WorkflowInstance>
    {

        /// <summary>
        /// Initializes a new <see cref="V1CancelWorkflowInstanceCommandHandler"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="workflowInstances">The <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s</param>
        /// <param name="runtimeProxyManager">The service used to manage runtime proxies</param>
        public V1CancelWorkflowInstanceCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, 
            IRepository<V1WorkflowInstance> workflowInstances, IWorkflowRuntimeProxyManager runtimeProxyManager)
            : base(loggerFactory, mediator, mapper)
        {
            this.WorkflowInstances = workflowInstances;
            this.RuntimeHostManager = runtimeProxyManager;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s
        /// </summary>
        protected IRepository<V1WorkflowInstance> WorkflowInstances { get; }

        /// <summary>
        /// Gets the service used to manage runtime proxies
        /// </summary>
        protected IWorkflowRuntimeProxyManager RuntimeHostManager { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<Integration.Models.V1WorkflowInstance>> HandleAsync(V1CancelWorkflowInstanceCommand command, CancellationToken cancellationToken = default)
        {
            var workflowInstance = await this.WorkflowInstances.FindAsync(command.WorkflowInstanceId, cancellationToken);
            if (workflowInstance == null)
                throw DomainException.NullReference(typeof(V1WorkflowInstance), command.WorkflowInstanceId);
            workflowInstance.Cancel();
            workflowInstance = await this.WorkflowInstances.UpdateAsync(workflowInstance, cancellationToken);
            await this.WorkflowInstances.SaveChangesAsync(cancellationToken);
            if(this.RuntimeHostManager.TryGetProxy(command.WorkflowInstanceId, out var proxy))
                await proxy.CancelAsync(cancellationToken);
            return this.Ok(this.Mapper.Map<Integration.Models.V1WorkflowInstance>(workflowInstance));
        }

    }

}
