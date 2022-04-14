using Neuroglia.Blazor.Dagre.Models;

namespace Neuroglia.Blazor.Dagre
{
    public interface IDagreService
        : IGraphLibJsonConverter
    {
        Task<IGraphViewModel> ComputePositionsAsync(IGraphViewModel graphViewModel, IDagreGraphOptions? options = null);
        Task<IGraphLib> GraphAsync(IDagreGraphOptions? options = null);
        Task<IGraphLib?> LayoutAsync(IGraphLib graph);
    }
}
