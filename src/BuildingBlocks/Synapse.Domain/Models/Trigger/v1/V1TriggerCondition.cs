using CloudNative.CloudEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents the object used to configure a <see cref="V1Trigger"/> condition
    /// </summary>
    public class V1TriggerCondition
    {

        /// <summary>
        /// Initializes a new <see cref="V1TriggerCondition"/>
        /// </summary>
        public V1TriggerCondition()
        {

        }

        /// <summary>
        /// Gets an <see cref="IList{T}"/> containing the <see cref="V1EventFilter"/> used to configure the filtering of <see cref="CloudEvent"/>s that can fire the <see cref="V1Trigger"/>
        /// </summary>
        [JsonProperty("filters")]
        public IList<V1EventFilter> Filters { get; set; } = new List<V1EventFilter>();

        /// <summary>
        /// Determines whether or not the <see cref="V1TriggerCondition"/> matches the specified <see cref="CloudEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="CloudEvent"/> to match</param>
        /// <returns>A boolean indicating whether or not the <see cref="V1TriggerCondition"/> matches the specified <see cref="CloudEvent"/></returns>
        public virtual bool Matches(CloudEvent e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            return this.GetMatchingFilterFor(e) != null;
        }

        /// <summary>
        /// Determines whether or not the <see cref="V1TriggerCondition"/> matches the specified <see cref="V1CorrelationContext"/>
        /// </summary>
        /// <param name="context">The <see cref="V1CorrelationContext"/> to match</param>
        /// <returns>A boolean indicating whether or not the <see cref="V1TriggerCondition"/> matches the specified <see cref="V1CorrelationContext"/></returns>
        public virtual bool Matches(V1CorrelationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            return context.BootstrapEvents.Any(e => this.Filters.Any(f => f.Filters(e)));
        }

        /// <summary>
        /// Gets the matching <see cref="V1EventFilter"/> for the specified <see cref="CloudEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="CloudEvent"/> to get the matching <see cref="V1EventFilter"/> for</param>
        /// <returns>The matching <see cref="V1EventFilter"/> for the specified <see cref="CloudEvent"/>, if any</returns>
        public virtual V1EventFilter GetMatchingFilterFor(CloudEvent e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            return this.Filters.FirstOrDefault(f => f.Filters(e));
        }

    }

}
