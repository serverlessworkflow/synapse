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
    /// Represents the <see cref="IOperationFilter"/> used to ignore <see cref="ODataQueryOptions"/> parameters
    /// </summary>
    public class IgnoreODataQueryOptionOperationFilter
        : IOperationFilter
    {

        /// <inheritdoc/>
        public virtual void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            context.ApiDescription.ParameterDescriptions
                .Where(param => param.ParameterDescriptor.ParameterType.IsGenericType && param.ParameterDescriptor.ParameterType.GetGenericTypeDefinition() == typeof(ODataQueryOptions<>))
                .ToList()
                .ForEach(param =>
                {
                    var parameter = operation.Parameters.SingleOrDefault(p => p.Name == param.Name);
                    if (parameter != null) operation.Parameters.Remove(parameter);
                });
        }

    }

}
