namespace Neuroglia.Blazor.Dagre.Models
{
    public interface IClusterViewModel
        : INodeViewModel
    {
        IDictionary<Guid, INodeViewModel> Children { get; set; }
    }
}
