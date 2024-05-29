namespace Synapse.Operator.Services;

/// <summary>
/// Defines the fundamentals of the service used to access the current Synapse Operator
/// </summary>
public interface IOperatorController
    : IHostedService
{

    /// <summary>
    /// Gets the service used to monitor the current <see cref="Resources.Operator"/>
    /// </summary>
    IResourceMonitor<Resources.Operator> Operator { get; }

}