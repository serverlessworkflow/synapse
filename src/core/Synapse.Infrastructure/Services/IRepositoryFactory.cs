namespace Synapse.Infrastructure.Services
{

    /// <summary>
    /// Defines the fundamentals of a service used to create <see cref="IRepository"/> instances
    /// </summary>
    public interface IRepositoryFactory
    {

        /// <summary>
        /// Creates a new <see cref="IRepository"/>
        /// </summary>
        /// <param name="entityType">The type of entity to create the <see cref="IRepository"/> for</param>
        /// <param name="keyType">The type of key used to uniquely identify the entities managed by the <see cref="IRepository"/> to create</param>
        /// <param name="modelType">The type of the application model to create a new <see cref="IRepository"/> for</param>
        /// <returns>A new <see cref="IRepository"/></returns>
        IRepository CreateRepository(Type entityType, Type keyType, ApplicationModelType modelType);

    }

}
