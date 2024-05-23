namespace Synapse;

/// <summary>
/// Defines extensions for <see cref="WorkflowDefinition"/>s
/// </summary>
public static class WorkflowDefinitionExtensions
{

    /// <summary>
    /// Gets the next <see cref="TaskDefinition"/> to perform next, if any
    /// </summary>
    /// <param name="workflow">The extended <see cref="WorkflowDefinition"/></param>
    /// <param name="after">The <see cref="TaskInstance"/> to perform the next <see cref="TaskInstance"/> after</param>
    /// <returns>The next <see cref="TaskDefinition"/> to perform next, if any</returns>
    public static KeyValuePair<string, TaskDefinition> GetTaskAfter(this WorkflowDefinition workflow, TaskInstance after)
    {
        ArgumentNullException.ThrowIfNull(after);
        switch (after.Next)
        {
            case FlowDirective.Continue:
                var afterTask = workflow.Do[after.Name!];
                var afterIndex = workflow.Do.Values.ToList().IndexOf(afterTask);
                return workflow.Do.Skip(afterIndex + 1).FirstOrDefault();
            case FlowDirective.End: case FlowDirective.Exit: return default;
            default: return new(after.Next!, workflow.Do[after.Next!]);
        }
    }

    /// <summary>
    /// Attempts to get the next <see cref="TaskDefinition"/> to perform next, if any
    /// </summary>
    /// <param name="workflow">The extended <see cref="WorkflowDefinition"/></param>
    /// <param name="after">The <see cref="TaskInstance"/> to perform the next <see cref="TaskInstance"/> after</param>
    /// <param name="task">The next <see cref="TaskDefinition"/> to perform next, if any</param>
    /// <returns>A boolean indicating whether or not a next <see cref="TaskInstance"/> must be executed next</returns>
    public static bool TryGetTaskAfter(this WorkflowDefinition workflow, TaskInstance after, out KeyValuePair<string, TaskDefinition> task)
    {
        ArgumentNullException.ThrowIfNull(after);
        task = workflow.GetTaskAfter(after);
        return task.Key == default;
    }

}