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

using System.Collections.ObjectModel;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents the context of an event correlation
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Models.V1CorrelationContext))]
    public class V1CorrelationContext
        : Entity<string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1CorrelationContext"/>
        /// </summary>
        public V1CorrelationContext()
            : base(Guid.NewGuid().ToString())
        {

        }

        [Newtonsoft.Json.JsonProperty(nameof(Mappings))]
        [System.Text.Json.Serialization.JsonPropertyName(nameof(Mappings))]
        private Dictionary<string, string> _Mappings = new();
        /// <summary>
        /// Gets an <see cref="IReadOnlyDictionary{TKey, TValue}"/> containing the correlations' value by key mappings
        /// </summary>
        public virtual IReadOnlyDictionary<string, string> Mappings => new ReadOnlyDictionary<string, string>(this._Mappings);

        [Newtonsoft.Json.JsonProperty(nameof(PendingEvents))]
        [System.Text.Json.Serialization.JsonPropertyName(nameof(PendingEvents))]
        private List<V1Event> _PendingEvents = new();
        /// <summary>
        /// Gets an <see cref="IReadOnlyCollection{T}"/> containing all correlated <see cref="V1Event"/>s pending processing
        /// </summary>
        public virtual IReadOnlyCollection<V1Event> PendingEvents => this._PendingEvents.AsReadOnly();

        /// <summary>
        /// Determines whether or not the specified <see cref="V1Event"/> correlates to the <see cref="V1CorrelationContext"/>
        /// </summary>
        /// <param name="e">The <see cref="V1Event"/> to correlate</param>
        /// <returns>A boolean indicating whether or not the specified <see cref="V1Event"/> correlates to the <see cref="V1CorrelationContext"/></returns>
        public virtual bool CorrelatesTo(V1Event e)
        {
            if (e == null)
                throw DomainException.ArgumentNull(nameof(e));
            if (this.PendingEvents.Any(be => be.Type == e.Type && be.Source == e.Source)) 
                return false; //the specified event type/source has already been correlated
            foreach (var mapping in this.Mappings)
            {
                if (!e.TryGetAttribute(mapping.Key, out var attributeValue)
                    || attributeValue != mapping.Value)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Correlates the specified <see cref="V1Event"/>
        /// </summary>
        /// <param name="e">The <see cref="V1Event"/> to correlate</param>
        /// <param name="mappings">An <see cref="IEnumerable{T}"/> containing the context attributes used to correlate the specified <see cref="V1Event"/></param>
        /// <param name="enqueue">A boolean indicating whether or not to enqueue the specified <see cref="V1Event"/> for processing. Defaults to false.</param>
        public virtual void Correlate(V1Event e, IEnumerable<string> mappings, bool enqueue = false)
        {
            if (e == null)
                throw DomainException.ArgumentNull(nameof(e));
            if (mappings == null)
                mappings = Array.Empty<string>();
            if (enqueue)
                this._PendingEvents.Add(e);
            foreach (var key in mappings)
            {
                if (this.Mappings.ContainsKey(key))
                    continue;
                if (!e.TryGetAttribute(key, out var attributeValue))
                    throw new InvalidOperationException($"The event with id '{e.Id}' does not define the required mapping '{key}'");
                this._Mappings.Add(key, attributeValue);
            }
        }

        /// <summary>
        /// Sets the specified correlation mapping
        /// </summary>
        /// <param name="key">The key of the correlation mapping to set</param>
        /// <param name="value">The value of the correlation mapping to set</param>
        internal protected virtual void SetMapping(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw DomainException.ArgumentNull(nameof(key));
            if (string.IsNullOrWhiteSpace(value))
                throw DomainException.ArgumentNull(nameof(value));
            this._Mappings[key] = value;
        }

        /// <summary>
        /// Creates a new <see cref="V1CorrelationContext"/> for the specified bootstrap <see cref="V1Event"/>
        /// </summary>
        /// <param name="e">The <see cref="V1Event"/> that has bootstrapped the <see cref="V1CorrelationContext"/></param>
        /// <param name="mappings">An <see cref="IEnumerable{T}"/> containing the context attributes used to correlate <see cref="V1Event"/>s</param>
        /// <returns>A new <see cref="V1CorrelationContext"/></returns>
        public static V1CorrelationContext CreateFor(V1Event e, IEnumerable<string> mappings)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            if (mappings == null)
                throw new ArgumentNullException(nameof(mappings));
            var correlationContext = new V1CorrelationContext();
            correlationContext._PendingEvents.Add(e);
            foreach (string key in mappings)
            {
                if (!e.TryGetAttribute(key, out var attributeValue))
                    throw new InvalidOperationException($"The cloud event with id '{e.Id}' does not define the required context attribute '{key}'");
                correlationContext._Mappings.Add(key, attributeValue);
            }
            return correlationContext;
        }

    }

}
