namespace Synapse.Integration.Events
{

    /// <summary>
    /// Describes an integration event, which is an event used to exchange information across domain boundaries
    /// </summary>
    public abstract class V1IntegrationEvent
        : IIntegrationEvent, IDataTransferObject
    {

        /// <summary>
        /// Gets the key of the aggregate that has produced the event
        /// </summary>
        public virtual object AggregateId { get; set; }

        /// <summary>
        /// Gets the date and time at which the event has occured
        /// </summary>
        public virtual DateTimeOffset CreatedAt { get; set; }

    }

}
