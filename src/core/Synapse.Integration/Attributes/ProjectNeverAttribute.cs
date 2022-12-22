namespace Synapse
{
    /// <summary>
    /// Represents an <see cref="Attribute"/> used to ignore the projection of a marked property
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ProjectNeverAttribute
        : QueryableAttribute
    {



    }

}
