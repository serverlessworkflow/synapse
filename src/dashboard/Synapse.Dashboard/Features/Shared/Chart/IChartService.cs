using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Synapse.Dashboard
{
    interface IChartService
    {
        Task<IJSObjectReference> CreateChartAsync(ElementReference canvasRef, ChartConfiguration configuration);
    }
}
