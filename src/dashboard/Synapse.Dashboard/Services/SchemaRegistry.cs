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

using Neuroglia.Serialization;
using Newtonsoft.Json.Schema;
using System.Collections.Concurrent;

namespace Synapse.Dashboard.Services
{

    /// <summary>
    /// Represents the default implementation of the <see cref="ISchemaRegistry"/> interface
    /// </summary>
    public class SchemaRegistry
        : ISchemaRegistry
    {

        /// <summary>
        /// Initializes a new <see cref="SchemaRegistry"/>
        /// </summary>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
        /// <param name="serializer">The service used to serialize and deserialize JSON</param>
        /// <param name="httpClientFactory">The service used to create <see cref="System.Net.Http.HttpClient"/>s</param>
        public SchemaRegistry(IServiceProvider serviceProvider, IJsonSerializer serializer, IHttpClientFactory httpClientFactory)
        {
            this.ServiceProvider = serviceProvider;
            this.Serializer = serializer;
            this.HttpClient = httpClientFactory.CreateClient();
        }

        /// <summary>
        /// Gets the current <see cref="IServiceProvider"/>
        /// </summary>
        protected IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the service used to serialize and deserialize JSON
        /// </summary>
        protected IJsonSerializer Serializer { get; }

        /// <summary>
        /// Gets the <see cref="System.Net.Http.HttpClient"/> used to fetch the Serverless Workflow schema
        /// </summary>
        protected HttpClient HttpClient { get; }

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey, TValue}"/> containing the loaded <see cref="JsonSchema"/>s
        /// </summary>
        protected ConcurrentDictionary<string, JSchema> Schemas { get; } = new();

        /// <inheritdoc/>
        public virtual async Task<JSchema> GetJsonSchemaAsync(string schemaUri, CancellationToken cancellationToken = default)
        {
            if (schemaUri == null) throw new ArgumentNullException(nameof(schemaUri));
            if (this.Schemas.TryGetValue(schemaUri, out var schema))
                return schema;
            var json = await this.HttpClient.GetStringAsync(schemaUri, cancellationToken);
            schema = JSchema.Parse(json, new JSchemaReaderSettings() { ResolveSchemaReferences = false });
            this.Schemas.TryAdd(schemaUri, schema);
            return schema;
        }

    }

}
