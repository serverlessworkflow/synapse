using ServerlessWorkflow.Sdk;
using Synapse.Cli.Services;
using Synapse.Domain.Models;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Synapse.Cli
{

    /// <summary>
    /// Defines extensions for <see cref="ISynapseService"/>s
    /// </summary>
    public static class ISynapseServiceExtensions
    {

        /// <summary>
        /// Reads a <see cref="V1Workflow"/> from the specified file
        /// </summary>
        /// <param name="synapse">The extended <see cref="ISynapseService"/></param>
        /// <param name="file">The file to read the <see cref="V1Workflow"/> from</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="V1Workflow"/></returns>
        public static async Task<V1Workflow> ReadWorkflowFromFileAsync(this ISynapseService synapse, string file, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException($"Failed to find the specified workflow definition file '{file}'", file);
            using Stream stream = File.OpenRead(file);
            WorkflowDefinitionFormat definitionFormat;
            switch (new FileInfo(file).Extension.ToLower())
            {
                case ".json":
                    definitionFormat = WorkflowDefinitionFormat.Json;
                    break;
                case ".yaml":
                case ".yml":
                    definitionFormat = WorkflowDefinitionFormat.Yaml;
                    break;
                default:
                    Console.WriteLine("Failed to automatically determine the definition format. Assuming YAML format.");
                    definitionFormat = WorkflowDefinitionFormat.Yaml;
                    break;
            }
            return await synapse.ReadWorkflowAsync(stream, definitionFormat);
        }

        /// <summary>
        /// Reads a <see cref="V1Workflow"/> from the specified raw definition
        /// </summary>
        /// <param name="synapse">The extended <see cref="ISynapseService"/></param>
        /// <param name="rawDefinition">The raw definition to read the <see cref="V1Workflow"/> from</param>
        /// <param name="definitionFormat">The format of the raw definition</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="V1Workflow"/></returns>
        public static async  Task<V1Workflow> ReadWorkflowAsync(this ISynapseService synapse, string rawDefinition, WorkflowDefinitionFormat definitionFormat, CancellationToken cancellationToken = default)
        {
            using Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(rawDefinition));
            return await synapse.ReadWorkflowAsync(stream, definitionFormat);
        }

    }

}
