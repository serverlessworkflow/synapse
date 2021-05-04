using Newtonsoft.Json.Linq;
using Synapse.Domain.Models;
using System;

namespace Synapse.Domain.Events.WorkflowInstances
{

    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever a new <see cref="V1WorkflowInstance"/> has been created
    /// </summary>
    public class V1WorkflowInstanceCreatedDomainEvent
        : DomainEvent<V1WorkflowInstance>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceCreatedDomainEvent"/>
        /// </summary>
        protected V1WorkflowInstanceCreatedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceCreatedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the newly created <see cref="V1WorkflowInstance"/></param>
        /// <param name="definitionId">The id of the <see cref="V1WorkflowInstance"/>'s <see cref="WorkflowDefinition"/></param>
        /// <param name="definitionVersion">The version of the <see cref="V1WorkflowInstance"/>'s <see cref="WorkflowDefinition"/></param>
        /// <param name="input">The newly created <see cref="V1WorkflowInstance"/>'s input data</param>
        public V1WorkflowInstanceCreatedDomainEvent(string id, string definitionId, Version definitionVersion, JToken input)
        {
            this.AggregateId = id;
            this.DefinitionId = definitionId;
            this.DefinitionVersion = definitionVersion;
            this.Input = input;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowInstance"/>'s <see cref="WorkflowDefinition"/>
        /// </summary>
        public string DefinitionId { get; protected set; }

        /// <summary>
        /// Gets the version of the <see cref="V1WorkflowInstance"/>'s <see cref="WorkflowDefinition"/>
        /// </summary>
        public Version DefinitionVersion { get; protected set; }

        /// <summary>
        /// Gets the newly created <see cref="V1WorkflowInstance"/>'s input data
        /// </summary>
        public JToken Input { get; protected set; }

    }

}
