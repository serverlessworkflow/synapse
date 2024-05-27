using System.Threading;

namespace Synapse;

/// <summary>
/// Defines extensions for <see cref="CompositeTaskDefinition"/>s
/// </summary>
public static class CompositeTaskDefinitionExtensions
{

    /// <summary>
    /// Gets the next <see cref="TaskDefinition"/> to perform next, if any
    /// </summary>
    /// <param name="compositeTask">The extended <see cref="CompositeTaskDefinition"/></param>
    /// <param name="after">The <see cref="TaskInstance"/> to perform the next <see cref="TaskInstance"/> after</param>
    /// <returns>The next <see cref="TaskDefinition"/> to perform next, if any</returns>
    public static KeyValuePair<string, TaskDefinition> GetTaskAfter(this CompositeTaskDefinition compositeTask, TaskInstance after)
    {
        ArgumentNullException.ThrowIfNull(after);
        if (compositeTask.Execute.Sequentially == null) throw new ArgumentException($"The specified composite task is configured to run task concurrently and cannot define sequences of tasks", nameof(compositeTask));
        switch (after.Next)
        {
            case FlowDirective.Continue:
                var afterTask = compositeTask.Execute.Sequentially[after.Name!];
                var afterIndex = compositeTask.Execute.Sequentially.Values.ToList().IndexOf(afterTask);
                return compositeTask.Execute.Sequentially.Skip(afterIndex + 1).FirstOrDefault();
            case FlowDirective.End: case FlowDirective.Exit: return default;
            default: return new(after.Next!, compositeTask.Execute.Sequentially[after.Next!]);
        }
    }

    /// <summary>
    /// Attempts to get the next <see cref="TaskDefinition"/> to perform next, if any
    /// </summary>
    /// <param name="compositeTask">The extended <see cref="CompositeTaskDefinition"/></param>
    /// <param name="after">The <see cref="TaskInstance"/> to perform the next <see cref="TaskInstance"/> after</param>
    /// <param name="task">The next <see cref="TaskDefinition"/> to perform next, if any</param>
    /// <returns>A boolean indicating whether or not a next <see cref="TaskInstance"/> must be executed next</returns>
    public static bool TryGetTaskAfter(this CompositeTaskDefinition compositeTask, TaskInstance after, out KeyValuePair<string, TaskDefinition> task)
    {
        ArgumentNullException.ThrowIfNull(after);
        task = compositeTask.GetTaskAfter(after);
        return task.Key == default;
    }

}
