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
                if (!string.IsNullOrEmpty(this.DataContentType))
                    yield return new KeyValuePair<string, string>(nameof(DataContentType).ToLower(), this.DataContentType);
                if (this.DataSchema != null)
                    yield return new KeyValuePair<string, string>(nameof(DataSchema).ToLower(), this.DataSchema.ToString());
                if (!string.IsNullOrEmpty(this.Subject))
                    yield return new KeyValuePair<string, string>(nameof(Subject).ToLower(), this.Subject);
                if (this.Time.HasValue)
                    yield return new KeyValuePair<string, string>(nameof(Time).ToLower(), this.Time.ToString()!);
                if(this.Extensions != null)
                {
                    foreach (var extension in this.Extensions)
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
                e.Extensions = new();
                foreach (var extensionsAttribute in cloudEvent.ExtensionAttributes)
                {
                    e.Extensions.Add(extensionsAttribute.Name, Dynamic.FromObject(extensionsAttribute.Format(cloudEvent[extensionsAttribute]!)));
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
            if(this.Extensions != null)
            {
                foreach (var attribute in this.Extensions)
                {
                    e[attribute.Key] = attribute.Value.ToObject();
                }
            }
            return e;
        }

    }

}
