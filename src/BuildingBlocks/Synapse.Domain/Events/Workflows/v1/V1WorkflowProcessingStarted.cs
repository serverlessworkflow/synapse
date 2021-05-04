using Synapse.Domain.Models;

namespace Synapse.Domain.Events.Workflows
{

    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever the processing of a <see cref="V1Workflow"/> has started
    /// </summary>
    public class V1WorkflowProcessingStarted
        : DomainEvent<V1Workflow>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowProcessingStarted"/>
        /// </summary>
        protected V1WorkflowProcessingStarted()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowProcessingStarted"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1Workflow"/> being processed</param>
        public V1WorkflowProcessingStarted(string id)
            : base(id)
        {

        }

    }

}
