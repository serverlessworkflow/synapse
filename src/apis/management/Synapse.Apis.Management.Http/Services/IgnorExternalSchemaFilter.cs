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

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Synapse.Apis.Management.Http.Services
{
    /// <summary>
    /// Represents the object used to filter schemas described by the generated OpenAPI documentation
    /// </summary>
    public class IgnorExternalSchemaFilter
        : ISchemaFilter, IDocumentFilter
    {

        /// <summary>
        /// Gets a <see cref="List{T}"/> containing all the schemas to include in the generated OpenAPI documentation
        /// </summary>
        internal protected static readonly List<string> IncludedSchemas = new();

        /// <inheritdoc/>
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.Namespace!.StartsWith("Neuroglia") 
                || context.Type.Namespace!.StartsWith("Synapse") 
                || context.Type.Namespace.StartsWith("ServerlessWorkflow")
                || context.Type.Namespace == "System")
                IncludedSchemas.Add(context.Type.Name);
        }

        /// <inheritdoc/>
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach(var schema in swaggerDoc.Components.Schemas.ToList())
            {
                if (IncludedSchemas.Contains(schema.Key)) continue;
                swaggerDoc.Components.Schemas.Remove(schema.Key);
            }
        }

    }

}
