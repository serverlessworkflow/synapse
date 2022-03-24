namespace Neuroglia.BlazorDagre.Models
{
    public interface IClusterViewModel
        : INodeViewModel
    {
        ICollection<INodeViewModel> Children { get; set; }
    }
}
