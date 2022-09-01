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

using System.Text.RegularExpressions;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents an object used to filter events
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Models.V1EventFilter))]
    public class V1EventFilter
    {

        /// <summary>
        /// Initializes a new <see cref="V1EventFilter"/>
        /// </summary>
        protected V1EventFilter()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1EventFilter"/>
        /// </summary>
        /// <param name="attributes">A <see cref="Dictionary{TKey, TValue}"/> containing the attributes to filter <see cref="V1Event"/>s by</param>
        /// <param name="correlationMappings">A <see cref="Dictionary{TKey, TValue}"/> containing the attributes key/value to use when correlating an incoming event to the <see cref="V1Correlation"/></param>
        public V1EventFilter(Dictionary<string, string> attributes, Dictionary<string, string> correlationMappings)
            : this()
        {
            this._Attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
            this._CorrelationMappings = correlationMappings ?? throw new ArgumentNullException(nameof(correlationMappings));
        }

        private Dictionary<string, string> _Attributes = new();
        /// <summary>
        /// Gets an <see cref="IDictionary{TKey, TValue}"/> containing the attributes to filter <see cref="V1Event"/>s by
        /// </summary>
        public IReadOnlyDictionary<string, string> Attributes => this._Attributes;

        private Dictionary<string, string> _CorrelationMappings = new();
        /// <summary>
        /// Gets an <see cref="IReadOnlyDictionary{TKey, TValue}"/> containing the attributes key/value to use when correlating an incoming event to the <see cref="V1Correlation"/>
        /// </summary>
        public IReadOnlyDictionary<string, string> CorrelationMappings => this._CorrelationMappings;

        /// <summary>
        /// Determines whether or not the <see cref="V1EventFilter"/> filters the specified <see cref="V1Event"/>
        /// </summary>
        /// <param name="e">The <see cref="V1Event"/> to filter</param>
        /// <returns>A boolean indicating whether or not the <see cref="V1EventFilter"/> filters the specified <see cref="V1Event"/></returns>
        public virtual bool Filters(V1Event e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            foreach (var attribute in this.Attributes)
            {
                if (!e.TryGetAttribute(attribute.Key, out var value))
                    return false;
                if (!Regex.IsMatch(value, attribute.Value, RegexOptions.IgnoreCase))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Creates a new <see cref="V1CorrelationCondition"/> used to match events defined by the specified <see cref="V1EventFilter"/>
        /// </summary>
        /// <param name="eventDefinition">The <see cref="EventDefinition"/> for which to build a new <see cref="V1EventFilter"/></param>
        /// <returns>A new <see cref="V1EventFilter"/></returns>
        public static V1EventFilter Match(EventDefinition eventDefinition)
        {
            if (eventDefinition == null)
                throw new ArgumentNullException(nameof(eventDefinition));
            var attributes = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(eventDefinition.Source))
                attributes.Add(nameof(CloudEvent.Source).ToLower(), eventDefinition.Source);
            if (!string.IsNullOrWhiteSpace(eventDefinition.Type))
                attributes.Add(nameof(CloudEvent.Type).ToLower(), eventDefinition.Type);
            var correlationMappings = new Dictionary<string, string>();
            if (eventDefinition.Correlations != null)
            {
                foreach (var mapping in eventDefinition.Correlations)
                {
                    var value = null as string;
                    if (!string.IsNullOrWhiteSpace(mapping.ContextAttributeValue)
                        && !mapping.ContextAttributeValue.IsRuntimeExpression())
                        value = mapping.ContextAttributeValue;
                    correlationMappings.Add(mapping.ContextAttributeName, value!);
                }
            }
            return new(attributes, correlationMappings);
        }

    }

}
