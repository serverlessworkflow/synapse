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

using Synapse.Domain.Events.WorkflowActivities;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Describes a workflow activity
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Models.V1WorkflowActivity))]
    public class V1WorkflowActivity
        : AggregateRoot<string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowActivity"/>
        /// </summary>
        protected V1WorkflowActivity()
            : base(null!)
        {
            this.WorkflowInstanceId = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <param name="workflowInstance">The <see cref="V1WorkflowInstance"/> the <see cref="V1WorkflowActivity"/> belongs to</param>
        /// <param name="type">The <see cref="V1WorkflowActivity"/>'s type</param>
        /// <param name="input">The <see cref="V1WorkflowActivity"/>'s data</param>
        /// <param name="metadata">The <see cref="V1WorkflowActivity"/>'s metadata</param>
        /// <param name="parent">The <see cref="V1WorkflowActivity"/>'s parent, if any</param>
        public V1WorkflowActivity(V1WorkflowInstance workflowInstance, V1WorkflowActivityType type, object? input = null, IDictionary<string, string>? metadata = null, V1WorkflowActivity? parent = null)
            : this()
        {
            if (workflowInstance == null)
                throw DomainException.ArgumentNull(nameof(workflowInstance));
            this.On(this.RegisterEvent(new V1WorkflowActivityCreatedDomainEvent(Guid.NewGuid().ToString(), workflowInstance.Id, type, input, metadata, parent?.Id)));
        }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowInstance"/> the <see cref="V1WorkflowActivity"/> belongs to
        /// </summary>
        public virtual string WorkflowInstanceId { get; protected set; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowActivity"/>'s type
        /// </summary>
        public virtual V1WorkflowActivityType Type { get; protected set; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowActivity"/>'s status
        /// </summary>
        public virtual V1WorkflowActivityStatus Status { get; protected set; }

        /// <summary>
        /// Gets the date and time at which the <see cref="V1WorkflowActivity"/> has started
        /// </summary>
        public virtual DateTimeOffset? StartedAt { get; protected set; }

        /// <summary>
        /// Gets the date and time at which the <see cref="V1WorkflowActivity"/> has been executed<para></para>
        /// Value is set when the <see cref="V1WorkflowActivity"/> has been cancelled, faults or completes
        /// </summary>
        public virtual DateTimeOffset? ExecutedAt { get; protected set; }

        /// <summary>
        /// Gets the <see cref="Neuroglia.Error"/> that caused the <see cref="V1WorkflowActivity"/> to end prematurily
        /// </summary>
        public virtual Neuroglia.Error? Error { get; protected set; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowActivity"/>'s input
        /// </summary>
        public virtual object? Input { get; protected set; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowInstance"/>'s output
        /// </summary>
        public virtual object? Output { get; protected set; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowActivity"/>'s metadata
        /// </summary>
        public virtual IDictionary<string, string>? Metadata { get; protected set; }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowActivity"/>'s parent, if any
        /// </summary>
        public virtual string? ParentId { get; protected set; }

        /// <summary>
        /// Starts or resumes the <see cref="V1WorkflowActivity"/>
        /// </summary>
        public virtual void StartOrResume()
        {
            if (this.Status != V1WorkflowActivityStatus.Pending)
                throw DomainException.UnexpectedState(typeof(V1WorkflowActivity), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1WorkflowActivityStartedDomainEvent(this.Id)));
        }

        /// <summary>
        /// Supsend the <see cref="V1WorkflowActivity"/>
        /// </summary>
        public virtual void Suspend()
        {
            if (this.Status != V1WorkflowActivityStatus.Running)
                throw DomainException.UnexpectedState(typeof(V1WorkflowActivity), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1WorkflowActivitySuspendedDomainEvent(this.Id)));
        }

        /// <summary>
        /// Faults the <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <param name="ex">The unhandled <see cref="Exception"/> that caused the <see cref="V1WorkflowActivity"/> to end prematurily</param>
        public virtual void Fault(Exception ex)
        {
            if (ex == null)
                throw DomainException.ArgumentNull(nameof(ex));
            if (this.Status >= V1WorkflowActivityStatus.Faulted)
                throw DomainException.UnexpectedState(typeof(V1WorkflowActivity), this.Id, this.Status);
            this.Fault(new Neuroglia.Error(ex.GetType().Name.Replace("exception", string.Empty, StringComparison.OrdinalIgnoreCase), ex.Message));
        }

        /// <summary>
        /// Faults the <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <param name="error">The unhandled <see cref="Error"/> that caused the <see cref="V1WorkflowActivity"/> to end prematurily</param>
        public virtual void Fault(Neuroglia.Error error)
        {
            if (error == null)
                throw DomainException.ArgumentNull(nameof(error));
            if (this.Status >= V1WorkflowActivityStatus.Faulted)
                throw DomainException.UnexpectedState(typeof(V1WorkflowActivity), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1WorkflowActivityFaultedDomainEvent(this.Id, error)));
        }

        /// <summary>
        /// Compensates the <see cref="V1WorkflowInstance"/>
        /// </summary>
        public virtual void Compensate()
        {
            if (this.Status > V1WorkflowActivityStatus.Faulted)
                throw DomainException.UnexpectedState(typeof(V1WorkflowActivity), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1WorkflowActivityCompensatingDomainEvent(this.Id)));
        }

        /// <summary>
        /// Marks the <see cref="V1WorkflowInstance"/> as compensated
        /// </summary>
        public virtual void MarkAsCompensated()
        {
            if (this.Status != V1WorkflowActivityStatus.Compensating)
                throw DomainException.UnexpectedState(typeof(V1WorkflowActivity), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1WorkflowActivityCompensatedDomainEvent(this.Id)));
        }

        /// <summary>
        /// Cancels the <see cref="V1WorkflowActivity"/>
        /// </summary>
        public virtual void Cancel()
        {
            if (this.Status >= V1WorkflowActivityStatus.Faulted)
                throw DomainException.UnexpectedState(typeof(V1WorkflowActivity), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1WorkflowActivityCancelledDomainEvent(this.Id)));
        }

        /// <summary>
        /// Skips the <see cref="V1WorkflowActivity"/>
        /// </summary>
        public virtual void Skip()
        {
            if (this.Status >= V1WorkflowActivityStatus.Faulted)
                throw DomainException.UnexpectedState(typeof(V1WorkflowActivity), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1WorkflowActivitySkippedDomainEvent(this.Id)));
        }

        /// <summary>
        /// Sets the <see cref="V1WorkflowActivity"/>'s metadata
        /// </summary>
        /// <param name="metadata">An <see cref="IDictionary{TKey, TValue}"/> containing the metadata to set</param>
        public virtual void SetMetadata(IDictionary<string, string>? metadata)
        {
            this.On(this.RegisterEvent(new V1WorkflowActivityMetadataChangedDomainEvent(this.Id, metadata)));
        }

        /// <summary>
        /// Completes and sets the <see cref="V1WorkflowActivity"/>'s output
        /// </summary>
        public virtual void SetOutput(object? output)
        {
            if (this.Status >= V1WorkflowActivityStatus.Faulted)
                throw DomainException.UnexpectedState(typeof(V1WorkflowActivity), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1WorkflowActivityCompletedDomainEvent(this.Id, output)));
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowActivityCreatedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowActivityCreatedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowActivityCreatedDomainEvent e)
        {
            this.Id = e.AggregateId;
            this.CreatedAt = e.CreatedAt;
            this.LastModified = e.CreatedAt;
            this.WorkflowInstanceId = e.WorkflowInstanceId;
            this.Type = e.Type;
            this.Input = e.Input;
            this.Metadata = e.Metadata;
            this.ParentId = e.ParentId;
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowActivityStartedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowActivityStartedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowActivityStartedDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.StartedAt = e.CreatedAt;
            this.Status = V1WorkflowActivityStatus.Running;
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowActivitySuspendedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowActivitySuspendedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowActivitySuspendedDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.Status = V1WorkflowActivityStatus.Suspended; //todo: keep track of runtime sessions
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowActivityResumedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowActivityResumedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowActivityResumedDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.Status = V1WorkflowActivityStatus.Running; //todo: keep track of runtime sessions
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowActivityFaultedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowActivityFaultedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowActivityFaultedDomainEvent e)
        {
            this.Status = V1WorkflowActivityStatus.Faulted;
            this.Error = e.Error;
            this.On(this.RegisterEvent(new V1WorkflowActivityExecutedDomainEvent(this.Id, this.Status, this.Error)));
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowActivityCompensatingDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowActivityCompensatingDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowActivityCompensatingDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.Status = V1WorkflowActivityStatus.Compensating;
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowActivityCompensatedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowActivityCompensatedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowActivityCompensatedDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.Status = V1WorkflowActivityStatus.Compensated;
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowActivityCancelledDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowActivityCancelledDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowActivityCancelledDomainEvent e)
        {
            this.Status = V1WorkflowActivityStatus.Cancelled;
            this.On(this.RegisterEvent(new V1WorkflowActivityExecutedDomainEvent(this.Id, this.Status)));
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowActivitySkippedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowActivitySkippedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowActivitySkippedDomainEvent e)
        {
            this.Status = V1WorkflowActivityStatus.Skipped;
            this.On(this.RegisterEvent(new V1WorkflowActivityExecutedDomainEvent(this.Id, this.Status)));
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowActivityMetadataChangedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowActivityMetadataChangedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowActivityMetadataChangedDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.Metadata = e.Metadata;
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowActivityCompletedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowActivityCompletedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowActivityCompletedDomainEvent e)
        {
            this.Status = V1WorkflowActivityStatus.Completed;
            this.Output = e.Output;
            this.On(this.RegisterEvent(new V1WorkflowActivityExecutedDomainEvent(this.Id, this.Status, this.Output)));
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowActivityExecutedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowActivityExecutedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowActivityExecutedDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.ExecutedAt = e.CreatedAt;
        }

    }

}
