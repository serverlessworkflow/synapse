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

using ServerlessWorkflow.Sdk.Models;
using Synapse.Integration.Commands.Workflows;
using Synapse.Integration.Models;
using System.ComponentModel.DataAnnotations;

namespace Synapse.Application.Commands.Workflows
{

    /// <summary>
    /// Represents the <see cref="ICommand"/> used to create a new <see cref="V1Workflow"/>
    /// </summary>
    [DataTransferObjectType(typeof(V1CreateWorkflowCommandDto))]
    public class V1CreateWorkflowCommand
        : Command<V1WorkflowDto>
    {

        /// <summary>
        /// Initializes a new <see cref="V1CreateWorkflowCommand"/>
        /// </summary>
        protected V1CreateWorkflowCommand()
        {
            this.Definition = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1CreateWorkflowCommand"/>
        /// </summary>
        /// <param name="definition">The definition of the <see cref="V1Workflow"/> to create</param>
        public V1CreateWorkflowCommand(WorkflowDefinition definition)
        {
            this.Definition = definition;
        }

        /// <summary>
        /// Gets the definition of the <see cref="V1Workflow"/> to create
        /// </summary>
        [Required]
        public virtual WorkflowDefinition Definition { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1CreateWorkflowCommand"/>s
    /// </summary>
    public class V1CreateWorkflowCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1CreateWorkflowCommand, V1WorkflowDto>
    {

        /// <summary>
        /// Initializes a new <see cref="V1CreateWorkflowCommandHandler"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="workflows">The <see cref="IRepository"/> used to manage <see cref="V1Workflow"/>s</param>
        /// <param name="runtimeHost">The current <see cref="IWorkflowRuntimeHost"/></param>
        public V1CreateWorkflowCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<V1Workflow> workflows, IWorkflowRuntimeHost runtimeHost) 
            : base(loggerFactory, mediator, mapper)
        {
            this.Workflows = workflows;
            this.RuntimeHost = runtimeHost;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1Workflow"/>s
        /// </summary>
        protected IRepository<V1Workflow> Workflows { get; }

        /// <summary>
        /// Gets the current <see cref="IWorkflowRuntimeHost"/>
        /// </summary>
        protected IWorkflowRuntimeHost RuntimeHost { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<V1WorkflowDto>> HandleAsync(V1CreateWorkflowCommand command, CancellationToken cancellationToken = default)
        {
            while (await this.Workflows.ContainsAsync(command.Definition.GetUniqueIdentifier(), cancellationToken))
            {
                var version = Version.Parse(command.Definition.Version);
                version = new Version(version.Major, version.Minor, version.Build == -1 ? 1 : version.Build + 1);
                command.Definition.Version = version.ToString(3);
            }
            var workflow = await this.Workflows.AddAsync(new(command.Definition), cancellationToken);
            await this.Workflows.SaveChangesAsync(cancellationToken);
            if(command.Definition.Start != null)
            {
                //todo: schedule or deploy trigger if needed
                //if (!string.IsNullOrWhiteSpace(workflow.Definition.Start.Schedule.Cron?.Expression))
                //    await this.RuntimeHost.ScheduleAsync(workflow, cancellationToken);
                //if (!string.IsNullOrWhiteSpace(workflow.Definition.Start.Schedule.Interval))
                //    await this.RuntimeHost.ScheduleAsync(workflow, cancellationToken);
            }
            return this.Ok(this.Mapper.Map<V1WorkflowDto>(workflow));
        }

    }

}
