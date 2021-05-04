using ServerlessWorkflow.Sdk.Models;
using Synapse.Domain.Models;
using Synapse.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Synapse
{

    /// <summary>
    /// Defines extensions for <see cref="V1Workflow"/>s
    /// </summary>
    public static class V1WorkflowRepositoryExtensions
    {

        /// <summary>
        /// Gets the latest version of the specified <see cref="WorkflowDefinition"/>
        /// </summary>
        /// <param name="workflows">The extended <see cref="IRepository{TResource}"/></param>
        /// <param name="id">The id of the <see cref="WorkflowDefinition"/> to get the latest version of</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The latest version of the specified <see cref="WorkflowDefinition"/></returns>
        public static async Task<string> GetLatestVersionOfAsync(this IRepository<V1Workflow> workflows, string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            string labelSelector = $"{SynapseConstants.Labels.Workflows.Id}={id}";
            return (await workflows.FilterAsync(labelSelector, cancellationToken: cancellationToken))
                .Select(w => w.Spec.Definition.Version)
                .OrderByDescending(v => v)
                .First();
        }

        /// <summary>
        /// Finds the <see cref="V1Workflow"/> with the specified <see cref="WorkflowDefinition"/>
        /// </summary>
        /// <param name="workflows">The extended <see cref="IRepository{TResource}"/></param>
        /// <param name="id">The id of the <see cref="WorkflowDefinition"/> to get the <see cref="V1Workflow"/> for</param>
        /// <param name="version">The version of the <see cref="WorkflowDefinition"/> to get the <see cref="V1Workflow"/> for</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The <see cref="V1Workflow"/> with the specified <see cref="WorkflowDefinition"/></returns>
        public static async Task<V1Workflow> FindAsync(this IRepository<V1Workflow> workflows, string id, string version, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrWhiteSpace(version))
                throw new ArgumentNullException(nameof(version));
            if (version == "latest")
                version = await workflows.GetLatestVersionOfAsync(id, cancellationToken);
            string labelSelector = $"{SynapseConstants.Labels.Workflows.Id}={id},{SynapseConstants.Labels.Workflows.Version}={version}";
            return (await workflows.FilterAsync(labelSelector, cancellationToken: cancellationToken)).FirstOrDefault();
        }

    }

}
