using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.NewtonsoftJson;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Synapse.Application.Configuration;
using Synapse.Application.Services;

namespace Synapse.Ports.HttpRest
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
        public static ISynapseApplicationBuilder UseHttpRestApi(this ISynapseApplicationBuilder synapse)
        {
            synapse.Services
                .AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
                    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.NonPublicSetterContractResolver();
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                })
                .AddOData((options, provider) =>
                {
                    IEdmModelBuilder builder = provider.GetRequiredService<IEdmModelBuilder>();
                    options.AddRouteComponents("api/odata", builder.Build())
                        .EnableQueryFeatures(50);
                    options.RouteOptions.EnableControllerNameCaseInsensitive = true;
                })
                .AddODataNewtonsoftJson()
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
                builder.IncludeXmlComments(typeof(V1WorkflowDto).Assembly.Location.Replace(".dll", ".xml"));
            });
            return synapse;
        }

    }

}
