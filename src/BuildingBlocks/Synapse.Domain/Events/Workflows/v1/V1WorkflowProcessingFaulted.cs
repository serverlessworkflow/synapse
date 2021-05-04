using Newtonsoft.Json;
using Synapse.Domain.Models;
using System.Collections.Generic;

namespace Synapse.Domain.Events.Workflows
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever the processing of a <see cref="V1Workflow"/> has faulted
    /// </summary>
    public class V1WorkflowProcessingFaulted
        : DomainEvent<V1Workflow>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowProcessingStarted"/>
        /// </summary>
        protected V1WorkflowProcessingFaulted()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowProcessingFaulted"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1Workflow"/> which's processing has faulted</param>
        /// <param name="errors">An <see cref="IEnumerable{T}"/> containing the <see cref="V1Error"/>s that have occured during the processing of the <see cref="V1Workflow"/></param>
        public V1WorkflowProcessingFaulted(string id, IEnumerable<V1Error> errors)
            : base(id)
        {
            this.Errors = errors;
        }

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> containing the <see cref="V1Error"/>s that have occured during the processing of the <see cref="V1Workflow"/>
        /// </summary>
        [JsonProperty("errors")]
        public IEnumerable<V1Error> Errors { get; protected set; }

    }

}
