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
using Synapse.Integration.Events.Schedules;

namespace Synapse.Domain.Events.Schedules
{

    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever a new <see cref="V1Schedule"/> has been created
    /// </summary>
    [DataTransferObjectType(typeof(V1ScheduleCreatedIntegrationEvent))]
    public class V1ScheduleCreatedDomainEvent
        : DomainEvent<Models.V1Schedule, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1ScheduleCreatedDomainEvent"/>
        /// </summary>
        protected V1ScheduleCreatedDomainEvent() { }

        /// <summary>
        /// Initializes a new <see cref="V1ScheduleCreatedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the newly created <see cref="V1Schedule"/></param>
        /// <param name="activationType">The activation type of the newly created <see cref="V1Schedule"/></param>
        /// <param name="definition">The definition of the newly created <see cref="V1Schedule"/></param>
        /// <param name="workflowId">The id of the <see cref="V1Workflow"/> scheduled by the newly created <see cref="V1Schedule"/></param>
        /// <param name="nextOccurenceAt">The date and time at which the <see cref="V1Schedule"/> will next occur</param>
        public V1ScheduleCreatedDomainEvent(string id, V1ScheduleActivationType activationType, ScheduleDefinition definition, string workflowId, DateTimeOffset? nextOccurenceAt)
            : base(id)
        {
            this.ActivationType = activationType;
            this.Definition = definition;
            this.WorkflowId = workflowId;
            this.NextOccurenceAt = nextOccurenceAt;
        }

        /// <summary>
        /// Gets the activation type of the newly created <see cref="V1Schedule"/>
        /// </summary>
        public virtual V1ScheduleActivationType ActivationType { get; protected set; }

        /// <summary>
        /// Gets the definition of the newly created <see cref="V1Schedule"/>
        /// </summary>
        public virtual ScheduleDefinition Definition { get; protected set; } = null!;

        /// <summary>
        /// Gets the id of the <see cref="V1Workflow"/> scheduled by the newly created <see cref="V1Schedule"/>
        /// </summary>
        public virtual string WorkflowId { get; protected set; } = null!;

        /// <summary>
        /// Gets the <see cref="V1Schedule"/>'s next occurence
        /// </summary>
        public virtual DateTimeOffset? NextOccurenceAt { get; protected set; }

    }

}
