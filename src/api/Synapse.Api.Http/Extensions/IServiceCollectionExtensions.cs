// Copyright © 2024-Present The Synapse Authors
//
// Licensed under the Apache License, Version 2.0 (the "License"),
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Neuroglia;
using Neuroglia.Security.Services;
using Synapse.Api.Http.Controllers;
using Synapse.Core.Api.Services;
using Synapse.Resources;
using System.Text.Json;

namespace Synapse.Api.Http;

/// <summary>
/// Defines extensions for <see cref="IServiceCollection"/>s
/// </summary>
public static class IServiceCollectionExtensions
{

    /// <summary>
    /// Adds and configures the Synapse HTTP API and its related services
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
    /// <returns>The configured <see cref="IServiceCollection"/></returns>
    public static IServiceCollection AddSynapseHttpApi(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserAccessor, HttpContextUserAccessor>();
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            })
            .AddApplicationPart(typeof(WorkflowsController).Assembly);
        services.AddSignalR();
        services.AddSingleton<ResourceWatchEventHubController>();
        services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<ResourceWatchEventHubController>());
        services.AddSwaggerGen(builder =>
        {
            builder.CustomOperationIds(o =>
            {
                var action = (ControllerActionDescriptor)o.ActionDescriptor;
                return $"{action.ActionName}".ToCamelCase();
            });
            builder.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            builder.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Synapse REST API",
                Version = "v1",
                Description = "The Open API documentation for the Synapse REST API",
                License = new OpenApiLicense()
                {
                    Name = "Apache-2.0",
                    Url = new("https://raw.githubusercontent.com/synapse/dsl/main/LICENSE")
                },
                Contact = new()
                {
                    Name = "The Synapse Authors",
                    Url = new Uri("https://github.com/serverlessworkflow/synapse")
                }
            });
            builder.IncludeXmlComments(typeof(Workflow).Assembly.Location.Replace(".dll", ".xml"));
        });
        return services;
    }

}
