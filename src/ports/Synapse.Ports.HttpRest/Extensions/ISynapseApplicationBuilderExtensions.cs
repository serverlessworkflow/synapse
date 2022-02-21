using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.NewtonsoftJson;
using Microsoft.AspNetCore.OData.Routing.Controllers;
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
            return synapse;
        }

    }

}
