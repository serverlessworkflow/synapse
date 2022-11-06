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

namespace Synapse.Application.Commands.Schedules
{

    /// <summary>
    /// Represents the <see cref="ICommand"/> used to retire a <see cref="V1Schedule"/>
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.Schedules.V1RetireScheduleCommand))]
    public class V1RetireScheduleCommand
        : Command
    {

        /// <summary>
        /// Initializes a new <see cref="V1RetireScheduleCommand"/>
        /// </summary>
        protected V1RetireScheduleCommand() { }

        /// <summary>
        /// Initializes a new <see cref="V1RetireScheduleCommand"/>
        /// </summary>
        /// <param name="scheduleId">The id of the <see cref="V1Schedule"/> to retire</param>
        public V1RetireScheduleCommand(string scheduleId)
        {
            this.ScheduleId = scheduleId;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1Schedule"/> to retire
        /// </summary>
        public virtual string ScheduleId { get; protected set; } = null!;

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1RetireScheduleCommand"/>s
    /// </summary>
    public class V1RetireScheduleCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1RetireScheduleCommand>
    {

        /// <summary>
        /// Initializes a new <see cref="V1RetireScheduleCommandHandler"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="schedules">The <see cref="IRepository"/> used to manage <see cref="V1Schedule"/>s</param>
        /// <param name="backgroundJobManager">The service used to manage background jobs</param>
        public V1RetireScheduleCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<V1Schedule> schedules, IBackgroundJobManager backgroundJobManager)
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
        public virtual async Task<IOperationResult> HandleAsync(V1RetireScheduleCommand command, CancellationToken cancellationToken = default)
        {
            var schedule = await this.Schedules.FindAsync(command.ScheduleId, cancellationToken);
            if (schedule == null) throw DomainException.NullReference(typeof(V1Schedule), command.ScheduleId);
            schedule.Retire();
            await this.Schedules.UpdateAsync(schedule, cancellationToken);
            await this.Schedules.SaveChangesAsync(cancellationToken);
            await this.BackgroundJobManager.CancelJobAsync(schedule.Id, cancellationToken);
            return this.Ok();
        }

    }

}
