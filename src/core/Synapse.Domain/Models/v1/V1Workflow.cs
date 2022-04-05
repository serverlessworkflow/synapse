/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using Synapse.Domain.Events.Workflows;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents a workflow
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Models.V1Workflow))]
    public class V1Workflow
        : AggregateRoot<string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1Workflow"/>
        /// </summary>
        protected V1Workflow() 
            : base(null!)
        {
            this.Definition = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1Workflow"/>
        /// </summary>
        /// <param name="definition">The <see cref="V1Workflow"/>'s definition</param>
        public V1Workflow(WorkflowDefinition definition)
            : this()
        {
            if(definition == null)
                throw DomainException.ArgumentNull(nameof(definition));
            this.On(this.RegisterEvent(new V1WorkflowCreatedDomainEvent(definition.GetUniqueIdentifier(), definition)));
        }

        /// <summary>
        /// Gets the <see cref="V1Workflow"/>'s definition
        /// </summary>
        public virtual WorkflowDefinition Definition { get; protected set; }

        /// <summary>
        /// Gets the date and time at which the <see cref="V1Workflow"/> was last instanciated
        /// </summary>
        public virtual DateTimeOffset? LastInstanciated { get; protected set; }

        /// <summary>
        /// Instanciates the <see cref="V1Workflow"/>
        /// </summary>
        public virtual void Instanciate()
        {
            this.On(this.RegisterEvent(new V1WorkflowInstanciatedDomainEvent(this.Id)));
        }

        /// <summary>
        /// Deletes the <see cref="V1Workflow"/>
        /// </summary>
        public virtual void Delete()
        {
            this.On(this.RegisterEvent(new V1WorkflowDeletedDomainEvent(this.Id)));
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowCreatedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowCreatedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowCreatedDomainEvent e)
        {
            this.Id = e.AggregateId;
            this.CreatedAt = e.CreatedAt;
            this.LastModified = e.CreatedAt;
            this.Definition = e.Definition;
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowInstanciatedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowInstanciatedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowInstanciatedDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.LastInstanciated = e.CreatedAt;
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowDeletedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowDeletedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowDeletedDomainEvent e)
        {

        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.Id;
        }

    }

}
