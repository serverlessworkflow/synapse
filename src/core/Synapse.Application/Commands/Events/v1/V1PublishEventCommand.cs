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

namespace Synapse.Application.Commands.Events
{

    /// <summary>
    /// Represents the <see cref="ICommand"/> used to publish a new <see cref="V1Event"/>
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.Events.V1PublishEventCommand))]
    public class V1PublishEventCommand
        : Command
    {

        /// <summary>
        /// Initializes a new <see cref="V1PublishEventCommand"/>
        /// </summary>
        protected V1PublishEventCommand() { }

        /// <summary>
        /// Initializes a new <see cref="V1PublishEventCommand"/>
        /// </summary>
        /// <param name="e">The <see cref="V1Event"/> to publish</param>
        public V1PublishEventCommand(V1Event e)
        {
            this.Event = e;
        }

        /// <summary>
        /// Gets the <see cref="V1Event"/> to publish
        /// </summary>
        public virtual V1Event Event { get; protected set; } = null!;

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1PublishEventCommand"/>s
    /// </summary>
    public class V1PublishEventCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1PublishEventCommand>
    {

        /// <summary>
        /// Initializes a new <see cref="V1PublishEventCommandHandler"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="cloudEventBus">The service used to publish and subscribe to <see cref="CloudEvent"/>s</param>
        public V1PublishEventCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, ICloudEventBus cloudEventBus) 
            : base(loggerFactory, mediator, mapper)
        {
            this.CloudEventBus = cloudEventBus;
        }

        /// <summary>
        /// Gets the service used to publish and subscribe to <see cref="CloudEvent"/>s
        /// </summary>
        protected ICloudEventBus CloudEventBus { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult> HandleAsync(V1PublishEventCommand command, CancellationToken cancellationToken = default)
        {
            var e = command.Event.AsCloudEvent();
            await this.CloudEventBus.PublishAsync(e, cancellationToken);
            return this.Ok();
        }

    }

}
