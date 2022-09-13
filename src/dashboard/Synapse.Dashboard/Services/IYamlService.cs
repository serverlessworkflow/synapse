using Microsoft.JSInterop;

namespace Synapse.Dashboard.Services
{
    /// <summary>
    /// A service used to manipulate YAML
    /// </summary>
    public interface IYamlService
    {
        /// <summary>
        /// Converts the provided JSON string to YAML
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        Task<string> YamlToJson(string json);

        /// <summary>
        /// Converts the provided YAML string to JSON
        /// </summary>
        /// <param name="yaml"></param>
        /// <returns></returns>
        Task<string> JsonToYaml(string yaml);

    }
}
