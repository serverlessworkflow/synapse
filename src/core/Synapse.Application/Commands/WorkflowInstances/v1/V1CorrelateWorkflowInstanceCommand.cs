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
    /// Represents an <see cref="ICommand"/> used to correlate an existing <see cref="V1WorkflowInstance"/>
    /// </summary>
    public class V1CorrelateWorkflowInstanceCommand
        : Command<Integration.Models.V1WorkflowInstance>
    {


        /// <summary>
        /// Initializes a new <see cref="V1CorrelateWorkflowInstanceCommand"/>
        /// </summary>
        protected V1CorrelateWorkflowInstanceCommand()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1CorrelateWorkflowInstanceCommand"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowInstance"/> to correlate</param>
        /// <param name="correlationContext">The <see cref="V1CorrelationContext"/> to correlate the <see cref="V1WorkflowInstance"/> with</param>
        public V1CorrelateWorkflowInstanceCommand(string id, V1CorrelationContext correlationContext)
        {
            this.Id = id;
            this.CorrelationContext = correlationContext;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowInstance"/> to correlate
        /// </summary>
        public virtual string Id { get; protected set; } = null!;

        /// <summary>
        /// Gets the <see cref="V1CorrelationContext"/> to correlate the <see cref="V1WorkflowInstance"/> with
        /// </summary>
        public virtual V1CorrelationContext CorrelationContext { get; protected set; } = null!;

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1CorrelateWorkflowInstanceCommand"/>s
    /// </summary>
    public class V1CorrelateWorkflowInstanceCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1CorrelateWorkflowInstanceCommand, Integration.Models.V1WorkflowInstance>
    {

        /// <summary>
        /// Initializes a new <see cref="V1CorrelateWorkflowInstanceCommandHandler"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="workflowInstances">The <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s</param>
        /// <param name="runtimeProxyManager">The service used to manage workflow runtime proxies</param>
        public V1CorrelateWorkflowInstanceCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, 
            IRepository<V1WorkflowInstance> workflowInstances, IWorkflowRuntimeProxyManager runtimeProxyManager) 
            : base(loggerFactory, mediator, mapper)
        {
            this.WorkflowInstances = workflowInstances;
            this.RuntimeProxyManager = runtimeProxyManager;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s
        /// </summary>
        protected IRepository<V1WorkflowInstance> WorkflowInstances { get; }


        /// <summary>
        /// Gets the service used to manage workflow runtime proxies
        /// </summary>
        protected IWorkflowRuntimeProxyManager RuntimeProxyManager { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<Integration.Models.V1WorkflowInstance>> HandleAsync(V1CorrelateWorkflowInstanceCommand command, CancellationToken cancellationToken = default)
        {
            var workflowInstance = await this.WorkflowInstances.FindAsync(command.Id, cancellationToken);
            if(workflowInstance == null)
                throw DomainException.NullReference(typeof(V1WorkflowInstance), command.Id);
            workflowInstance.SetCorrelationContext(command.CorrelationContext);
            workflowInstance = await this.WorkflowInstances.UpdateAsync(workflowInstance, cancellationToken);
            await this.WorkflowInstances.SaveChangesAsync(cancellationToken);
            switch (workflowInstance.Status)
            {
                case V1WorkflowInstanceStatus.Suspended:
                    await this.Mediator.ExecuteAndUnwrapAsync(new V1StartWorkflowInstanceCommand(workflowInstance.Id), cancellationToken);
                    break;
                case V1WorkflowInstanceStatus.Running:
                    var runtimeProxy = this.RuntimeProxyManager.GetProxy(workflowInstance.Id);
                    await runtimeProxy.CorrelateAsync(command.CorrelationContext, cancellationToken);
                    break;
                default:
                    throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), workflowInstance.Id, workflowInstance.Status);
            }
            return this.Ok(this.Mapper.Map<Integration.Models.V1WorkflowInstance>(workflowInstance));
        }

    }

}
