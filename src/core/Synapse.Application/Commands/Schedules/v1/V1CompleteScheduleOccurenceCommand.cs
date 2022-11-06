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

namespace Synapse.Application.Commands.Schedules;

/// <summary>
/// Represents the <see cref="ICommand"/> used to complete a <see cref="V1Schedule"/>'s occurence
/// </summary>
[DataTransferObjectType(typeof(Integration.Commands.Schedules.V1CompleteScheduleOccurenceCommand))]
public class V1CompleteScheduleOccurenceCommand
    : Command<Integration.Models.V1Schedule>
{

    /// <summary>
    /// Initializes a new <see cref="V1CompleteScheduleOccurenceCommand"/>
    /// </summary>
    protected V1CompleteScheduleOccurenceCommand() { }

    /// <summary>
    /// Initializes a new <see cref="V1CompleteScheduleOccurenceCommand"/>
    /// </summary>
    /// <param name="scheduleId">The id of the <see cref="V1Schedule"/> to complete an occurence of</param>
    /// <param name="workflowInstanceId">The id of the <see cref="V1WorkflowInstance"/> that has been executed</param>
    public V1CompleteScheduleOccurenceCommand(string scheduleId, string workflowInstanceId)
    {
        this.ScheduleId = scheduleId;
        this.WorkflowInstanceId = workflowInstanceId;
    }

    /// <summary>
    /// Gets the id of the <see cref="V1Schedule"/> to complete an occurence of
    /// </summary>
    public virtual string ScheduleId { get; protected set; } = null!;

    /// <summary>
    /// Gets the id of the <see cref="V1WorkflowInstance"/> that has been executed
    /// </summary>
    public virtual string WorkflowInstanceId { get; protected set; } = null!;

}

/// <summary>
/// Represents the service used to handle <see cref="V1CompleteScheduleOccurenceCommand"/>s
/// </summary>
public class V1CompleteScheduleOccurenceCommandHandler
    : CommandHandlerBase,
    ICommandHandler<V1CompleteScheduleOccurenceCommand, Integration.Models.V1Schedule>
{

    /// <summary>
    /// Initializes a new <see cref="V1CompleteScheduleOccurenceCommandHandler"/>
    /// </summary>
    /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
    /// <param name="mediator">The service used to mediate calls</param>
    /// <param name="mapper">The service used to map objects</param>
    /// <param name="schedules">The <see cref="IRepository"/> used to manage <see cref="V1Schedule"/>s</param>
    /// <param name="backgroundJobManager">The service used to manage background jobs</param>
    public V1CompleteScheduleOccurenceCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<V1Schedule> schedules, IBackgroundJobManager backgroundJobManager)
        : base(loggerFactory, mediator, mapper)
    {
        this.Schedules = schedules;
        this.BackgroundJobManager = backgroundJobManager;
    }

    /// <summary>
    /// Gets the <see cref="IRepository"/> used to manage <see cref="V1Schedule"/>s
    /// </summary>
    protected IRepository<V1Schedule> Schedules { get; }

    /// <summary>
    /// Gets the service used to manage background jobs
    /// </summary>
    protected IBackgroundJobManager BackgroundJobManager { get; }

    /// <inheritdoc/>
    public virtual async Task<IOperationResult<Integration.Models.V1Schedule>> HandleAsync(V1CompleteScheduleOccurenceCommand command, CancellationToken cancellationToken = default)
    {
        var schedule = await this.Schedules.FindAsync(command.ScheduleId, cancellationToken);
        if (schedule == null) throw DomainException.NullReference(typeof(V1Schedule), command.ScheduleId);
        schedule.CompleteOccurence(command.WorkflowInstanceId);
        await this.Schedules.UpdateAsync(schedule, cancellationToken);
        await this.Schedules.SaveChangesAsync(cancellationToken);
        if(schedule.Definition.Type == ScheduleDefinitionType.Interval && schedule.NextOccurenceAt.HasValue) await this.BackgroundJobManager.ScheduleJobAsync(schedule, cancellationToken);
        return this.Ok(this.Mapper.Map<Integration.Models.V1Schedule>(schedule));
    }

}