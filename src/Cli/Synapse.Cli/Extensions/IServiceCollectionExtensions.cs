using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine;
using System.Linq;

namespace Synapse.Cli
{

    /// <summary>
    /// Defines extensions for <see cref="IServiceCollection"/>s
    /// </summary>
    public static class IServiceCollectionExtensions
    {

        /// <summary>
        /// Adds and configures all <see cref="Command"/>s loaded from the specified assemblies
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
        /// <param name="markupTypes">An array containing the markup types of the assemblies to load <see cref="Command"/>s from</param>
        /// <returns>The configured <see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddCliCommands(this IServiceCollection services, params Type[] markupTypes)
        {
            foreach(Type commandType in markupTypes
                .Select(t => t.Assembly)
                .Distinct()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsInterface && !t.IsGenericType && typeof(Command).IsAssignableFrom(t)))
            {
                services.AddSingleton(typeof(Command), commandType);
            }
            return services;
        }

    }

}
