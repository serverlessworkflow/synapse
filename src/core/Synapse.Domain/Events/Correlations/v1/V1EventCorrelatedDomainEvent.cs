using Synapse.Integration.Events.Correlations;

namespace Synapse.Domain.Events.Correlations
{

    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever a new <see cref="CloudEvent"/> has been correlated
    /// </summary>
    [DataTransferObjectType(typeof(V1EventCorrelatedIntegrationEvent))]
    public class V1EventCorrelatedDomainEvent
        : DomainEvent<Models.V1Correlation, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1EventCorrelatedDomainEvent"/>
        /// </summary>
        protected V1EventCorrelatedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1EventCorrelatedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the performed correlation</param>
        /// <param name="contextId">The id of the context in which the correlation has been performed</param>
        /// <param name="e">The <see cref="V1Event"/> that has been correlated</param>
        /// <param name="mappings">An <see cref="ICollection{T}"/> containing the keys of the mappings used to correlate the <see cref="Models.V1Event"/></param>
        public V1EventCorrelatedDomainEvent(string id, string contextId, Models.V1Event e, IEnumerable<string> mappings)
            : base(id)
        {
            this.ContextId = contextId;
            this.Event = e;
            this.Mappings = mappings;
        }

        /// <summary>
        /// Gets the id of the context in which the correlation has been performed
        /// </summary>
        public virtual string ContextId { get; protected set; } = null!;

        /// <summary>
        /// Gets the <see cref="V1Event"/> that has been correlated
        /// </summary>
        public virtual Models.V1Event Event { get; protected set; } = null!;

        /// <summary>
        /// Gets an <see cref="ICollection{T}"/> containing the keys of the mappings used to correlate the <see cref="Models.V1Event"/>
        /// </summary>
        public virtual IEnumerable<string> Mappings { get; protected set; } = null!;

    }

}
