using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Runner.Application.Services
{
    /// <summary>
    /// Defines the fundamentals of a service used to process <see cref="TransitionDefinition"/> activities
    /// </summary>
    public interface ITransitionProcessor
        : IWorkflowActivityProcessor
    {

        /// <summary>
        /// Gets the <see cref="TransitionDefinition"/> to process
        /// </summary>
        TransitionDefinition Transition { get; }

    }

}
