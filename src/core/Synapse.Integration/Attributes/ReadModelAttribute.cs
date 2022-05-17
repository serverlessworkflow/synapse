namespace Synapse
{
    /// <summary>
    /// Represents an <see cref="Attribute"/> used to mark an entity as a read model
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ReadModelAttribute
        : QueryableAttribute
    {



    }

}
