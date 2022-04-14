namespace Synapse.Worker.Services
{
    /// <summary>
    /// Defines the fundamentals of a service used to process <see cref="EndDefinition"/> activities
    /// </summary>
    public interface IEndProcessor
        : IWorkflowActivityProcessor
    {

        /// <summary>
        /// Gets the <see cref="EndDefinition"/> to process
        /// </summary>
        EndDefinition? End { get; }

    }

}
