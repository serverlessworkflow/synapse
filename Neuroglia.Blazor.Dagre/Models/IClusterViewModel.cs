namespace Neuroglia.Blazor.Dagre.Models
{
    public interface IClusterViewModel
        : INodeViewModel
    {
        ICollection<INodeViewModel> Children { get; set; }
    }
}
