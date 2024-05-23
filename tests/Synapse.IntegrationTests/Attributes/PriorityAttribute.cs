namespace Synapse.IntegrationTests;

/// <summary>
/// Represents an <see cref="Attribute"/> used to set the priority of a test case
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class PriorityAttribute
    : Attribute
{

    /// <summary>
    /// Initializes a new <see cref="PriorityAttribute"/>
    /// </summary>
    /// <param name="priority">The test case's priority</param>
    public PriorityAttribute(int priority)
    {
        this.Priority = priority;
    }

    /// <summary>
    /// Gets the test case's priority
    /// </summary>
    public int Priority { get; private set; }

}
