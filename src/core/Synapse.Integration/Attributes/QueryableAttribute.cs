namespace Synapse
{

    /// <summary>
    /// Represents an <see cref="Attribute"/> used to mark an entity as queryable
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class QueryableAttribute
        : Attribute
    {



    }

}
