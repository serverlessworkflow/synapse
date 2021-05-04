using Newtonsoft.Json;
using System.Collections.Generic;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents the status of a <see cref="V1Workflow"/>
    /// </summary>
    public class V1WorkflowStatus
    {

        /// <summary>
        /// Gets the <see cref="V1Workflow"/>'s status type
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public V1WorkflowDefinitionStatus Type { get; set; }

        /// <summary>
        /// Gets a <see cref="List{T}"/> containing all the <see cref="V1Error"/>s that have occured during the processing of the <see cref="V1Workflow"/>
        /// </summary>
        [JsonProperty(PropertyName = "errors")]
        public IList<V1Error> Errors { get; set; } = new List<V1Error>();

    }

}
