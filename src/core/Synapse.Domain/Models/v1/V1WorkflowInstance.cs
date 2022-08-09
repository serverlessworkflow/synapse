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

using Synapse.Domain.Events.WorkflowInstances;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represent an instance of a <see cref="V1Workflow"/>
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Models.V1WorkflowInstance))]
    public class V1WorkflowInstance
        : AggregateRoot<string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstance"/>
        /// </summary>
        protected V1WorkflowInstance()
            : base(null!)
        {
            this.WorkflowId = null!;
            this.Key = null!;
            this.CorrelationContext = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstance"/>
        /// </summary>
        /// <param name="key">The <see cref="V1WorkflowInstance"/>'s key</param>
        /// <param name="workflow">The <see cref="V1WorkflowInstance"/>'s <see cref="V1Workflow"/></param>
        /// <param name="activationType">The <see cref="V1WorkflowInstance"/>'s activation type</param>
        /// <param name="input">The <see cref="V1WorkflowInstance"/>'s input data</param>
        /// <param name="correlationContext">An <see cref="IEnumerable{T}"/> containing the <see cref="CloudEvent"/>s that have triggered the creation of the <see cref="V1WorkflowInstance"/></param>
        /// <param name="parent">The <see cref="V1WorkflowInstance"/>'s parent, if any</param>
        public V1WorkflowInstance(string key, V1Workflow workflow, V1WorkflowInstanceActivationType activationType, object? input = null, V1CorrelationContext? correlationContext = null, V1WorkflowInstance? parent = null)
            : this()
        {
            if(workflow == null)
                throw DomainException.ArgumentNull(nameof(workflow));
            if (input == null)
                input = new();
            if (correlationContext == null)
                correlationContext = new();
            this.On(this.RegisterEvent(new V1WorkflowInstanceCreatedDomainEvent(BuildUniqueIdentifier(key, workflow), workflow.Id, key.ToLowerInvariant(), activationType, input, correlationContext, parent?.Id)));
        }

        /// <summary>
        /// Gets the id of the instanciated <see cref="V1Workflow"/>.<para></para>
        /// The workflow id is used as the first out of the two components of the <see cref="V1WorkflowInstance"/>'s id
        /// </summary>
        public virtual string WorkflowId { get; protected set; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowInstance"/>'s key.<para></para>
        /// The key is used as the second out of the two components of the <see cref="V1WorkflowInstance"/>'s id
        /// </summary>
        public virtual string Key { get; protected set; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowInstance"/>'s activation type
        /// </summary>
        public virtual V1WorkflowInstanceActivationType ActivationType { get; protected set; }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowInstance"/>'s parent, if any
        /// </summary>
        public virtual string? ParentId { get; protected set; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowInstance"/>'s input
        /// </summary>
        public virtual object? Input { get; protected set; }

        private List<V1Event> _TriggerEvents = null!;
        /// <summary>
        /// Gets an <see cref="IReadOnlyCollection{T}"/> containing descriptors of the <see cref="CloudEvent"/>s that have triggered the <see cref="V1WorkflowInstance"/>
        /// </summary>
        public virtual IReadOnlyCollection<V1Event> TriggerEvents
        {
            get
            {
                return this._TriggerEvents?.AsReadOnly()!;
            }
        }

        /// <summary>
        /// Gets the <see cref="V1WorkflowInstance"/>'s status
        /// </summary>
        public virtual V1WorkflowInstanceStatus Status { get; protected set; }

        /// <summary>
        /// Gets the date and time at which the <see cref="V1WorkflowInstance"/> has started
        /// </summary>
        public virtual DateTimeOffset? StartedAt { get; protected set; }

        /// <summary>
        /// Gets the date and time at which the <see cref="V1WorkflowInstance"/> has been executed<para></para>
        /// The value is set when the <see cref="V1WorkflowInstance"/> has been cancelled, faults or completes.
        /// </summary>
        public virtual DateTimeOffset? ExecutedAt { get; protected set; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowInstance"/>'s <see cref="V1CorrelationContext"/>
        /// </summary>
        public virtual V1CorrelationContext CorrelationContext { get; protected set; }

        private readonly List<V1WorkflowRuntimeSession> _Sessions = new();
        /// <summary>
        /// Gets an <see cref="IReadOnlyCollection{T}"/> containing the sessions the <see cref="V1WorkflowInstance"/> is made out of
        /// </summary>
        public virtual IReadOnlyCollection<V1WorkflowRuntimeSession> Sessions
        {
            get
            {
                return this._Sessions.AsReadOnly();
            }
        } 

        /// <summary>
        /// Gets the currently active <see cref="V1WorkflowRuntimeSession"/>, if any
        /// </summary>
        public virtual V1WorkflowRuntimeSession? ActiveSession => this.Sessions.FirstOrDefault(s => s.IsActive);

        private readonly List<V1WorkflowActivity> _Activities = new();
        /// <summary>
        /// Gets an <see cref="IReadOnlyCollection{T}"/> containing the activities the <see cref="V1WorkflowInstance"/> is made out of
        /// </summary>
        public virtual IReadOnlyCollection<V1WorkflowActivity> Activities
        {
            get
            {
                return this._Activities.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the <see cref="Neuroglia.Error"/> that caused the <see cref="V1WorkflowInstance"/> to end prematurily
        /// </summary>
        public virtual Neuroglia.Error? Error { get; protected set; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowInstance"/>'s output
        /// </summary>
        public virtual object? Output { get; protected set; }

        /// <summary>
        /// Schedules the <see cref="V1WorkflowInstance"/>'s execution
        /// </summary>
        public virtual void Scheduling()
        {
            if (this.Status != V1WorkflowInstanceStatus.Pending
                && this.Status != V1WorkflowInstanceStatus.Suspended)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1WorkflowInstanceSchedulingDomainEvent(this.Id)));
        }

        /// <summary>
        /// Schedules the <see cref="V1WorkflowInstance"/>'s execution
        /// </summary>
        public virtual void MarkAsScheduled()
        {
            if (this.Status != V1WorkflowInstanceStatus.Pending
                 && this.Status != V1WorkflowInstanceStatus.Suspended)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1WorkflowInstanceScheduledDomainEvent(this.Id)));
        }

        /// <summary>
        /// Starts the <see cref="V1WorkflowInstance"/>'s execution
        /// </summary>
        /// <param name="processId">A string used to uniquely identify the <see cref="V1WorkflowInstance"/>'s process</param>
        public virtual void Start(string processId)
        {
            if(string.IsNullOrEmpty(processId))
                throw DomainException.ArgumentNull(nameof(processId));
            if (this.Status > V1WorkflowInstanceStatus.Scheduled)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1WorkflowInstanceStartingDomainEvent(this.Id, processId)));
        }

        /// <summary>
        /// Marks the <see cref="V1WorkflowInstance"/> as running
        /// </summary>
        public virtual void MarkAsRunning()
        {
            if (this.Status != V1WorkflowInstanceStatus.Starting
                && this.Status != V1WorkflowInstanceStatus.Resuming)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1WorkflowInstanceStartedDomainEvent(this.Id)));
        }

        /// <summary>
        /// Suspsends the <see cref="V1WorkflowInstance"/>'s execution
        /// </summary>
        public virtual void Suspend()
        {
            if (this.Status != V1WorkflowInstanceStatus.Running)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1WorkflowInstanceSuspendingDomainEvent(this.Id)));
        }

        /// <summary>
        /// Marks the <see cref="V1WorkflowInstance"/> as suspended
        /// </summary>
        public virtual void MarkAsSuspended()
        {
            if (this.Status != V1WorkflowInstanceStatus.Running
                && this.Status != V1WorkflowInstanceStatus.Suspending)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1WorkflowInstanceSuspendedDomainEvent(this.Id)));
        }

        /// <summary>
        /// Resumes the <see cref="V1WorkflowInstance"/>'s execution
        /// </summary>
        /// <param name="processId">A string used to uniquely identify the <see cref="V1WorkflowInstance"/>'s process</param>
        public virtual void Resume(string processId)
        {
            if (string.IsNullOrEmpty(processId))
                throw DomainException.ArgumentNull(nameof(processId));
            if (this.Status != V1WorkflowInstanceStatus.Suspended)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1WorkflowInstanceResumingDomainEvent(this.Id, processId)));
        }

        /// <summary>
        /// Marks the <see cref="V1WorkflowInstance"/> as resumed
        /// </summary>
        public virtual void MarkAsResumed()
        {
            if (this.Status != V1WorkflowInstanceStatus.Suspended)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1WorkflowInstanceResumedDomainEvent(this.Id)));
        }

        /// <summary>
        /// Sets the <see cref="V1Workflow"/>'s <see cref="V1CorrelationContext"/>
        /// </summary>
        /// <param name="correlationContext">The <see cref="V1CorrelationContext"/> to set</param>
        public virtual void SetCorrelationContext(V1CorrelationContext correlationContext)
        {
            if (correlationContext == null)
                throw DomainException.ArgumentNull(nameof(correlationContext));
            if (this.Status >= V1WorkflowInstanceStatus.Faulted)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1WorkflowCorrelationContextChangedDomainEvent(this.Id, correlationContext)));
        }

        /// <summary>
        /// Sets the specified correlation mapping
        /// </summary>
        /// <param name="key">The key of the mapping to set</param>
        /// <param name="value">The value of the mapping to set</param>
        public virtual void SetCorrelationMapping(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw DomainException.ArgumentNull(nameof(key));
            if (string.IsNullOrWhiteSpace(value))
                throw DomainException.ArgumentNull(nameof(value));
            if (this.Status >= V1WorkflowInstanceStatus.Faulted)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1WorkflowCorrelationMappingSetDomainEvent(this.Id, key, value)));
        }

        /// <summary>
        /// Faults the <see cref="V1WorkflowInstance"/>
        /// </summary>
        /// <param name="error">The <see cref="Error"/> that has caused the <see cref="V1WorkflowInstance"/> to fault</param>
        public virtual void Fault(Neuroglia.Error error)
        {
            if (error == null)
                throw new ArgumentNullException(nameof(error));
            if (this.Status >= V1WorkflowInstanceStatus.Faulted)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1WorkflowInstanceFaultedDomainEvent(this.Id, error)));
        }

        /// <summary>
        /// Cancels the <see cref="V1WorkflowInstance"/>'s execution
        /// </summary>
        public virtual void Cancel()
        {
            if (this.Status >= V1WorkflowInstanceStatus.Faulted)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1WorkflowInstanceCancellingDomainEvent(this.Id)));
        }

        /// <summary>
        /// Marks the <see cref="V1WorkflowInstance"/> as cancelled
        /// </summary>
        public virtual void MarkAsCancelled()
        {
            if (this.Status != V1WorkflowInstanceStatus.Cancelling)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1WorkflowInstanceCancelledDomainEvent(this.Id)));
        }

        /// <summary>
        /// Completes and sets the <see cref="V1WorkflowInstance"/>'s output 
        /// </summary>
        /// <param name="output">The <see cref="V1WorkflowInstance"/>'s output</param>
        public virtual void MarkAsCompleted(object? output)
        {
            if (this.Status >= V1WorkflowInstanceStatus.Faulted)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1WorkflowInstanceCompletedDomainEvent(this.Id, output)));
        }

        /// <summary>
        /// Deletes the <see cref="V1WorkflowInstance"/>
        /// </summary>
        public virtual void Delete()
        {
            this.On(this.RegisterEvent(new V1WorkflowInstanceDeletedDomainEvent(this.Id)));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.Id;
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowInstanceCreatedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowInstanceCreatedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowInstanceCreatedDomainEvent e)
        {
            this.Id = e.AggregateId;
            this.CreatedAt = e.CreatedAt;
            this.LastModified = e.CreatedAt;
            this.WorkflowId = e.WorkflowId;
            this.Key = e.Key;
            this.ActivationType = e.ActivationType;
            this.Input = e.Input;
            this._TriggerEvents = e.CorrelationContext.PendingEvents.ToList();
            this.CorrelationContext = e.CorrelationContext;
            this.ParentId = e.ParentId;
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowInstanceSchedulingDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowInstanceSchedulingDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowInstanceSchedulingDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.Status = V1WorkflowInstanceStatus.Scheduling;
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowInstanceScheduledDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowInstanceScheduledDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowInstanceScheduledDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.Status = V1WorkflowInstanceStatus.Scheduled;
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowInstanceStartingDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowInstanceStartingDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowInstanceStartingDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.Status = V1WorkflowInstanceStatus.Starting;
            this._Sessions.Add(new(e.CreatedAt, e.ProcessId));
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowInstanceStartedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowInstanceStartedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowInstanceStartedDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.Status = V1WorkflowInstanceStatus.Running;
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowInstanceSuspendingDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowInstanceSuspendingDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowInstanceSuspendingDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.Status = V1WorkflowInstanceStatus.Suspending;
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowInstanceSuspendedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowInstanceSuspendedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowInstanceSuspendedDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.Status = V1WorkflowInstanceStatus.Suspended;
            var session = this.Sessions.FirstOrDefault(s => s.IsActive);
            if (session != null)
                session.MarkAsEnded(e.CreatedAt);
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowInstanceResumingDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowInstanceResumingDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowInstanceResumingDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.Status = V1WorkflowInstanceStatus.Resuming;
            this._Sessions.Add(new(e.CreatedAt, e.ProcessId));
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowInstanceResumedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowInstanceResumedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowInstanceResumedDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.Status = V1WorkflowInstanceStatus.Running; //todo: keep track of runtime sessions
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowCorrelationContextChangedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowCorrelationContextChangedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowCorrelationContextChangedDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.CorrelationContext = e.CorrelationContext;
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowCorrelationMappingSetDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowCorrelationMappingSetDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowCorrelationMappingSetDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.CorrelationContext.SetMapping(e.Key, e.Value);
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowInstanceFaultedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowInstanceFaultedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowInstanceFaultedDomainEvent e)
        {
            this.Status = V1WorkflowInstanceStatus.Faulted;
            this.Error = e.Error;
            this.On(this.RegisterEvent(new V1WorkflowInstanceExecutedDomainEvent(this.Id, this.Status, this.Error)));
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowInstanceCancellingDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowInstanceCancellingDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowInstanceCancellingDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.Status = V1WorkflowInstanceStatus.Cancelling;
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowInstanceCancelledDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowInstanceCancelledDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowInstanceCancelledDomainEvent e)
        {
            this.Status = V1WorkflowInstanceStatus.Cancelled;
            this.On(this.RegisterEvent(new V1WorkflowInstanceExecutedDomainEvent(this.Id, this.Status)));
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowInstanceCompletedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowInstanceCompletedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowInstanceCompletedDomainEvent e)
        {
            this.Status = V1WorkflowInstanceStatus.Completed;
            this.Output = e.Output;
            this.On(this.RegisterEvent(new V1WorkflowInstanceExecutedDomainEvent(this.Id, this.Status, this.Output)));
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowInstanceExecutedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowInstanceExecutedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowInstanceExecutedDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.ExecutedAt = e.CreatedAt;
            var session = this.Sessions.FirstOrDefault(s => s.IsActive);
            if (session != null)
                session.MarkAsEnded(e.CreatedAt);
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowInstanceDeletedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowInstanceDeletedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowInstanceDeletedDomainEvent e)
        {
            
        }

        /// <summary>
        /// Builds a <see cref="V1WorkflowInstance"/> unique identifier based on the specified key and <see cref="V1Workflow"/>
        /// </summary>
        /// <param name="key">The key of the <see cref="V1WorkflowInstance"/> to build the unique identifier for</param>
        /// <param name="workflow">The <see cref="V1Workflow"/> of the <see cref="V1WorkflowInstance"/> to build the unique identifier for</param>
        /// <returns>A <see cref="V1WorkflowInstance"/> unique identifier</returns>
        public static string BuildUniqueIdentifier(string key, V1Workflow workflow)
        {
            return $"{workflow.Definition.Id}-{key}";
        }

    }

}
