using Newtonsoft.Json;
using System.Collections.Generic;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents the status of a <see cref="V1Trigger"/>
    /// </summary>
    public class V1TriggerStatus
    {

        /// <summary>
        /// Initializes a new <see cref="V1TriggerStatus"/>
        /// </summary>
        public V1TriggerStatus()
        {

        }

        /// <summary>
        /// Gets an <see cref="IList{T}"/> containing the <see cref="V1Trigger"/>'s <see cref="V1CorrelationContext"/>s
        /// </summary>
        [JsonProperty("correlationContexts")]
        public IList<V1CorrelationContext> CorrelationContexts { get; set; } = new List<V1CorrelationContext>();

    }

}
