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

using Synapse.Integration.Events.Correlations;

namespace Synapse.Domain.Events.Correlations
{

    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever a new <see cref="V1Correlation"/> has been created
    /// </summary>
    [DataTransferObjectType(typeof(V1CorrelatedCreatedIntegrationEvent))]
    public class V1CorrelationCreatedDomainEvent
        : DomainEvent<Models.V1Correlation, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1CorrelationCreatedDomainEvent"/>
        /// </summary>
        protected V1CorrelationCreatedDomainEvent()
        {
            this.Conditions = null!;
            this.Outcome = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1CorrelationCreatedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the newly created <see cref="V1Correlation"/></param>
        /// <param name="lifetime">The lifetime of the newly created <see cref="V1Correlation"/></param>
        /// <param name="conditionType">The type of <see cref="V1CorrelationCondition"/> evaluation the newly created <see cref="V1Correlation"/> should use</param>
        /// <param name="conditions">An <see cref="IEnumerable{T}"/> containing all <see cref="V1CorrelationCondition"/>s the newly created <see cref="V1Correlation"/> is made out of</param>
        /// <param name="outcome">The <see cref="V1CorrelationOutcome"/> of the newly created <see cref="V1Correlation"/></param>
        /// <param name="context">The initial <see cref="V1CorrelationContext"/> of the newly created <see cref="V1Correlation"/></param>
        public V1CorrelationCreatedDomainEvent(string id, V1CorrelationLifetime lifetime, V1CorrelationConditionType conditionType,
            IEnumerable<Models.V1CorrelationCondition> conditions, Models.V1CorrelationOutcome outcome, Models.V1CorrelationContext? context)
            : base(id)
        {
            this.Lifetime = lifetime;
            this.ConditionType = conditionType;
            this.Conditions = conditions;
            this.Outcome = outcome;
            this.Context = context;
        }

        /// <summary>
        /// Gets the lifetime of the newly created <see cref="V1Correlation"/>
        /// </summary>
        public virtual V1CorrelationLifetime Lifetime { get; protected set; }

        /// <summary>
        /// Gets the type of <see cref="V1CorrelationCondition"/> evaluation the newly created <see cref="V1Correlation"/> should use
        /// </summary>
        public virtual V1CorrelationConditionType ConditionType { get; protected set; }

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> containing all <see cref="V1CorrelationCondition"/>s the newly created <see cref="V1Correlation"/> is made out of
        /// </summary>
        public virtual IEnumerable<Models.V1CorrelationCondition> Conditions { get; protected set; }

        /// <summary>
        /// Gets the <see cref="V1CorrelationOutcome"/> of the newly created <see cref="V1Correlation"/>
        /// </summary>
        public virtual Models.V1CorrelationOutcome Outcome { get; protected set; }

        /// <summary>
        /// Gets the initial <see cref="V1CorrelationContext"/> of the newly created <see cref="V1Correlation"/>
        /// </summary>
        public virtual Models.V1CorrelationContext? Context { get; protected set; }


    }

}
