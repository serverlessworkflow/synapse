﻿/*
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
    [DataTransferObjectType(typeof(V1WorkflowDto))]
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

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.Id;
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

    }

}