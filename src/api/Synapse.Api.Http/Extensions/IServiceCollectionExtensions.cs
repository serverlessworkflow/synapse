﻿// Copyright © 2024-Present The Synapse Authors
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
using Synapse.Api.Application;
using Synapse.Api.Application.Services;
using Synapse.Api.Http.Controllers;
using Synapse.Core.Api.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    /// <param name="authority">The API's JWT authority</param>
    /// <returns>The configured <see cref="IServiceCollection"/></returns>
    public static IServiceCollection AddSynapseHttpApi(this IServiceCollection services, string? authority = null)
    {
        ServiceAccountSigningKey.Initialize();
        services.AddHttpContextAccessor();
        services.AddScoped<IUserAccessor, HttpContextUserAccessor>();
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull | JsonIgnoreCondition.WhenWritingDefault;
            })
            .AddApplicationPart(typeof(WorkflowsController).Assembly);
        services.AddIdentityServer(options =>
        {
            if (!string.IsNullOrWhiteSpace(authority)) options.IssuerUri = authority;
        })
            .AddSigningCredential(ServiceAccountSigningKey.LoadPrivateKey())
            .AddInMemoryApiResources(SynapseApiDefaults.OpenIDConnect.ApiResources.AsEnumerable())
            .AddInMemoryIdentityResources(SynapseApiDefaults.OpenIDConnect.IdentityResources.AsEnumerable())
            .AddInMemoryApiScopes(SynapseApiDefaults.OpenIDConnect.ApiScopes.AsEnumerable())
            .AddClientStore<ServiceAccountClientStore>()
            .AddProfileService<ServiceAccountProfileService>();
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
            builder.AddSecurityDefinition("Static Token", new OpenApiSecurityScheme
            {
                Description = "Static token authorization using the Bearer scheme",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "Static Token"
            });
            builder.AddSecurityRequirement(new()
            {
                {
                    new()
                    {
                        Reference = new()
                        {
                            Id = "Static Token",
                            Type = ReferenceType.SecurityScheme
                        }
                    },
                    Array.Empty<string>()
                }
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