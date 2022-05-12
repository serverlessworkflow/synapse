using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Synapse.Dashboard
{
    public class ChartService
        : IChartService
    {
        protected readonly IJSRuntime jsRuntime;
        protected IJSInProcessObjectReference? chartModule = null;

        public ChartService(IJSRuntime jSRuntime)
        {
            this.jsRuntime = jSRuntime;
        }

        public async Task<IJSObjectReference> CreateChartAsync(ElementReference canvasRef, ChartConfiguration configuration)
        {
            if (this.chartModule == null)
            {
                this.chartModule = await this.jsRuntime.InvokeAsync<IJSInProcessObjectReference>("import", "./js/chart.js");
            }
            return await this.chartModule.InvokeAsync<IJSObjectReference>("createChart", canvasRef, configuration);
        }
    }
}
