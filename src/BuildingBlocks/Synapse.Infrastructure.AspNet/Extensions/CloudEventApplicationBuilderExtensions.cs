using CloudNative.CloudEvents;
using Microsoft.AspNetCore.Builder;
using Synapse.Infrastructure.Middlewares;

namespace Synapse
{

    /// <summary>
    /// Defines <see cref="CloudEvent"/>-related extensions for <see cref="IApplicationBuilder"/>s
    /// </summary>
    public static class CloudEventApplicationBuilderExtensions
    {

        /// <summary>
        /// Configures the <see cref="IApplicationBuilder"/> to use the <see cref="CloudEventsMiddleware"/>
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to configure</param>
        /// <returns>The configured <see cref="IApplicationBuilder"/></returns>
        public static IApplicationBuilder UseCloudEvents(this IApplicationBuilder app)
        {
            app.UseMiddleware<CloudEventsMiddleware>();
            return app;
        }

    }

}
