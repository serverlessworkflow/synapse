namespace Synapse.Runtime.Services
{
    /// <summary>
    /// Defines the fundamentals of a service used to process <see cref="ActionDefinition"/> activities
    /// </summary>
    public interface IActionProcessor
        : IWorkflowActivityProcessor
    {

        /// <summary>
        /// Gets the <see cref="ActionDefinition"/> to process
        /// </summary>
        ActionDefinition Action { get; }

    }

}
