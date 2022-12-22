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

using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Text.RegularExpressions;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents an event
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Models.V1Event))]
    public class V1Event
    {

        /// <summary>
        /// Initializes a new <see cref="V1Event"/>
        /// </summary>
        protected V1Event()
        {
            this.Id = null!;
            this.Source = null!;
            this.Type = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1Event"/>
        /// </summary>
        /// <param name="id">The event's id</param>
        /// <param name="source">The event's source <see cref="Uri"/></param>
        /// <param name="type">event's type</param>
        /// <param name="specVersion">The event's spec version</param>
        public V1Event(string id, Uri source, string type, string? specVersion)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrEmpty(type))
                throw new ArgumentNullException(nameof(type));
            if (specVersion == null)
                specVersion = CloudEventsSpecVersion.V1_0.VersionId;
            this.Id = id;
            this.Source = source;
            this.SpecVersion = specVersion;
            this.Type = type;
        }

        /// <summary>
        /// Gets the event's <see href="https://github.com/cloudevents/spec/blob/v1.0.2/cloudevents/spec.md#id">id</see>
        /// </summary>
        public virtual string Id { get; protected set; }

        /// <summary>
        /// Gets the event's <see href="https://github.com/cloudevents/spec/blob/v1.0.2/cloudevents/spec.md#source">source</see> <see cref="Uri"/>
        /// </summary>
        public virtual Uri Source { get; protected set; }

        /// <summary>
        /// Gets the event's <see href="https://github.com/cloudevents/spec/blob/v1.0.2/cloudevents/spec.md#specversion">spec version</see>
        /// </summary>
        public virtual string SpecVersion { get; protected set; } = CloudEventsSpecVersion.V1_0.VersionId;

        /// <summary>
        /// Gets the event's <see href="https://github.com/cloudevents/spec/blob/v1.0.2/cloudevents/spec.md#type">type</see>
        /// </summary>
        public virtual string Type { get; protected set; }

        /// <summary>
        /// Gets the event's data content type
        /// </summary>
        public virtual string? DataContentType { get; protected set; }

        /// <summary>
        /// Gets the event's data schema <see cref="Uri"/>, if any
        /// </summary>
        public virtual Uri? DataSchema { get; protected set; }

        /// <summary>
        /// Gets the event's subject
        /// </summary>
        public virtual string? Subject { get; protected set; }

        /// <summary>
        /// Gets the event's type
        /// </summary>
        public virtual DateTimeOffset? Time { get; protected set; }

        /// <summary>
        /// Gets the event's data
        /// </summary>
        public virtual Neuroglia.Serialization.Dynamic? Data { get; protected set; }

        /// <summary>
        /// Gets an <see cref="IDictionary{TKey, TValue}"/> that contains the event's extension attributes key/value mappings
        /// </summary>
        [Newtonsoft.Json.JsonExtensionData]
        [System.Text.Json.Serialization.JsonExtensionData]
        public virtual IDictionary<string, object> ExtensionAttributes { get; protected set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets an <see cref="IReadOnlyDictionary{TKey, TValue}"/> containing all the <see cref="V1Event"/> attributes
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual IReadOnlyDictionary<string, string> Attributes
        {
            get
            {
                return this.AttributesEnumerator.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());
            }
        }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        IEnumerable<KeyValuePair<string, string>> AttributesEnumerator
        {
            get
            {
                yield return new KeyValuePair<string, string>(nameof(Id).ToLower(), this.Id);
                yield return new KeyValuePair<string, string>(nameof(Source).ToLower(), this.Source.ToString());
                yield return new KeyValuePair<string, string>(nameof(SpecVersion).ToLower(), this.SpecVersion);
                yield return new KeyValuePair<string, string>(nameof(Type).ToLower(), this.Type);
                if(!string.IsNullOrEmpty(this.DataContentType))
                    yield return new KeyValuePair<string, string>(nameof(DataContentType).ToLower(), this.DataContentType);
                if (this.DataSchema != null)
                    yield return new KeyValuePair<string, string>(nameof(DataSchema).ToLower(), this.DataSchema.ToString());
                if (!string.IsNullOrEmpty(this.Subject))
                    yield return new KeyValuePair<string, string>(nameof(Subject).ToLower(), this.Subject);
                if (this.Time.HasValue)
                    yield return new KeyValuePair<string, string>(nameof(Time).ToLower(), this.Time.ToString()!);
                if(this.ExtensionAttributes != null)
                {
                    foreach (var extension in this.ExtensionAttributes)
                        yield return new(extension.Key, extension.Value.ToString()!);
                }
            }
        }

        /// <summary>
        /// Attempts to get the attribute with the specified name
        /// </summary>
        /// <param name="name">The name of the attribute to get</param>
        /// <param name="value">The value of the attribute, if any</param>
        /// <returns>A boolean indicating whether or not the attribute with the specified name is defined</returns>
        public virtual bool TryGetAttribute(string name, out string value)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            return this.Attributes.TryGetValue(name, out value!);
        }

        /// <summary>
        /// Determines whether or not the <see cref="V1Event"/> matches the specified <see cref="EventDefinition"/>
        /// </summary>
        /// <param name="eventDefinition">The <see cref="EventDefinition"/> to match</param>
        /// <returns>A boolean indicating whether or not the <see cref="V1Event"/> matches the specified <see cref="EventDefinition"/></returns>
        public virtual bool Matches(EventDefinition eventDefinition)
        {
            if (eventDefinition == null)
                throw new ArgumentNullException(nameof(eventDefinition));
            if (!string.IsNullOrWhiteSpace(eventDefinition.Source) 
                && !Regex.IsMatch(this.Source.ToString(), eventDefinition.Source, RegexOptions.IgnoreCase))
                return false;
            if (!string.IsNullOrWhiteSpace(eventDefinition.Type) 
                && !Regex.IsMatch(this.Type, eventDefinition.Type, RegexOptions.IgnoreCase))
                return false;
            if (eventDefinition.Correlations != null)
            {
                foreach (EventCorrelationDefinition correlationDefinition in eventDefinition.Correlations)
                {
                    if (!this.TryGetAttribute(correlationDefinition.ContextAttributeName, out string value)) return false;
                    if (string.IsNullOrWhiteSpace(correlationDefinition.ContextAttributeValue)) continue;
                    if (!Regex.IsMatch(value, correlationDefinition.ContextAttributeValue, RegexOptions.IgnoreCase)) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Creates a new <see cref="V1Event"/> for the specified <see cref="CloudEvent"/>
        /// </summary>
        /// <param name="cloudEvent">The <see cref="CloudEvent"/> to create a new <see cref="V1Event"/> for</param>
        /// <returns>A new <see cref="V1Event"/></returns>
        public static V1Event CreateFrom(CloudEvent cloudEvent)
        {
            if (cloudEvent == null)
                throw new ArgumentNullException(nameof(cloudEvent));
            var e = new V1Event(cloudEvent.Id!, cloudEvent.Source!, cloudEvent.Type!, cloudEvent.SpecVersion.VersionId)
            {
                DataContentType = cloudEvent.DataContentType,
                DataSchema = cloudEvent.DataSchema,
                Subject = cloudEvent.Subject,
                Time = cloudEvent.Time
            };
            var data = cloudEvent.Data;
            if (data is JObject jobject)
                data = jobject.ToObject<ExpandoObject>();
            e.Data = Neuroglia.Serialization.Dynamic.FromObject(data);
            foreach(var extensionsAttribute in cloudEvent.ExtensionAttributes)
            {
                e.ExtensionAttributes.Add(extensionsAttribute.Name, extensionsAttribute.Format(cloudEvent[extensionsAttribute]!));
            }
            return e;
        }

    }

}
