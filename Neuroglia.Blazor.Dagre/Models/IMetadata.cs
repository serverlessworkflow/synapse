namespace Neuroglia.Blazor.Dagre.Models
{
    public interface IMetadata
    {
        /// <summary>
        /// The metadata payload
        /// </summary>
        IDictionary<string, object>? Metadata { get; set; }
    }
}
