using Newtonsoft.Json.Linq;
using ServerlessWorkflow.Sdk;
using Synapse.Domain.Models;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Synapse.Cli.Services
{

    /// <summary>
    /// Defines the fundamentals of a service used to interact with Synapse
    /// </summary>
    public interface ISynapseService
    {

        /// <summary>
        /// Installs Synapse
        /// </summary>
        /// <param name="namespace">The namespace to install Synapse into. Defaults to 'synapse'</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task InstallAsync(string @namespace = "synapse", CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds the specified <see cref="V1Workflow"/>
        /// </summary>
        /// <param name="reference">The <see cref="V1Workflow"/>'s reference</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The matching <see cref="V1Workflow"/></returns>
        Task<V1Workflow> FindWorkflowByReferenceAsync(string reference, CancellationToken cancellationToken = default);

        /// <summary>
        /// Reads a <see cref="V1Workflow"/> from the specified <see cref="Stream"/>
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to load the <see cref="V1Workflow"/> from</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="V1Workflow"/></returns>
        Task<V1Workflow> ReadWorkflowAsync(Stream stream, WorkflowDefinitionFormat definitionFormat = WorkflowDefinitionFormat.Yaml, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deploys the specified <see cref="V1Workflow"/>
        /// </summary>
        /// <param name="workflow">The <see cref="V1Workflow"/> to deploy</param>
        /// <param name="wait">Wait for the <see cref="V1Workflow"/> to be successfully deployed. Default to false</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The newly deployed <see cref="V1Workflow"/></returns>
        Task<V1Workflow> DeployWorkflowAsync(V1Workflow workflow, bool wait = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Ryns the specified <see cref="V1Workflow"/>
        /// </summary>
        /// <param name="workflow">The <see cref="V1Workflow"/> to run</param>
        /// <param name="input">The <see cref="V1Workflow"/>'s input data</param>
        /// <param name="wait">A boolean indicating whether or not to wait for the <see cref="V1WorkflowInstance"/>'s execution</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The newly created <see cref="V1WorkflowInstance"/></returns>
        Task<V1WorkflowInstance> RunWorkflowAsync(V1Workflow workflow, JObject input = null, bool wait = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the specified <see cref="V1Workflow"/>
        /// </summary>
        /// <param name="name">The name of the <see cref="V1Workflow"/> to delete</param>
        /// <param name="namespace">The namespace the <see cref="V1Workflow"/> to delete belongs to</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task DeleteWorkflowAsync(string name, string @namespace, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes all deployed <see cref="V1Workflow"/>s
        /// </summary>
        /// <param name="namespace">The namespace the <see cref="V1Workflow"/>s to delete belong to</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task DeleteAllWorkflowsAsync(string @namespace, CancellationToken cancellationToken = default);

    }

}
