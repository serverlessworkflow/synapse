using Microsoft.JSInterop;

namespace Synapse.Dashboard.Services
{
    /// <inheritdoc/>
    public class YamlService
        : IYamlService
    {
        protected readonly IJSRuntime jsRuntime;
        protected IJSInProcessObjectReference? yamlModule = null;

        public YamlService(IJSRuntime jSRuntime)
        {
            this.jsRuntime = jSRuntime;
        }

        /// <inheritdoc/>
        public async Task<string> YamlToJson(string json)
        {
            if (this.yamlModule == null)
            {
                this.yamlModule = await this.jsRuntime.InvokeAsync<IJSInProcessObjectReference>("import", "./js/yaml-conversion.js");
            }
            return await this.yamlModule.InvokeAsync<string>("yamlToJson", json);
        }

        /// <inheritdoc/>
        public async Task<string> JsonToYaml(string yaml)
        {
            if (this.yamlModule == null)
            {
                this.yamlModule = await this.jsRuntime.InvokeAsync<IJSInProcessObjectReference>("import", "./js/yaml-conversion.js");
            }
            return await this.yamlModule.InvokeAsync<string>("jsonToYaml", yaml);
        }

    }
}
