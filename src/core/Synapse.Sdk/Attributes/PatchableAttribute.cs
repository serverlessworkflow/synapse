namespace Synapse.Sdk
{
    /// <summary>
    /// Represents an <see cref="Attribute"/> used to mark a class as patchable
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PatchableAttribute
        : Attribute
    {



    }

}
