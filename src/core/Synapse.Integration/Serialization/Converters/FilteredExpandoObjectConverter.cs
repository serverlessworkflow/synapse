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

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Dynamic;

namespace Synapse.Integration.Serialization.Converters
{

    /// <summary>
    /// Represents the service used to convert <see cref="ExpandoObject"/>s
    /// </summary>
    public class FilteredExpandoObjectConverter 
        : ExpandoObjectConverter
    {

        /// <summary>
        /// Initializes a new <see cref="FilteredExpandoObjectConverter"/>
        /// </summary>
        /// <param name="namingStrategy">The <see cref="Newtonsoft.Json.Serialization.NamingStrategy"/> to use</param>
        public FilteredExpandoObjectConverter(NamingStrategy namingStrategy = null)
        {
            this.NamingStrategy = namingStrategy ?? new CamelCaseNamingStrategy();
        }

        /// <inheritdoc/>
        public override bool CanWrite => true;

        /// <summary>
        /// Gets the <see cref="Newtonsoft.Json.Serialization.NamingStrategy"/> to use
        /// </summary>
        protected NamingStrategy NamingStrategy { get; }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            var expando = (IDictionary<string, object>)value;
            var dictionary = expando
                .Where(p => p.Value is not null)
                .ToDictionary(p => this.NamingStrategy.GetPropertyName(p.Key, false), p => p.Value);
            serializer.Serialize(writer, dictionary);
        }

    }
}
