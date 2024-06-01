using Neuroglia;

namespace Synapse.Cli;

/// <summary>
/// Defines extensions for <see cref="IServiceCollection"/>s
/// </summary>
public static class IServiceCollectionExtensions
{

    /// <summary>
    /// Adds and configures all <see cref="Command"/>s loaded from the specified assemblies
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
    /// <returns>The configured <see cref="IServiceCollection"/></returns>
    public static IServiceCollection AddCliCommands(this IServiceCollection services)
    {
        foreach (var commandType in TypeCacheUtil.FindFilteredTypes
        (
            "synctl-cmd",
            t => t.IsClass && !t.IsAbstract && !t.IsInterface && !t.IsGenericType && typeof(Command).IsAssignableFrom(t) && t.IsPublic,
            typeof(Commands.Command).Assembly)
        )
        {
            services.AddSingleton(typeof(Command), commandType);
        }
        return services;
    }

}

