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

using Synapse.Application.Commands.Generic;
using Synapse.Application.Commands.WorkflowInstances;
using System.ComponentModel.DataAnnotations;

namespace Synapse.Application.Commands.Workflows
{
    /// <summary>
    /// Represents the <see cref="ICommand"/> used to delete an existing <see cref="V1Workflow"/>
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.Workflows.V1DeleteWorkflowCommand))]
    public class V1DeleteWorkflowCommand
        : Command
    {

        /// <summary>
        /// Initializes a new <see cref="V1DeleteWorkflowCommand"/>
        /// </summary>
        protected V1DeleteWorkflowCommand()
        {
            this.WorkflowId = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1DeleteWorkflowCommand"/>
        /// </summary>
        /// <param name="workflowId">The id of the <see cref="V1Workflow"/> to delete. Note that failing to specify the version will remove all versions of the specified <see cref="V1Workflow"/></param>
        public V1DeleteWorkflowCommand(string workflowId)
        {
            this.WorkflowId = workflowId;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1Workflow"/> to delete. Note that failing to specify the version will remove all versions of the specified <see cref="V1Workflow"/>
        /// </summary>
        [Required, MinLength(1)]
        public virtual string WorkflowId { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1DeleteWorkflowCommand"/>s
    /// </summary>
    public class V1DeleteWorkflowCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1DeleteWorkflowCommand>
    {

        /// <summary>
        /// Initializes a new <see cref="V1DeleteWorkflowCommandHandler"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="workflowWriteModems">The <see cref="IRepository"/> used to manage <see cref="V1Workflow"/>s</param>
        /// <param name="workflowReadModels">The <see cref="IRepository"/> used to manage <see cref="Integration.Models.V1Workflow"/>s</param>
        /// <param name="workflowInstanceWriteModels">The <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s</param>
        /// <param name="workflowInstances">The <see cref="IRepository"/> used to manage <see cref="Integration.Models.V1WorkflowInstance"/>s</param>
        public V1DeleteWorkflowCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper,
            IRepository<V1Workflow> workflowWriteModems, IRepository<Integration.Models.V1Workflow> workflowReadModels, IRepository<Integration.Models.V1WorkflowInstance> workflowInstances) 
            : base(loggerFactory, mediator, mapper)
        {
            this.WorkflowWriteModels = workflowWriteModems;
            this.WorkflowReadModels = workflowReadModels;
            this.WorkflowInstances = workflowInstances;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1Workflow"/>s
        /// </summary>
        protected IRepository<V1Workflow> WorkflowWriteModels { get; }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="Integration.Models.V1Workflow"/>s
        /// </summary>
        protected IRepository<Integration.Models.V1Workflow> WorkflowReadModels { get; }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="Integration.Models.V1WorkflowInstance"/>s
        /// </summary>
        protected IRepository<Integration.Models.V1WorkflowInstance> WorkflowInstances { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult> HandleAsync(V1DeleteWorkflowCommand command, CancellationToken cancellationToken = default)
        {
            var components = command.WorkflowId.Split(":");
            if(components.Length == 2)
            {
                var workflow = await this.WorkflowWriteModels.FindAsync(command.WorkflowId, cancellationToken);
                if (workflow == null)
                    throw DomainException.NullReference(typeof(V1Workflow), command.WorkflowId);
                foreach (var instanceId in this.WorkflowInstances.AsQueryable()
                    .Where(i => i.WorkflowId == workflow.Id)
                    .Select(i => i.Id)
                    .ToList())
                {
                    await this.Mediator.ExecuteAndUnwrapAsync(new V1DeleteWorkflowInstanceCommand(instanceId));
                }
                workflow.Delete();
                await this.WorkflowWriteModels.UpdateAsync(workflow, cancellationToken);
                await this.WorkflowWriteModels.RemoveAsync(workflow, cancellationToken);
                await this.WorkflowWriteModels.SaveChangesAsync(cancellationToken);
            }
            else
            {
                var workflowIds = this.WorkflowReadModels.AsQueryable()
                    .Where(w => w.Definition.Id!.Equals(command.WorkflowId, StringComparison.OrdinalIgnoreCase))
                    .Select(w => w.Id)
                    .ToList();
                if(!workflowIds.Any())
                    throw DomainException.NullReference(typeof(V1Workflow), command.WorkflowId);
                foreach (var workflowId in workflowIds)
                {
                    var workflow = await this.WorkflowWriteModels.FindAsync(workflowId, cancellationToken);
                    if (workflow == null)
                        throw DomainException.NullReference(typeof(V1Workflow), workflowId);
                    workflow.Delete();
                    foreach (var instanceId in this.WorkflowInstances.AsQueryable()
                       .Where(i => i.WorkflowId == workflow.Id)
                       .Select(i => i.Id)
                       .ToList())
                    {
                        await this.Mediator.ExecuteAndUnwrapAsync(new V1DeleteWorkflowInstanceCommand(instanceId));
                    }
                    await this.WorkflowWriteModels.UpdateAsync(workflow, cancellationToken);
                    await this.WorkflowWriteModels.RemoveAsync(workflow, cancellationToken);
                }
                await this.WorkflowWriteModels.SaveChangesAsync(cancellationToken);
            }
            return this.Ok();
        }

    }

}
