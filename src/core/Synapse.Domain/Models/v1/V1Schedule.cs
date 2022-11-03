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

using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Synapse.Domain.Events.Schedules;
using System.Text.RegularExpressions;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents a <see cref="V1Workflow"/> schedule
    /// </summary>
    [Patchable]
    [DataTransferObjectType(typeof(Integration.Models.V1Schedule))]
    public class V1Schedule
        : AggregateRoot<string>, IDeletable
    {

        /// <summary>
        /// Initializes a new <see cref="V1Schedule"/>
        /// </summary>
        protected V1Schedule() 
            : base(string.Empty)
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1Schedule"/>
        /// </summary>
        /// <param name="type">The <see cref="V1Schedule"/>'s type</param>
        /// <param name="definition">The <see cref="V1Schedule"/>'s <see cref="ScheduleDefinition"/></param>
        /// <param name="workflow">The <see cref="V1Workflow"/> to schedule</param>
        public V1Schedule(V1ScheduleType type, ScheduleDefinition definition, V1Workflow workflow)
            : base(BuildId(workflow?.Id!))
        {
            if (definition == null) throw DomainException.ArgumentNull(nameof(definition));
            if (workflow == null) throw DomainException.ArgumentNull(nameof(workflow));
            this.On(this.RegisterEvent(new V1ScheduleCreatedDomainEvent(this.Id, type, definition, workflow.Id, this.Definition.GetNextOccurence())));
        }

        /// <summary>
        /// Gets the <see cref="V1Schedule"/>'s type
        /// </summary>
        public virtual V1ScheduleType Type { get; protected set; }

        /// <summary>
        /// Gets the <see cref="V1Schedule"/>'s status
        /// </summary>
        public virtual V1ScheduleStatus Status { get; protected set; }

        /// <summary>
        /// Gets the <see cref="V1Schedule"/>'s definition
        /// </summary>
        public virtual ScheduleDefinition Definition { get; protected set; } = null!;

        /// <summary>
        /// Gets the id of the scheduled <see cref="V1Workflow"/>
        /// </summary>
        public virtual string WorkflowId { get; protected set; } = null!;

        /// <summary>
        /// Gets the date and time the <see cref="V1Schedule"/> has last been suspended at
        /// </summary>
        public virtual DateTimeOffset? SuspendedAt { get; protected set; }

        /// <summary>
        /// Gets the date and time the <see cref="V1Schedule"/> has been retired at
        /// </summary>
        public virtual DateTimeOffset? RetiredAt { get; protected set; }

        /// <summary>
        /// Gets the date and time the <see cref="V1Schedule"/> has been made obsolete at
        /// </summary>
        public virtual DateTimeOffset? ObsoletedAt { get; protected set; }

        /// <summary>
        /// Gets the date and time at which the <see cref="V1Schedule"/> has last occured
        /// </summary>
        public virtual DateTimeOffset? LastOccuredAt { get; protected set; }

        /// <summary>
        /// Gets the date and time at which the <see cref="V1Schedule"/> will next occur
        /// </summary>
        public virtual DateTimeOffset? NextOccurenceAt { get; protected set; }

        /// <summary>
        /// Gets the total number of occurences
        /// </summary>
        public virtual long TotalOccurences { get; protected set; }

        /// <summary>
        /// Sets the <see cref="V1Schedule"/>'s definition
        /// </summary>
        /// <param name="definition">The <see cref="V1Schedule"/>'s definition</param>
        [JsonPatchOperation(OperationType.Replace, nameof(Definition))]
        public virtual void SetDefinition(ScheduleDefinition definition)
        {
            if (definition == null) throw DomainException.ArgumentNull(nameof(definition));
            this.On(this.RegisterEvent(new V1ScheduleDefinitionChangedDomainEvent(this.Id, definition, definition.GetNextOccurence(this.LastOccuredAt))));
        }

        /// <summary>
        /// Triggers a new occurence of the <see cref="V1Schedule"/>
        /// </summary>
        public virtual void Trigger()
        {
            if (this.Status != V1ScheduleStatus.Active) throw DomainException.UnexpectedState(typeof(V1Schedule), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1ScheduleOccuredDomainEvent(this.Id, this.Definition.GetNextOccurence())));
        }

        /// <summary>
        /// Suspends the <see cref="V1Schedule"/>
        /// </summary>
        public virtual void Suspend()
        {
            if (this.Status == V1ScheduleStatus.Active) throw DomainException.UnexpectedState(typeof(V1Schedule), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1ScheduleSuspendedDomainEvent(this.Id)));
        }

        /// <summary>
        /// Resumes the <see cref="V1Schedule"/>
        /// </summary>
        public virtual void Resume()
        {
            if (this.Status == V1ScheduleStatus.Suspended) throw DomainException.UnexpectedState(typeof(V1Schedule), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1ScheduleResumedDomainEvent(this.Id)));
        }

        /// <summary>
        /// Retires the <see cref="V1Schedule"/>
        /// </summary>
        public virtual void Retire()
        {
            if (this.Status >= V1ScheduleStatus.Retired) throw DomainException.UnexpectedState(typeof(V1Schedule), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1ScheduleRetiredDomainEvent(this.Id)));
        }

        /// <summary>
        /// Makes the <see cref="V1Schedule"/> obsolete
        /// </summary>
        public virtual void MakeObsolete()
        {
            if (this.Status >= V1ScheduleStatus.Retired) throw DomainException.UnexpectedState(typeof(V1Schedule), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1ScheduleObsolitedDomainEvent(this.Id)));
        }

        /// <summary>
        /// Deletes the <see cref="V1Schedule"/>
        /// </summary>
        public virtual void Delete()
        {
            this.On(this.RegisterEvent(new V1ScheduleDeletedDomainEvent(this.Id)));
        }

        /// <summary>
        /// Handles the specified <see cref="V1ScheduleDefinitionChangedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1ScheduleDefinitionChangedDomainEvent"/> to handle</param>
        protected virtual void On(V1ScheduleDefinitionChangedDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.Definition = e.Definition;
            this.NextOccurenceAt = e.NextOccurenceAt;
        }

        /// <summary>
        /// Handles the specified <see cref="V1ScheduleCreatedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1ScheduleCreatedDomainEvent"/> to handle</param>
        protected virtual void On(V1ScheduleCreatedDomainEvent e)
        {
            this.Id = e.AggregateId;
            this.CreatedAt = e.CreatedAt;
            this.LastModified = e.CreatedAt;
            this.Type = e.Type;
            this.Status = V1ScheduleStatus.Active;
            this.Definition = e.Definition;
            this.WorkflowId = e.WorkflowId;
            this.NextOccurenceAt = e.NextOccurenceAt;
        }

        /// <summary>
        /// Handles the specified <see cref="V1ScheduleOccuredDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1ScheduleOccuredDomainEvent"/> to handle</param>
        protected virtual void On(V1ScheduleOccuredDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.LastOccuredAt = e.CreatedAt;
            this.NextOccurenceAt = e.NextOccurenceAt;
            this.TotalOccurences++;
        }

        /// <summary>
        /// Handles the specified <see cref="V1ScheduleSuspendedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1ScheduleSuspendedDomainEvent"/> to handle</param>
        protected virtual void On(V1ScheduleSuspendedDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.SuspendedAt = e.CreatedAt;
            this.Status = V1ScheduleStatus.Suspended;
            this.NextOccurenceAt = null;
        }

        /// <summary>
        /// Handles the specified <see cref="V1ScheduleResumedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1ScheduleResumedDomainEvent"/> to handle</param>
        protected virtual void On(V1ScheduleResumedDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.SuspendedAt = null;
            this.Status = V1ScheduleStatus.Active;
        }

        /// <summary>
        /// Handles the specified <see cref="V1ScheduleRetiredDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1ScheduleRetiredDomainEvent"/> to handle</param>
        protected virtual void On(V1ScheduleRetiredDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.RetiredAt = e.CreatedAt;
            this.Status = V1ScheduleStatus.Retired;
            this.NextOccurenceAt = null;
        }

        /// <summary>
        /// Handles the specified <see cref="V1ScheduleObsolitedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1ScheduleObsolitedDomainEvent"/> to handle</param>
        protected virtual void On(V1ScheduleObsolitedDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.ObsoletedAt = e.CreatedAt;
            this.Status = V1ScheduleStatus.Obsolete;
            this.NextOccurenceAt = null;
        }

        /// <summary>
        /// Handles the specified <see cref="V1ScheduleDeletedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1ScheduleDeletedDomainEvent"/> to handle</param>
        protected virtual void On(V1ScheduleDeletedDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.NextOccurenceAt = null;
        }

        /// <summary>
        /// Builds a new <see cref="V1Schedule"/> id
        /// </summary>
        /// <param name="workflowId">The id of the <see cref="V1Workflow"/> to build a new id for</param>
        /// <returns>A new <see cref="V1Schedule"/> id</returns>
        /// <remarks>Note that because it uses a generated <see cref="Guid"/>, this method should never return twice the same output</remarks>
        public static string BuildId(string workflowId)
        {
            if (string.IsNullOrWhiteSpace(workflowId)) throw DomainException.ArgumentNullOrWhitespace(nameof(workflowId));
            return $"{workflowId}-{Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", string.Empty)}";
        }

    }

}
