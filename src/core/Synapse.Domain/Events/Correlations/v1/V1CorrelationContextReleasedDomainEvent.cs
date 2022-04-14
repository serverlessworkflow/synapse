using Synapse.Integration.Events.Correlations;

namespace Synapse.Domain.Events.Correlations
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever a <see cref="Models.V1CorrelationContext"/> has been released by a <see cref="Models.V1Correlation"/>
    /// </summary>
    [DataTransferObjectType(typeof(V1CorrelationContextReleasedIntegrationEvent))]
    public class V1CorrelationContextReleasedDomainEvent
        : DomainEvent<Models.V1Correlation, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1CorrelationContextReleasedDomainEvent"/>
        /// </summary>
        protected V1CorrelationContextReleasedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1CorrelationContextReleasedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the performed correlation</param>
        /// <param name="contextId">The id of the context that has been released</param>
        public V1CorrelationContextReleasedDomainEvent(string id, string contextId)
            : base(id)
        {
            this.ContextId = contextId;
        }

        /// <summary>
        /// Gets the id of the context that has been released
        /// </summary>
        public virtual string ContextId { get; protected set; } = null!;

    }

}
