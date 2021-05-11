using CloudNative.CloudEvents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerlessWorkflow.Sdk.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents a wrapper for an encoded <see cref="CloudEvent"/>
    /// </summary>
    public class V1CloudEvent
    {

        /// <summary>
        /// Gets the wrapped <see cref="CloudEvent"/>'s id
        /// </summary>
        [JsonIgnore]
        public string Id
        {
            get
            {
                if (!this.TryGetAttribute(nameof(Id).ToLower(), out string value))
                    return default;
                return value;
            }
        }

        /// <summary>
        /// Gets the wrapped <see cref="CloudEvent"/>'s type
        /// </summary>
        [JsonIgnore]
        public string Type
        {
            get
            {
                if (!this.TryGetAttribute(CloudEventAttributes.TypeAttributeName(), out string value))
                    return default;
                return value;
            }
        }

        /// <summary>
        /// Gets the wrapped <see cref="CloudEvent"/>'s source
        /// </summary>
        [JsonIgnore]
        public Uri Source
        {
            get
            {
                if (!this.TryGetAttribute(CloudEventAttributes.SourceAttributeName(), out string value))
                    return default;
                return new Uri(value, UriKind.RelativeOrAbsolute);
            }
        }

        /// <summary>
        /// Gets an <see cref="IDictionary{TKey, TValue}"/> representing the wrapped <see cref="CloudEvent"/>'s attributes
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, JToken> Attributes { get; set; } = new Dictionary<string, JToken>();

        /// <summary>
        /// Attempts to retrieve the specified attribute
        /// </summary>
        /// <param name="key">The key of the attribute to retrieve</param>
        /// <param name="value">The attribute's value, if any</param>
        /// <returns>A boolean indicating whether or not the specified attribute could be retrieved</returns>
        public virtual bool TryGetAttribute(string key, out string value)
        {
            value = null;
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            if (!this.Attributes.TryGetValue(key, out JToken jvalue))
                return false;
            value = jvalue.Value<string>();
            if (value.StartsWith("\"") && value.EndsWith("\""))
                value = value[1..^1];
            return true;
        }

        /// <summary>
        /// Determines whether or not the <see cref="V1CloudEvent"/> matches the specified <see cref="EventDefinition"/>
        /// </summary>
        /// <param name="eventDefinition">The <see cref="EventDefinition"/> to match</param>
        /// <returns>A boolean indicating whether or not the <see cref="V1CloudEvent"/> matches the specified <see cref="EventDefinition"/></returns>
        public virtual bool Matches(EventDefinition eventDefinition)
        {
            if (eventDefinition == null)
                throw new ArgumentNullException(nameof(eventDefinition));
            if (!string.IsNullOrWhiteSpace(eventDefinition.Source) && !Regex.IsMatch(this.Source.ToString(), eventDefinition.Source))
                return false;
            if (!string.IsNullOrWhiteSpace(eventDefinition.Type) && !Regex.IsMatch(this.Type, eventDefinition.Type))
                return false;
            if(eventDefinition.Correlations != null)
            {
                foreach (EventCorrelationDefinition correlationDefinition in eventDefinition.Correlations)
                {
                    if (!this.TryGetAttribute(correlationDefinition.ContextAttributeName, out string value))
                        return false;
                    if (string.IsNullOrWhiteSpace(correlationDefinition.ContextAttributeValue))
                        continue;
                    if (!Regex.IsMatch(value, correlationDefinition.ContextAttributeValue))
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Converts the <see cref="V1CloudEvent"/> into a new <see cref="CloudEvent"/>
        /// </summary>
        /// <param name="formatter">The service used to format the <see cref="CloudEvent"/></param>
        /// <returns>A new <see cref="CloudEvent"/></returns>
        public virtual CloudEvent ToCloudEvent(ICloudEventFormatter formatter)
        {
            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));
            return formatter.DecodeStructuredEvent(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this)), Array.Empty<ICloudEventExtension>());
        }

        /// <summary>
        /// Creates a new <see cref="V1CloudEvent"/> for the specified <see cref="CloudEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="CloudEvent"/> to create a new <see cref="V1CloudEvent"/> for</param>
        /// <param name="formatter">The service used to encode the specified <see cref="CloudEvent"/></param>
        /// <returns>A new <see cref="V1CloudEvent"/></returns>
        public static V1CloudEvent CreateFor(CloudEvent e, ICloudEventFormatter formatter)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));
            V1CloudEvent wrapper = new();
            foreach(KeyValuePair<string, object> attribute in e.GetAttributes())
            {
                wrapper.Attributes.Add(attribute.Key, Encoding.UTF8.GetString(formatter.EncodeAttribute(e.SpecVersion, attribute.Key, attribute.Value, Array.Empty<ICloudEventExtension>())));
            }
            return wrapper;
        }

    }

}
