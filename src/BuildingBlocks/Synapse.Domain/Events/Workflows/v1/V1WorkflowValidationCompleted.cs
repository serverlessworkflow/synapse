using Synapse.Domain.Models;
using System.Collections.Generic;

namespace Synapse.Domain.Events.Workflows
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever the validation of a <see cref="V1Workflow"/> has been completed
    /// </summary>
    public class V1WorkflowValidationCompleted
        : DomainEvent<V1Workflow>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowValidationCompleted"/>
        /// </summary>
        protected V1WorkflowValidationCompleted()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowValidationCompleted"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1Workflow"/> which's validation has been completed</param>
        /// <param name="isValid">A boolean indicating whether or not the <see cref="V1Workflow"/> is valid</param>
        /// <param name="validationErrors">An <see cref="IEnumerable{T}"/> containing the <see cref="V1Workflow"/> validation <see cref="V1Error"/>s</param>
        public V1WorkflowValidationCompleted(string id, bool isValid, IEnumerable<V1Error> validationErrors)
            : base(id)
        {
            this.IsValid = isValid;
            this.ValidationErrors = validationErrors;
        }

        /// <summary>
        /// Gets a boolean indicating whether or not the <see cref="V1Workflow"/> is valid
        /// </summary>
        public bool IsValid { get; protected set; }

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> containing the <see cref="V1Workflow"/> validation <see cref="V1Error"/>s
        /// </summary>
        public IEnumerable<V1Error> ValidationErrors { get; protected set; }

    }

}
