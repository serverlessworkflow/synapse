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

using Cronos;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Application.Commands.WorkflowInstances;
using System.ComponentModel.DataAnnotations;

namespace Synapse.Application.Commands.Workflows
{

    /// <summary>
    /// Represents the <see cref="ICommand"/> used to schedule a <see cref="V1Workflow"/>
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.Workflows.V1ScheduleWorkflowCommand))]
    public class V1ScheduleWorkflowCommand
        : Command
    {

        /// <summary>
        /// Initializes a new <see cref="V1ScheduleWorkflowCommand"/>
        /// </summary>
        protected V1ScheduleWorkflowCommand()
        {
            this.WorkflowId = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1ScheduleWorkflowCommand"/>
        /// </summary>
        /// <param name="workflowId">The id of the <see cref="V1Workflow"/> to schedule</param>
        /// <param name="catchUpMissedOccurences">A boolean indicating whether or not to catch up missed CRON occurences</param>
        public V1ScheduleWorkflowCommand(string workflowId, bool catchUpMissedOccurences)
        {
            this.WorkflowId = workflowId;
            this.CatchUpMissedOccurences = catchUpMissedOccurences;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1Workflow"/> to schedule
        /// </summary>
        [Required, MinLength(1)]
        public virtual string WorkflowId { get; protected set; }

        /// <summary>
        /// Gets a boolean indicating whether or not to catch up missed CRON occurences
        /// </summary>
        public virtual bool CatchUpMissedOccurences { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1ScheduleWorkflowCommand"/>s
    /// </summary>
    public class V1ScheduleWorkflowCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1ScheduleWorkflowCommand>
    {

        /// <summary>
        /// Initializes a new <see cref="V1ScheduleWorkflowCommandHandler"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="workflows">The <see cref="IRepository"/> used to manage <see cref="V1Workflow"/>s</param>
        public V1ScheduleWorkflowCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<V1Workflow> workflows) 
            : base(loggerFactory, mediator, mapper)
        {
            this.Workflows = workflows;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1Workflow"/>s
        /// </summary>
        protected IRepository<V1Workflow> Workflows { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult> HandleAsync(V1ScheduleWorkflowCommand command, CancellationToken cancellationToken = default)
        {
            var workflow = await this.Workflows.FindAsync(command.WorkflowId, cancellationToken);
            if (workflow == null)
                throw DomainException.NullReference(typeof(V1Workflow), command.WorkflowId);
            var rawCronExpression = workflow.Definition.Start!.Schedule!.CronExpression;
            if (workflow.Definition.Start.Schedule.Cron == null)
                throw new DomainException($"The schedule's '{nameof(ScheduleDefinition.CronExpression)}' or '{nameof(ScheduleDefinition.Cron)}' property of the specified workflow must be set in order to be scheduled");
            if (string.IsNullOrWhiteSpace(rawCronExpression))
                rawCronExpression = workflow.Definition.Start!.Schedule!.Cron.Expression;
            var cronExpression = CronExpression.Parse(rawCronExpression);
            var timeZone = TimeZoneInfo.Local;
            if (!string.IsNullOrWhiteSpace(workflow.Definition.Start.Schedule.Timezone))
                timeZone = TimeZoneInfo.FindSystemTimeZoneById(workflow.Definition.Start.Schedule.Timezone);
            Func<IServiceProvider, Task> job = async provider =>
            {
                var mediator = provider.GetRequiredService<IMediator>();
                await mediator.ExecuteAndUnwrapAsync(new V1CreateWorkflowInstanceCommand(workflow.Id, V1WorkflowInstanceActivationType.Cron, new(), null, true, null));
            };
            if (command.CatchUpMissedOccurences
                && workflow.LastInstanciated.HasValue)
            {
                foreach(var occurence in cronExpression.GetOccurrences(workflow.LastInstanciated.Value.DateTime, DateTime.Now, timeZone))
                {
                    await this.Mediator.ExecuteAndUnwrapAsync(new V1CreateWorkflowInstanceCommand(workflow.Id, V1WorkflowInstanceActivationType.Cron, new(), null, true, null), cancellationToken);
                }
            }
            //await this.CronJobScheduler.ScheduleJobAsync(workflow.Definition.Id!, cronExpression, timeZone, job, workflow.Definition.Start.Schedule.Cron?.ValidUntil, cancellationToken);
            return this.Ok();
        }

    }

}
