using CloudNative.CloudEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents the context of a <see cref="CloudEvent"/> correlation
    /// </summary>
    public class V1CorrelationContext
    {

        /// <summary>
        /// Initializes a new <see cref="V1CorrelationContext"/>
        /// </summary>
        public V1CorrelationContext()
        {
            this.Id = Guid.NewGuid();
        }

        /// <summary>
        /// Gets the <see cref="V1CorrelationContext"/>'s id
        /// </summary>
        [JsonProperty("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets an <see cref="IDictionary{TKey, TValue}"/> containing the correlation context attribute mappings
        /// </summary>
        [JsonProperty("contextAttributes")]
        public IDictionary<string, string> ContextAttributes { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets an <see cref="IList{T}"/> containing the <see cref="V1CorrelationContext"/> boostrap <see cref="V1CloudEvent"/>s
        /// </summary>
        [JsonProperty("bootstrapEvents")]
        public IList<V1CloudEvent> BootstrapEvents { get; set; } = new List<V1CloudEvent>();

        /// <summary>
        /// Determines whether or not the specified <see cref="CloudEvent"/> correlates to the <see cref="V1CorrelationContext"/>
        /// </summary>
        /// <param name="e">The <see cref="CloudEvent"/> to correlate</param>
        /// <returns>A boolean indicating whether or not the specified <see cref="CloudEvent"/> correlates to the <see cref="V1CorrelationContext"/></returns>
        public virtual bool CorrelatesTo(CloudEvent e)
        {
            if (e == null)
                throw DomainException.ArgumentNull(nameof(e));
            if (this.BootstrapEvents.Any(be => be.Type == e.Type && be.Source == e.Source))
                return false;
            foreach (KeyValuePair<string, string> contextAttribute in this.ContextAttributes)
            {
                if (!e.TryGetAttribute(contextAttribute.Key, out string attributeValue)
                    || attributeValue != contextAttribute.Value)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Correlates the specified <see cref="V1CloudEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1CloudEvent"/> to correlate</param>
        /// <param name="contextAttributes">An <see cref="IEnumerable{T}"/> containing the context attributes used to correlate the specified <see cref="V1CloudEvent"/></param>
        /// <param name="isBootstrapEvent">Determines whether or not the specified <see cref="CloudEvent"/> is a boostrap event and should be persisted. Defaults to false</param>
        internal virtual void Correlate(V1CloudEvent e, IEnumerable<string> contextAttributes, bool isBootstrapEvent = false)
        {
            if (e == null)
                throw DomainException.ArgumentNull(nameof(e));
            if (contextAttributes == null)
                contextAttributes = Array.Empty<string>();
            if(isBootstrapEvent)
                this.BootstrapEvents.Add(e);
            foreach (string attributeKey in contextAttributes)
            {
                if (this.ContextAttributes.ContainsKey(attributeKey))
                    continue;
                if (!e.TryGetAttribute(attributeKey, out string attributeValue))
                    throw new InvalidOperationException($"The cloud event with id '{e.Id}' does not define the required context attribute '{attributeKey}'");
                this.ContextAttributes.Add(attributeKey, attributeValue);
            }
        }

        /// <summary>
        /// Correlates the specified <see cref="CloudEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="CloudEvent"/> to correlate</param>
        /// <param name="contextAttributes">An <see cref="IEnumerable{T}"/> containing the context attributes used to correlate the specified <see cref="CloudEvent"/></param>
        /// <param name="formatter">The service used to encode the specified <see cref="CloudEvent"/></param>
        internal virtual void CorrelateBoostrapEvent(CloudEvent e, IEnumerable<string> contextAttributes, ICloudEventFormatter formatter)
        {
            if (e == null)
                throw DomainException.ArgumentNull(nameof(e));
            if (contextAttributes == null)
                contextAttributes = Array.Empty<string>();
            this.Correlate(V1CloudEvent.CreateFor(e, formatter), contextAttributes, true);
        }

        /// <summary>
        /// Creates a new <see cref="V1CorrelationContext"/> for the specified bootstrap <see cref="CloudEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="CloudEvent"/> that has bootstrapped the <see cref="V1CorrelationContext"/></param>
        /// <param name="contextAttributes">An <see cref="IEnumerable{T}"/> containing the context attributes used to correlate <see cref="CloudEvent"/>s</param>
        /// <param name="formatter">The service used to encode the specified <see cref="CloudEvent"/></param>
        /// <returns>A new <see cref="V1CorrelationContext"/></returns>
        public static V1CorrelationContext CreateFor(CloudEvent e, IEnumerable<string> contextAttributes, ICloudEventFormatter formatter)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            if (contextAttributes == null)
                throw new ArgumentNullException(nameof(contextAttributes));
            if(formatter == null)
                throw new ArgumentNullException(nameof(formatter));
            V1CorrelationContext correlationContext = new();
            correlationContext.BootstrapEvents.Add(V1CloudEvent.CreateFor(e, formatter));
            foreach (string attributeKey in contextAttributes)
            {
                if (!e.TryGetAttribute(attributeKey, out string attributeValue))
                    throw new InvalidOperationException($"The cloud event with id '{e.Id}' does not define the required context attribute '{attributeKey}'");
                correlationContext.ContextAttributes.Add(attributeKey, attributeValue);
            }
            return correlationContext;
        }

    }

}
