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

using ServerlessWorkflow.Sdk;
using ServerlessWorkflow.Sdk.Models;
using ServerlessWorkflow.Sdk.Services.Validation;
using Synapse.Application.Commands.Correlations;
using Synapse.Application.Queries.Workflows;
using System.ComponentModel.DataAnnotations;

namespace Synapse.Application.Commands.Workflows
{

    /// <summary>
    /// Represents the <see cref="ICommand"/> used to create a new <see cref="V1Workflow"/>
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.Workflows.V1CreateWorkflowCommand))]
    public class V1CreateWorkflowCommand
        : Command<Integration.Models.V1Workflow>
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
        /// <param name="ifNotExists">A boolean indicating whether the <see cref="V1Workflow"/> should be created only if it does not already exist. Defaults to false, in which case the <see cref="Definition"/> is automatically versionned</param>
        public V1CreateWorkflowCommand(WorkflowDefinition definition, bool ifNotExists)
        {
            this.Definition = definition;
            this.IfNotExists = ifNotExists;
        }

        /// <summary>
        /// Gets the definition of the <see cref="V1Workflow"/> to create
        /// </summary>
        [Required]
        public virtual WorkflowDefinition Definition { get; protected set; }

        /// <summary>
        /// Gets a boolean indicating whether the <see cref="V1Workflow"/> should be created only if it does not already exist. Defaults to false, in which case the <see cref="Definition"/> is automatically versionned
        /// </summary>
        public virtual bool IfNotExists { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1CreateWorkflowCommand"/>s
    /// </summary>
    public class V1CreateWorkflowCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1CreateWorkflowCommand, Integration.Models.V1Workflow>
    {

        /// <summary>
        /// Initializes a new <see cref="V1CreateWorkflowCommandHandler"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="workflowValidator">The service used to validate <see cref="WorkflowDefinition"/>s</param>
        /// <param name="workflows">The <see cref="IRepository"/> used to manage <see cref="V1Workflow"/>s</param>
        /// <param name="runtimeHost">The current <see cref="IWorkflowRuntime"/></param>
        public V1CreateWorkflowCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IWorkflowValidator workflowValidator, IRepository<V1Workflow> workflows, IWorkflowRuntime runtimeHost) 
            : base(loggerFactory, mediator, mapper)
        {
            this.WorkflowValidator = workflowValidator;
            this.Workflows = workflows;
            this.RuntimeHost = runtimeHost;
        }

        /// <summary>
        /// Gets the service used to validate <see cref="WorkflowDefinition"/>s
        /// </summary>
        protected IWorkflowValidator WorkflowValidator { get; }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1Workflow"/>s
        /// </summary>
        protected IRepository<V1Workflow> Workflows { get; }

        /// <summary>
        /// Gets the current <see cref="IWorkflowRuntime"/>
        /// </summary>
        protected IWorkflowRuntime RuntimeHost { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<Integration.Models.V1Workflow>> HandleAsync(V1CreateWorkflowCommand command, CancellationToken cancellationToken = default)
        {
            //var validationResult = await this.WorkflowValidator.ValidateAsync(command.Definition, true, true, cancellationToken);
            //if (!validationResult.IsValid)
            //    return this.Invalid(validationResult.AsErrors().ToArray());
            foreach(var subflowRef in command.Definition.GetSubflowReferences())
            {
                var reference = subflowRef.WorkflowId;
                if(!string.IsNullOrWhiteSpace(subflowRef.Version))
                    reference += $":{subflowRef.Version}";
                var subflow = await this.Mediator.ExecuteAndUnwrapAsync(new V1GetWorkflowByIdQuery(subflowRef.WorkflowId, subflowRef.Version), cancellationToken);
                if (subflow == null)
                    throw DomainException.NullReference(typeof(V1Workflow), $"Failed to find the referenced workflow '{reference}'");
            }
            if (command.IfNotExists
                && await this.Workflows.ContainsAsync(command.Definition.GetUniqueIdentifier(), cancellationToken))
            {
                return this.NotModified();
            }
            else
            {
                while (await this.Workflows.ContainsAsync(command.Definition.GetUniqueIdentifier(), cancellationToken))
                {
                    var version = Version.Parse(command.Definition.Version);
                    version = new Version(version.Major, version.Minor, version.Build == -1 ? 1 : version.Build + 1);
                    command.Definition.Version = version.ToString(3);
                }
            }
            var workflow = await this.Workflows.AddAsync(new(command.Definition), cancellationToken);
            await this.Workflows.SaveChangesAsync(cancellationToken);
            var startState = workflow.Definition.GetStartState();
            if (startState is EventStateDefinition eventState)
            {
                var lifetime = V1CorrelationLifetime.Transient;
                var conditionType = eventState.Exclusive ? V1CorrelationConditionType.AnyOf : V1CorrelationConditionType.AllOf;
                var conditions = new List<V1CorrelationCondition>();
                foreach(var trigger in eventState.Triggers)
                {
                    var filters = new List<V1EventFilter>(trigger.Events.Count);
                    foreach(var eventRef in trigger.Events)
                    {
                        if (!workflow.Definition.TryGetEvent(eventRef, out var e))
                            throw DomainException.NullReference(typeof(EventDefinition), eventRef, nameof(EventDefinition.Name));
                        filters.Add(V1EventFilter.Match(e));
                    }
                    conditions.Add(new(filters.ToArray()));
                }
                var outcome = new V1CorrelationOutcome(V1CorrelationOutcomeType.Start, workflow.Id);
                await this.Mediator.ExecuteAndUnwrapAsync(new V1CreateCorrelationCommand(lifetime, conditionType, conditions, outcome, null), cancellationToken: cancellationToken);
            }
            else if (!string.IsNullOrWhiteSpace(workflow.Definition.Start?.Schedule?.Cron?.Expression)) 
            {
                await this.Mediator.ExecuteAndUnwrapAsync(new V1ScheduleWorkflowCommand(workflow.Id, false), cancellationToken);
            }
            return this.Ok(this.Mapper.Map<Integration.Models.V1Workflow>(workflow));
        }

    }

}
