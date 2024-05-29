namespace Synapse.Api.Client.Services;

/// <summary>
/// Defines the fundamentals of a Synapse API client
/// </summary>
public interface ISynapseApiClient
{

    /// <summary>
    /// Gets the Synapse API used to manage <see cref="Namespace"/>s
    /// </summary>
    IClusterResourceApiClient<Namespace> Namespaces { get; }

    /// <summary>
    /// Gets the Synapse API used to manage <see cref="Operator"/>s
    /// </summary>
    INamespacedResourceApiClient<Operator> Operators { get; }

    /// <summary>
    /// Gets the Synapse API used to manage <see cref="Workflow"/>s
    /// </summary>
    INamespacedResourceApiClient<Workflow> Workflows { get; }

    /// <summary>
    /// Gets the Synapse API used to manage workflow data <see cref="Document"/>s
    /// </summary>
    IDocumentApiClient WorkflowData { get; }

    /// <summary>
    /// Gets the Synapse API used to manage <see cref="WorkflowInstance"/>s
    /// </summary>
    INamespacedResourceApiClient<WorkflowInstance> WorkflowInstances { get; }

}
