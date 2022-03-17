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

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents the outcome of an event correlation
    /// </summary>
    public class V1CorrelationOutcome
    {

        /// <summary>
        /// Initializes a new <see cref="V1CorrelationOutcome"/>
        /// </summary>
        protected V1CorrelationOutcome()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1CorrelationOutcome"/>
        /// </summary>
        /// <param name="type">The <see cref="V1CorrelationOutcome"/>'s type</param>
        /// <param name="target">The identifier of the <see cref="V1CorrelationOutcome"/>'s target (a <see cref="V1Workflow"/> or a <see cref="V1WorkflowInstance"/>)</param>
        public V1CorrelationOutcome(V1CorrelationOutcomeType type, string target)
        {
            if (string.IsNullOrWhiteSpace(target))
                throw DomainException.ArgumentNullOrWhitespace(nameof(target));
            this.Type = type;
            this.Target = target;
        }

        /// <summary>
        /// Gets the <see cref="V1CorrelationOutcomeType"/>'s type
        /// </summary>
        public virtual V1CorrelationOutcomeType Type { get; protected set; }

        /// <summary>
        /// Gets the identifier of the <see cref="V1CorrelationOutcome"/>'s target (a <see cref="V1Workflow"/> or a <see cref="V1WorkflowInstance"/>)
        /// </summary>
        public virtual string Target { get; protected set; } = null!;

    }

}
