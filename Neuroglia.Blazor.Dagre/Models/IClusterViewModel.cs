namespace Neuroglia.Blazor.Dagre.Models
{
    public interface IClusterViewModel
        : IGraphElement, INodeViewModel
    {
        IReadOnlyDictionary<Guid, INodeViewModel> Children { get; }
        IReadOnlyDictionary<Guid, INodeViewModel> AllNodes { get; }
        IReadOnlyDictionary<Guid, IClusterViewModel> AllClusters { get; }
        Task AddChildAsync(INodeViewModel node);
        event Action<INodeViewModel>? ChildAdded;
    }
}
