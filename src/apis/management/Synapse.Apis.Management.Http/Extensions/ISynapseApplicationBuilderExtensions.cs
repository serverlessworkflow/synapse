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

using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.NewtonsoftJson;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Synapse.Apis.Management.Http.Services;
using Synapse.Application.Configuration;
using Synapse.Application.Services;

namespace Synapse.Apis.Management.Http
{

    /// <summary>
    /// Defines extensions for <see cref="ISynapseApplicationBuilder"/>s
    /// </summary>
    public static class ISynapseApplicationBuilderExtensions
    {

        /// <summary>
        /// Configures Synapse to use the Http REST API port
        /// </summary>
        /// <param name="synapse">The <see cref="ISynapseApplicationBuilder"/> to configure</param>
        /// <returns>The configured <see cref="ISynapseApplicationBuilder"/></returns>
        public static ISynapseApplicationBuilder UseHttpManagementApi(this ISynapseApplicationBuilder synapse)
        {
            synapse.Services
                .AddControllers()
                .AddOData((options, provider) =>
                {
                    IEdmModelBuilder builder = provider.GetRequiredService<IEdmModelBuilder>();
                    options.AddRouteComponents("api/odata", builder.Build(), services => services.AddSingleton<ISearchBinder, ODataSearchBinder>())
                        .EnableQueryFeatures(50);
                    options.RouteOptions.EnableControllerNameCaseInsensitive = true;
                })
                .AddODataNewtonsoftJson()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new NonPublicSetterContractResolver();
                    options.SerializerSettings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore;
                    options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                })
                .AddApplicationPart(typeof(ISynapseApplicationBuilderExtensions).Assembly)
                .AddApplicationPart(typeof(MetadataController).Assembly);
            synapse.Services.AddSwaggerGen(builder =>
            {
                builder.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                builder.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Synapse API",
                    Version = "v1",
                    Description = "The Open API documentation for the Synapse API",
                    Contact = new()
                    {
                        Name = "The Synapse Authors",
                        Url = new Uri("https://github.com/serverlessworkflow/synapse/")
                    }
                });
                builder.IncludeXmlComments(typeof(ISynapseApplicationBuilderExtensions).Assembly.Location.Replace(".dll", ".xml"));
                builder.IncludeXmlComments(typeof(Integration.Models.V1Workflow).Assembly.Location.Replace(".dll", ".xml"));
            });
            return synapse;
        }

    }

}
