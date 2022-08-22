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
    /// Represents the <see cref="ICommand"/> used to resume the execution of an existing <see cref="V1WorkflowInstance"/>
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.WorkflowInstances.V1ResumeWorkflowInstanceCommand))]
    public class V1ResumeWorkflowInstanceCommand
        : Command<Integration.Models.V1WorkflowInstance>
    {

        /// <summary>
        /// Initializes a new <see cref="V1ResumeWorkflowInstanceCommand"/>
        /// </summary>
        protected V1ResumeWorkflowInstanceCommand()
        {
            this.WorkflowInstanceId = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1ResumeWorkflowInstanceCommand"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowInstance"/> to resume the execution of</param>
        public V1ResumeWorkflowInstanceCommand(string id)
        {
            this.WorkflowInstanceId = id;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowInstance"/> to resume the execution of
        /// </summary>
        public virtual string WorkflowInstanceId { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1ResumeWorkflowInstanceCommand"/>s
    /// </summary>
    public class V1ResumeWorkflowInstanceCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1ResumeWorkflowInstanceCommand, Integration.Models.V1WorkflowInstance>
    {

        /// <summary>
        /// Initializes a new <see cref="V1ResumeWorkflowInstanceCommandHandler"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="workflows">The <see cref="IRepository"/> used to manage <see cref="V1Workflow"/>s</param>
        /// <param name="workflowInstances">The <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s</param>
        /// <param name="processManager">The service used to manage <see cref="IWorkflowProcess"/>es</param>
        public V1ResumeWorkflowInstanceCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<V1Workflow> workflows, IRepository<V1WorkflowInstance> workflowInstances, IWorkflowProcessManager processManager)
            : base(loggerFactory, mediator, mapper)
        {
            this.Workflows = workflows;
            this.WorkflowInstances = workflowInstances;
            this.ProcessManager = processManager;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1Workflow"/>s
        /// </summary>
        protected IRepository<V1Workflow> Workflows { get; }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s
        /// </summary>
        protected IRepository<V1WorkflowInstance> WorkflowInstances { get; }

        /// <summary>
        /// Gets the service used to manage <see cref="IWorkflowProcess"/>es
        /// </summary>
        protected IWorkflowProcessManager ProcessManager { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<Integration.Models.V1WorkflowInstance>> HandleAsync(V1ResumeWorkflowInstanceCommand command, CancellationToken cancellationToken = default)
        {
            var workflowInstance = await this.WorkflowInstances.FindAsync(command.WorkflowInstanceId, cancellationToken);
            if (workflowInstance == null)
                throw DomainException.NullReference(typeof(V1WorkflowInstance), command.WorkflowInstanceId);
            var workflow = await this.Workflows.FindAsync(workflowInstance.WorkflowId, cancellationToken);
            if (workflow == null)
                throw DomainException.NullReference(typeof(V1Workflow), workflowInstance.WorkflowId);
            var process = await this.ProcessManager.StartProcessAsync(workflow, workflowInstance, cancellationToken);
            workflowInstance.Resume(process.Id);
            workflowInstance = await this.WorkflowInstances.UpdateAsync(workflowInstance, cancellationToken);
            await this.WorkflowInstances.SaveChangesAsync(cancellationToken);
            return this.Ok(this.Mapper.Map<Integration.Models.V1WorkflowInstance>(workflowInstance));
        }

    }

}
