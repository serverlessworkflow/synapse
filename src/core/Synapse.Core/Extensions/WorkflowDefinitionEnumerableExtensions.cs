using Semver;

namespace Synapse;

/// <summary>
/// Defines extensions for <see cref="IEnumerable{T}"/> instances that contains <see cref="WorkflowDefinition"/>s
/// </summary>
public static class WorkflowDefinitionEnumerableExtensions
{

    /// <summary>
    /// Gets the latest version of the <see cref="WorkflowDefinition"/>
    /// </summary>
    /// <param name="definitions">An <see cref="IEnumerable{T}"/> containing the <see cref="WorkflowDefinition"/>s to get the latest of</param>
    /// <returns>The latest <see cref="WorkflowDefinition"/></returns>
    public static WorkflowDefinition GetLatest(this IEnumerable<WorkflowDefinition> definitions) => definitions.OrderByDescending(wf => SemVersion.Parse(wf.Document.Version, SemVersionStyles.Strict)).First();

    /// <summary>
    /// Gets the specified <see cref="WorkflowDefinition"/> version
    /// </summary>
    /// <param name="definitions">An <see cref="IEnumerable{T}"/> containing the <see cref="WorkflowDefinition"/>s to get the specified version from</param>
    /// <param name="version">The version of the <see cref="WorkflowDefinition"/> to get</param>
    /// <returns>The <see cref="WorkflowDefinition"/> with the specified version, if any</returns>
    public static WorkflowDefinition? Get(this IEnumerable<WorkflowDefinition> definitions, string version) => definitions.FirstOrDefault(wf => wf.Document.Version == version);

}