using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents the spec of a <see cref="V1Trigger"/>
    /// </summary>
    public class V1TriggerSpec
    {

        /// <summary>
        /// Initializes a new <see cref="V1TriggerSpec"/>
        /// </summary>
        public V1TriggerSpec()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1TriggerSpec"/>
        /// </summary>
        /// <param name="correlationMode">The <see cref="V1Trigger"/>'s correlation mode</param>
        /// <param name="conditionType">The <see cref="V1Trigger"/>'s condition type</param>
        /// <param name="outcome">The <see cref="V1TriggerOutcome"/> used to configure the <see cref="V1Trigger"/>'s outcome</param>
        public V1TriggerSpec(V1TriggerCorrelationMode correlationMode, V1TriggerConditionType conditionType, V1TriggerOutcome outcome)
            : this()
        {
            this.CorrelationMode = correlationMode;
            this.ConditionType = conditionType;
            this.Outcome = outcome ?? throw new ArgumentNullException(nameof(outcome));
        }

        /// <summary>
        /// Initializes a new <see cref="V1TriggerSpec"/>
        /// </summary>
        /// <param name="correlationMode">The <see cref="V1Trigger"/>'s correlation mode</param>
        /// <param name="conditionType">The <see cref="V1Trigger"/>'s condition type</param>
        /// <param name="outcome">The <see cref="V1TriggerOutcome"/> used to configure the <see cref="V1Trigger"/>'s outcome</param>
        /// <param name="conditions">An array containing the <see cref="V1Trigger"/>'s <see cref="V1TriggerCondition"/>s</param>
        public V1TriggerSpec(V1TriggerCorrelationMode correlationMode, V1TriggerConditionType conditionType, V1TriggerOutcome outcome, params V1TriggerCondition[] conditions)
            : this(correlationMode, conditionType, outcome)
        {
            if(conditions == null)
                throw new ArgumentNullException(nameof(conditions));
            this.Conditions = conditions.ToList();
        }

        /// <summary>
        /// Gets the <see cref="V1Trigger"/>'s correlation mode
        /// </summary>
        [JsonProperty("correlationMode")]
        public V1TriggerCorrelationMode CorrelationMode { get; set; }

        /// <summary>
        /// Gets the <see cref="V1Trigger"/>'s condition type
        /// </summary>
        [JsonProperty("conditionType")]
        public V1TriggerConditionType ConditionType { get; set; }

        /// <summary>
        /// Gets an <see cref="IList{T}"/> containing the <see cref="V1Trigger"/>'s conditions
        /// </summary>
        [JsonProperty("conditions")]
        public IList<V1TriggerCondition> Conditions { get; set; } = new List<V1TriggerCondition>();

        /// <summary>
        /// Gets the <see cref="V1TriggerOutcome"/> used to configure the <see cref="V1Trigger"/>'s outcome
        /// </summary>
        [JsonProperty("outcome")]
        public V1TriggerOutcome Outcome { get; set; } = new V1TriggerOutcome();

    }

}
