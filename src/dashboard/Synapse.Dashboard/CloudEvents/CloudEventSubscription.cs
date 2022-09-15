/*
 * Copyright © 2022-Present The Synapse Authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using Neuroglia.Data.Flux;
using Neuroglia.Mapping;
using Synapse.Integration.Events;
using Synapse.Integration.Models;

namespace Synapse.Dashboard.CloudEvents
{

    /// <summary>
    /// Represents the default generic implementation of the <see cref="ICloudEventSubscription"/>
    /// </summary>
    /// <typeparam name="TEvent">The type of the <see cref="V1IntegrationEvent"/> to subscribe to</typeparam>
    public abstract class CloudEventSubscription<TEntity, TEvent>
        : ICloudEventSubscription
        where TEntity : Entity
        where TEvent : V1IntegrationEvent
    {

        /// <summary>
        /// Initializes a new <see cref="CloudEventSubscription{TEntity, TEvent}"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="dispatcher">The service used to dispatch Flux actions</param>
        /// <param name="mapper">The service used to map objects</param>
        protected CloudEventSubscription(ILoggerFactory loggerFactory, IDispatcher dispatcher, IMapper mapper)
        {
            this.EventType = Synapse.CloudEvents.TypeOf<TEvent, TEntity>();
            this.Logger = loggerFactory.CreateLogger(this.GetType());
            this.Dispatcher = dispatcher;
            this.Mapper = mapper;
        }

        /// <summary>
        /// Gets the type of <see cref="V1Event"/>s the <see cref="ICloudEventSubscription"/> filters
        /// </summary>
        protected string EventType { get; }

        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the service used to dispatch Flux actions
        /// </summary>
        protected IDispatcher Dispatcher { get; }

        /// <summary>
        /// Gets the service used to map objects
        /// </summary>
        protected IMapper Mapper { get; }

        /// <inheritdoc/>
        public virtual bool Filters(V1Event e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            return e.Type.Equals(this.EventType, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public virtual void Handle(V1Event e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            this.Handle(e.Data.ToObject<TEvent>());
        }

        public abstract void Handle(TEvent e);

    }

}
