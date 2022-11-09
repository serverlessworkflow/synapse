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

using CloudNative.CloudEvents;
using System.Net.Mime;

namespace Synapse.Integration.Models
{

    public partial class V1Event
    {

        /// <summary>
        /// Gets an <see cref="IReadOnlyDictionary{TKey, TValue}"/> containing all the <see cref="V1Event"/> attributes
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual IReadOnlyDictionary<string, string> Attributes
        {
            get
            {
                return this.AttributesEnumerator.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
        }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        IEnumerable<KeyValuePair<string, string>> AttributesEnumerator
        {
            get
            {
                yield return new KeyValuePair<string, string>(nameof(Id).ToLower(), this.Id);
                yield return new KeyValuePair<string, string>(nameof(Source).ToLower(), this.Source == null ? null : this.Source.ToString());
                yield return new KeyValuePair<string, string>(nameof(SpecVersion).ToLower(), this.SpecVersion);
                yield return new KeyValuePair<string, string>(nameof(Type).ToLower(), this.Type);
                if (!string.IsNullOrEmpty(this.DataContentType))
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
        /// Gets an <see cref="IReadOnlyDictionary{TKey, TValue}"/> containing the default context <see cref="V1Event"/> attributes
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual IReadOnlyDictionary<string, string> ContextAttributes
        {
            get
            {
                return this.ContextAttributesEnumerator.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());
            }
        }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        IEnumerable<KeyValuePair<string, string>> ContextAttributesEnumerator
        {
            get
            {
                yield return new KeyValuePair<string, string>(nameof(Id).ToLower(), this.Id);
                yield return new KeyValuePair<string, string>(nameof(Source).ToLower(), this.Source.ToString());
                yield return new KeyValuePair<string, string>(nameof(SpecVersion).ToLower(), this.SpecVersion);
                yield return new KeyValuePair<string, string>(nameof(Type).ToLower(), this.Type);
                if (!string.IsNullOrEmpty(this.DataContentType))
                    yield return new KeyValuePair<string, string>(nameof(DataContentType).ToLower(), this.DataContentType);
                if (this.DataSchema != null)
                    yield return new KeyValuePair<string, string>(nameof(DataSchema).ToLower(), this.DataSchema.ToString());
                if (!string.IsNullOrEmpty(this.Subject))
                    yield return new KeyValuePair<string, string>(nameof(Subject).ToLower(), this.Subject);
                if (this.Time.HasValue)
                    yield return new KeyValuePair<string, string>(nameof(Time).ToLower(), this.Time.ToString()!);
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
        /// Sets the specified attribute
        /// </summary>
        /// <param name="name">The name of the attribute to set</param>
        /// <param name="value">The attribute's name</param>
        public void SetAttribute(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (name.Equals(nameof(V1Event.DataContentType), StringComparison.OrdinalIgnoreCase)) this.DataContentType = value;
            else if (name.Equals(nameof(V1Event.DataSchema), StringComparison.OrdinalIgnoreCase)) this.DataSchema = string.IsNullOrWhiteSpace(value) ? null : new(value);
            else if (name.Equals(nameof(V1Event.Id), StringComparison.OrdinalIgnoreCase)) this.Id = value;
            else if (name.Equals(nameof(V1Event.Source), StringComparison.OrdinalIgnoreCase)) this.Source = (string.IsNullOrWhiteSpace(value) ? null : new(value))!;
            else if (name.Equals(nameof(V1Event.SpecVersion), StringComparison.OrdinalIgnoreCase)) this.SpecVersion = value;
            else if (name.Equals(nameof(V1Event.Subject), StringComparison.OrdinalIgnoreCase)) this.Subject = value;
            else if (name.Equals(nameof(V1Event.Time), StringComparison.OrdinalIgnoreCase)) this.Time = string.IsNullOrWhiteSpace(value) ? null : DateTime.Parse(value);
            else if (name.Equals(nameof(V1Event.Type), StringComparison.OrdinalIgnoreCase)) this.Type = value;
            else
            {
                var dynamicValue = value == null ? null : Dynamic.FromObject(value);
                if (this.ExtensionAttributes == null) this.ExtensionAttributes = new();
                this.ExtensionAttributes[name.ToLowerInvariant()] = dynamicValue;
            }
        }

        /// <summary>
        /// Creates a new <see cref="V1Event"/>
        /// </summary>
        /// <returns>A new <see cref="V1Event"/></returns>
        public static V1Event Create()
        {
            return new() { Id = Guid.NewGuid().ToString(), SpecVersion = CloudEventsSpecVersion.Default.VersionId, DataContentType = MediaTypeNames.Application.Json };
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
            var e = new V1Event()
            {
                SpecVersion = cloudEvent.SpecVersion.VersionId,
                Id = cloudEvent.Id,
                Source = cloudEvent.Source!,
                Type = cloudEvent.Type!,
                DataContentType = cloudEvent.DataContentType,
                DataSchema = cloudEvent.DataSchema,
                Subject = cloudEvent.Subject,
                Time = cloudEvent.Time?.DateTime,
                Data = cloudEvent.Data == null ? null : Dynamic.FromObject(cloudEvent.Data)
            };
            if(cloudEvent.ExtensionAttributes != null)
            {
                e.ExtensionAttributes = new();
                foreach (var extensionsAttribute in cloudEvent.ExtensionAttributes)
                {
                    e.ExtensionAttributes.Add(extensionsAttribute.Name, Dynamic.FromObject(extensionsAttribute.Format(cloudEvent[extensionsAttribute]!)));
                }
            }
            return e;
        }

        /// <summary>
        /// Converts the <see cref="V1Event"/> into a new <see cref="CloudEvent"/>
        /// </summary>
        /// <returns>A new <see cref="CloudEvent"/></returns>
        public CloudEvent ToCloudEvent()
        {
            var e = new CloudEvent()
            {
                Id = this.Id,
                Source = this.Source,
                Type = this.Type,
                Subject = this.Subject,
                Time = this.Time,
                DataContentType = this.DataContentType,
                DataSchema = this.DataSchema,
                Data = this.Data?.ToObject()
            };
            if(this.ExtensionAttributes != null)
            {
                foreach (var attribute in this.ExtensionAttributes)
                {
                    e[attribute.Key] = attribute.Value.ToObject();
                }
            }
            return e;
        }

    }

}
