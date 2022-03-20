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
    /// Represents a condition of an event correlation
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Models.V1CorrelationCondition))]
    public class V1CorrelationCondition
    {

        /// <summary>
        /// Initializes a new <see cref="V1CorrelationCondition"/>
        /// </summary>
        protected V1CorrelationCondition()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1CorrelationCondition"/>
        /// </summary>
        public V1CorrelationCondition(params V1EventFilter[] filters)
        {
            if (filters == null)
                throw DomainException.ArgumentNull(nameof(filters));
            if (!filters.Any())
                throw DomainException.ArgumentMustHaveMinimumLengthOf(nameof(filters), 1);
            this._Filters = filters.ToList();
        }

        private List<V1EventFilter> _Filters = new();
        /// <summary>
        /// Gets an <see cref="IReadOnlyCollection{T}"/> containing the <see cref="V1EventFilter"/> used to configure the filtering of events that can fire the <see cref="V1Correlation"/>
        /// </summary>
        public IReadOnlyCollection<V1EventFilter> Filters => this._Filters.AsReadOnly();

        /// <summary>
        /// Determines whether or not the <see cref="V1CorrelationCondition"/> matches the specified <see cref="V1Event"/>
        /// </summary>
        /// <param name="e">The <see cref="V1Event"/> to match</param>
        /// <returns>A boolean indicating whether or not the <see cref="V1CorrelationCondition"/> matches the specified <see cref="V1Event"/></returns>
        public virtual bool Matches(V1Event e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            return this.GetMatchingFilterFor(e) != null;
        }

        /// <summary>
        /// Determines whether or not the <see cref="V1CorrelationCondition"/> matches the specified <see cref="V1CorrelationContext"/>
        /// </summary>
        /// <param name="context">The <see cref="V1CorrelationContext"/> to match</param>
        /// <returns>A boolean indicating whether or not the <see cref="V1CorrelationCondition"/> matches the specified <see cref="V1CorrelationContext"/></returns>
        public virtual bool Matches(V1CorrelationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            return context.PendingEvents.Any(e => this.Filters.Any(f => f.Filters(e)));
        }

        /// <summary>
        /// Gets the matching <see cref="V1EventFilter"/> for the specified <see cref="V1Event"/>
        /// </summary>
        /// <param name="e">The <see cref="V1Event"/> to get the matching <see cref="V1EventFilter"/> for</param>
        /// <returns>The matching <see cref="V1EventFilter"/> for the specified <see cref="V1Event"/>, if any</returns>
        public virtual V1EventFilter? GetMatchingFilterFor(V1Event e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            return this.Filters.FirstOrDefault(f => f.Filters(e));
        }

        /// <summary>
        /// Creates a new <see cref="V1CorrelationCondition"/> used to match events defined by the specified <see cref="EventDefinition"/>
        /// </summary>
        /// <param name="eventDefinition">The <see cref="EventDefinition"/> for which to build a new <see cref="V1CorrelationCondition"/></param>
        /// <returns>A new <see cref="V1CorrelationCondition"/></returns>
        public static V1CorrelationCondition Match(EventDefinition eventDefinition)
        {
            if(eventDefinition == null)
                throw new ArgumentNullException(nameof(eventDefinition));
            var mappings = new Dictionary<string, string>();
            if(!string.IsNullOrWhiteSpace(eventDefinition.Source))
                mappings.Add(nameof(CloudEvent.Source).ToLower(), eventDefinition.Source);
            if (!string.IsNullOrWhiteSpace(eventDefinition.Type))
                mappings.Add(nameof(CloudEvent.Type).ToLower(), eventDefinition.Type);
            var filter = new V1EventFilter(mappings);
            return new(filter);
        }

    }

}
