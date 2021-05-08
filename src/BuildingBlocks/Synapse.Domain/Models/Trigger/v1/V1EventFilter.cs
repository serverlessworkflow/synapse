using CloudNative.CloudEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents an object used to filter <see cref="CloudEvent"/>s
    /// </summary>
    public class V1EventFilter
    {

        /// <summary>
        /// Initializes a new <see cref="V1EventFilter"/>
        /// </summary>
        public V1EventFilter()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1EventFilter"/>
        /// </summary>
        /// <param name="contextAttributes">An <see cref="IDictionary{TKey, TValue}"/> containing the context attributes to filter <see cref="CloudEvent"/>s by</param>
        public V1EventFilter(IDictionary<string, string> contextAttributes)
            : this()
        {
            this.ContextAttributes = contextAttributes ?? throw new ArgumentNullException(nameof(contextAttributes));
        }

        /// <summary>
        /// Gets an <see cref="IDictionary{TKey, TValue}"/> containing the context attributes to filter <see cref="CloudEvent"/>s by
        /// </summary>
        [JsonProperty("contextAttributes")]
        public IDictionary<string, string> ContextAttributes { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets an <see cref="IDictionary{TKey, TValue}"/> containing the attributes key/value to use when correlating an incoming <see cref="CloudEvent"/> to the <see cref="V1Trigger"/>
        /// </summary>
        [JsonProperty("correlations")]
        public IDictionary<string, string> Correlations { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Determines whether or not the <see cref="V1EventFilter"/> filters the specified <see cref="CloudEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="CloudEvent"/> to filter</param>
        /// <returns>A boolean indicating whether or not the <see cref="V1EventFilter"/> filters the specified <see cref="CloudEvent"/></returns>
        public virtual bool Filters(CloudEvent e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            foreach (KeyValuePair<string, string> attribute in this.ContextAttributes)
            {
                if (!e.TryGetAttribute(attribute.Key, out object value))
                    return false;
                if (attribute.Value != value?.ToString())
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether or not the <see cref="V1EventFilter"/> filters the specified <see cref="V1CloudEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1CloudEvent"/> to filter</param>
        /// <returns>A boolean indicating whether or not the <see cref="V1EventFilter"/> filters the specified <see cref="V1CloudEvent"/></returns>
        public virtual bool Filters(V1CloudEvent e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            foreach (KeyValuePair<string, string> attribute in this.ContextAttributes)
            {
                if (!e.TryGetAttribute(attribute.Key, out string value))
                    return false;  
                if (attribute.Value != value)
                    return false;  
            }
            return true;
        }

    }

}
