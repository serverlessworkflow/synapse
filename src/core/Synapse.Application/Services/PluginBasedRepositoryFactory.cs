using Synapse.Infrastructure;
using Synapse.Infrastructure.Plugins;

namespace Synapse.Application.Services
{

    /// <summary>
    /// Represents the default, <see cref="IRepositoryPlugin"/>-based implementation of the <see cref="IRepositoryFactory"/> interface
    /// </summary>
    public class PluginBasedRepositoryFactory
        : IRepositoryFactory
    {

        /// <summary>
        /// Initializes a new <see cref="PluginBasedRepositoryFactory"/>
        /// </summary>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
        /// <param name="pluginManager">A service used to manage the application's <see cref="IPlugin"/>s</param>
        /// <param name="options">The service used to access the current <see cref="SynapseApplicationOptions"/></param>
        public PluginBasedRepositoryFactory(IServiceProvider serviceProvider, IPluginManager pluginManager, IOptions<SynapseApplicationOptions> options)
        {
            this.ServiceProvider = serviceProvider;
            this.PluginManager = pluginManager;
            this.Options = options.Value;
        }

        /// <summary>
        /// Gets the current <see cref="IServiceProvider"/>
        /// </summary>
        protected IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the service used to manage the application's <see cref="IPlugin"/>s
        /// </summary>
        protected IPluginManager PluginManager { get; }

        /// <summary>
        /// Gets the current <see cref="SynapseApplicationOptions"/>
        /// </summary>
        protected SynapseApplicationOptions Options { get; }

        /// <inheritdoc/>
        public virtual IRepository CreateRepository(Type entityType, Type keyType, ApplicationModelType modelType)
        {
            if(entityType == null)
                throw new ArgumentNullException(nameof(entityType));
            if (keyType == null)
                throw new ArgumentNullException(nameof(keyType));
            var pluginName = modelType switch
            {
                ApplicationModelType.WriteModel => this.Options.Persistence.DefaultWriteModelRepository?.PluginName,
                ApplicationModelType.ReadModel => this.Options.Persistence.DefaultReadModelRepository?.PluginName,
                _ => throw new NotSupportedException($"The specified {nameof(ApplicationModelType)} '{modelType}' is not supported")
            };
            if (this.Options.Persistence.Repositories.TryGetValue(entityType.FullName!, out var repositoryOptions))
                pluginName = repositoryOptions.PluginName;
            if (string.IsNullOrWhiteSpace(pluginName)
                || !this.PluginManager.TryGetPlugin<IRepositoryPlugin>(pluginName, out var factory))
                return (IRepository)ActivatorUtilities.CreateInstance(this.ServiceProvider, typeof(DistributedCacheRepository<,>).MakeGenericType(entityType, keyType));
            return factory.CreateRepository(entityType, keyType);
        }

    }

}
