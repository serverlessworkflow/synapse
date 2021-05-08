using k8s.Models;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerlessWorkflow.Sdk.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents a <see cref="V1WorkflowInstance"/> activity
    /// </summary>
    public class V1WorkflowActivity
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowActivity"/>
        /// </summary>
        protected V1WorkflowActivity()
        {

        }

        /// <summary>
        ///  Initializes a new <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <param name="type">The <see cref="V1WorkflowActivity"/>'s type</param>
        /// <param name="workflowInstance">The <see cref="V1WorkflowInstance"/> the <see cref="V1WorkflowActivity"/> belongs to</param>
        /// <param name="data">The <see cref="V1WorkflowActivity"/>'s data</param>
        /// <param name="parent">The <see cref="V1WorkflowActivity"/>'s parent, if any</param>
        public V1WorkflowActivity(V1WorkflowActivityType type, V1WorkflowInstance workflowInstance, JToken data, V1WorkflowActivity parent = null)
        {
            if (workflowInstance == null)
                throw new ArgumentNullException(nameof(workflowInstance));
            this.Id = Guid.NewGuid();
            this.CreatedAt = DateTimeOffset.Now;
            this.Type = type;
            this.WorkflowInstance = workflowInstance.Name();
            this.Data = data ?? throw new ArgumentNullException(nameof(data));
            this.ParentId = parent?.Id;
        }

        /// <summary>
        ///  Initializes a new <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <param name="type">The <see cref="V1WorkflowActivity"/>'s type</param>
        /// <param name="workflowInstance">The <see cref="V1WorkflowInstance"/> the <see cref="V1WorkflowActivity"/> belongs to</param>
        /// <param name="data">The <see cref="V1WorkflowActivity"/>'s data</param>
        /// <param name="metadata">The <see cref="V1WorkflowActivity"/>'s metadata</param>
        /// <param name="parent">The <see cref="V1WorkflowActivity"/>'s parent, if any</param>
        public V1WorkflowActivity(V1WorkflowActivityType type, V1WorkflowInstance workflowInstance, JToken data, V1WorkflowActivityMetadata metadata, V1WorkflowActivity parent = null)
            : this(type, workflowInstance, data, parent)
        {
            this.Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        /// <summary>
        /// Gets the <see cref="V1WorkflowActivity"/>'s id
        /// </summary>
        [JsonProperty("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowActivity"/>'s type
        /// </summary>
        [JsonProperty("type")]
        public V1WorkflowActivityType Type { get; set; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowActivity"/>'s status
        /// </summary>
        [JsonProperty("status")]
        public V1WorkflowActivityStatus Status { get; set; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowActivity"/>'s instance name
        /// </summary>
        [JsonProperty("workflowInstance")]
        public string WorkflowInstance { get; set; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowActivity"/>'s parent id, if any
        /// </summary>
        [JsonProperty("parentId")]
        public Guid? ParentId { get; set; }

        /// <summary>
        /// Gets the date and time the <see cref="V1WorkflowActivity"/> has been created at
        /// </summary>
        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Gets the date and time the <see cref="V1WorkflowActivity"/> has been initialized at
        /// </summary>
        [JsonProperty("initializedAt")]
        public DateTimeOffset? InitializedAt { get; set; }

        /// <summary>
        /// Gets the date and time the <see cref="V1WorkflowActivity"/> has been started at
        /// </summary>
        [JsonProperty("startedAt")]
        public DateTimeOffset? StartedAt { get; set; }

        /// <summary>
        /// Gets the date and time the <see cref="V1WorkflowActivity"/> has been executed at
        /// </summary>
        [JsonProperty("executedAt")]
        public DateTimeOffset? ExecutedAt { get; set; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowActivity"/>'s data
        /// </summary>
        [JsonProperty("data")]
        public JToken Data { get; set; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowActivity"/>'s metadata
        /// </summary>
        [JsonProperty("metadata")]
        public V1WorkflowActivityMetadata Metadata { get; set; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowActivity"/>'s result
        /// </summary>
        [JsonProperty("result")]
        public V1WorkflowExecutionResult Result { get; set; }

        /// <summary>
        /// Gets a <see cref="List{T}"/> containing the <see cref="V1ExecutionInterruption"/>s that have occured during the <see cref="V1WorkflowActivity"/>'s execution
        /// </summary>
        [JsonProperty("interruptions")]
        public List<V1ExecutionInterruption> Interruptions { get; set; } = new List<V1ExecutionInterruption>();

        /// <summary>
        /// Gets a <see cref="List{T}"/> containing the <see cref="V1Error"/>s that have occured during the <see cref="V1WorkflowActivity"/>'s execution
        /// </summary>
        [JsonProperty("errors")]
        public List<V1Error> Errors { get; set; } = new List<V1Error>();

        /// <summary>
        /// Gets a boolean indicating whether or not the <see cref="V1WorkflowActivity"/> is active
        /// </summary>
        [JsonIgnore]
        public bool IsActive
        {
            get
            {
                return this.Status == V1WorkflowActivityStatus.Pending || this.Status == V1WorkflowActivityStatus.Executing || this.Status == V1WorkflowActivityStatus.Suspended;
            }
        }

        private JsonPatchDocument<V1WorkflowActivity> _Patch;
        /// <summary>
        /// Gets the <see cref="V1WorkflowActivity"/>'s <see cref="IJsonPatchDocument"/>
        /// </summary>
        [JsonIgnore]
        protected JsonPatchDocument<V1WorkflowActivity> Patch
        {
            get
            {
                if (this._Patch == null)
                    this._Patch = new JsonPatchDocument<V1WorkflowActivity>();
                return this._Patch;
            }
        }

        /// <summary>
        /// Gets the <see cref="V1WorkflowActivity"/>'s <see cref="IJsonPatchDocument"/>
        /// </summary>
        public JsonPatchDocument<V1WorkflowActivity> GetPatch()
        {
            return this._Patch;
        }

        /// <summary>
        /// Gets a boolean indicating whether or not the the <see cref="V1WorkflowActivity"/> has a patch pending
        /// </summary>
        [JsonIgnore]
        public bool HasPatch
        {
            get
            {
                return this._Patch != null && this._Patch.Operations.Any();
            }
        }

        /// <summary>
        /// Initializes the <see cref="V1WorkflowActivity"/>
        /// </summary>
        internal virtual void Initialize()
        {
            if (this.Status != V1WorkflowActivityStatus.Pending)
                throw DomainException.UnexpectedState(typeof(V1WorkflowActivity), this.Id, this.Status);
            this.Patch.Replace(a => a.Status, V1WorkflowActivityStatus.Initializing);
            this.Patch.Replace(a => a.InitializedAt, DateTimeOffset.Now);
            this.Patch.ApplyTo(this);
        }

        /// <summary>
        /// Processes the <see cref="V1WorkflowActivity"/>
        /// </summary>
        internal virtual void Process()
        {
            switch (this.Status)
            {
                case V1WorkflowActivityStatus.Initializing:
                    this.Patch.Replace(a => a.StartedAt, DateTimeOffset.Now);
                    this.Patch.Replace(a => a.Status, V1WorkflowActivityStatus.Executing);
                    this.Patch.ApplyTo(this);
                    break;
                case V1WorkflowActivityStatus.Suspended:
                    this.Resume();
                    break;
                default:
                    throw DomainException.UnexpectedState(typeof(V1WorkflowActivity), this.Id, this.Status);
            }
        }

        /// <summary>
        /// Suspends the <see cref="V1WorkflowActivity"/>
        /// </summary>
        internal virtual void Suspend()
        {
            if(this.Status != V1WorkflowActivityStatus.Executing)
                throw DomainException.UnexpectedState(typeof(V1WorkflowActivity), this.Id, this.Status);
            V1ExecutionInterruption interruption = new(DateTimeOffset.Now);
            this.Patch.Replace(a => a.Status, V1WorkflowActivityStatus.Suspended);
            this.Patch.Add(a => a.Interruptions, interruption);
            this.Patch.ApplyTo(this);
        }

        /// <summary>
        /// Resumes the <see cref="V1WorkflowActivity"/>
        /// </summary>
        internal virtual void Resume()
        {
            if (this.Status != V1WorkflowActivityStatus.Suspended)
                throw DomainException.UnexpectedState(typeof(V1WorkflowActivity), this.Id, this.Status);
            V1ExecutionInterruption interruption = this.Interruptions.OrderBy(i => i.SuspendedAt).Last(i => !i.HasBeenResumed);
            int index = this.Interruptions.IndexOf(interruption);
            this.Patch.Replace(a => a.Status, V1WorkflowActivityStatus.Executing);
            this.Patch.Replace(a => a.Interruptions, interruption, index);
            this.Patch.ApplyTo(this);
        }

        /// <summary>
        /// Faults the <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <param name="ex">The <see cref="Exception"/> that has been thrown during the <see cref="V1WorkflowActivity"/>'s execution</param>
        internal virtual void Fault(Exception ex)
        {
            if (ex == null)
                throw DomainException.ArgumentNull(nameof(ex));
            if (this.Status >= V1WorkflowActivityStatus.Faulted)
                throw DomainException.UnexpectedState(typeof(V1WorkflowActivity), this.Id, this.Status);
            this.Patch.Replace(a => a.Status, V1WorkflowActivityStatus.Faulted);
            this.Patch.Add(a => a.Errors, ex.ToV1Error());
            this.Patch.Replace(a => a.ExecutedAt, DateTimeOffset.Now);
            this.Patch.ApplyTo(this);
        }

        /// <summary>
        /// Terminates the <see cref="V1WorkflowActivity"/>
        /// </summary>
        internal virtual void Terminate()
        {
            if(this.Status >= V1WorkflowActivityStatus.Faulted)
                throw DomainException.UnexpectedState(typeof(V1WorkflowActivity), this.Id, this.Status);
            this.Patch.Replace(a => a.Status, V1WorkflowActivityStatus.Faulted);
            this.Patch.Replace(a => a.ExecutedAt, DateTimeOffset.Now);
            this.Patch.ApplyTo(this);
        }

        /// <summary>
        /// Times out the <see cref="V1WorkflowActivity"/>
        /// </summary>
        internal virtual void TimeOut()
        {
            if (this.Status >= V1WorkflowActivityStatus.Faulted)
                throw DomainException.UnexpectedState(typeof(V1WorkflowActivity), this.Id, this.Status);
            this.Patch.Replace(a => a.Status, V1WorkflowActivityStatus.TimedOut);
            this.Patch.Replace(a => a.ExecutedAt, DateTimeOffset.Now);
            this.Patch.ApplyTo(this);
        }

        /// <summary>
        /// Updates the <see cref="V1WorkflowActivity"/>'s data
        /// </summary>
        internal virtual void UpdateData(JToken data)
        {
            if(data == null)
                throw DomainException.ArgumentNull(nameof(data));
            if (this.Status >= V1WorkflowActivityStatus.Faulted)
                throw DomainException.UnexpectedState(typeof(V1WorkflowActivity), this.Id, this.Status);
            this.Patch.Replace(a => a.Data, data);
            this.Patch.ApplyTo(this);
        }

        /// <summary>
        /// Sets the <see cref="V1WorkflowActivity"/>'s execution result
        /// </summary>
        internal virtual void SetResult(V1WorkflowExecutionResult result)
        {
            if (result == null)
                throw DomainException.ArgumentNull(nameof(result));
            this.Patch.Replace(a => a.Status, V1WorkflowActivityStatus.Executed);
            this.Patch.Replace(a => a.Result, result);
            this.Patch.Replace(a => a.ExecutedAt, DateTimeOffset.Now);
            this.Patch.ApplyTo(this);
        }

        #region Static Instanciation Helpers

        /// <summary>
        /// Creates a new <see cref="V1WorkflowActivity"/> for the specified <see cref="StateDefinition"/>
        /// </summary>
        /// <param name="workflowInstance">The <see cref="V1WorkflowInstance"/> that owns the <see cref="V1WorkflowActivity"/> to create</param>
        /// <param name="state">The <see cref="StateDefinition"/> to create the <see cref="V1WorkflowActivity"/> for</param>
        /// <param name="input">The input data</param>
        /// <param name="parent">The parent <see cref="V1WorkflowActivity"/>, if any</param>
        /// <returns>A new <see cref="V1WorkflowActivity"/></returns>
        public static V1WorkflowActivity State(V1WorkflowInstance workflowInstance, StateDefinition state, JToken input, V1WorkflowActivity parent = null)
        {
            if (workflowInstance == null)
                throw new ArgumentNullException(nameof(workflowInstance));
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            return new V1WorkflowActivity(V1WorkflowActivityType.State, workflowInstance, input, new V1WorkflowActivityMetadata() { State = state.Name }, parent);
        }

        /// <summary>
        /// Creates a new <see cref="V1WorkflowActivity"/> that execute a transition from the specified state
        /// </summary>
        /// <param name="workflowInstance">The <see cref="V1WorkflowInstance"/> that owns the <see cref="V1WorkflowActivity"/> to create</param>
        /// <param name="state">The <see cref="StateDefinition"/> to transition from</param>
        /// <param name="input">The input data</param>
        /// <param name="parent">The parent <see cref="V1WorkflowActivity"/>, if any</param>
        /// <returns>A new <see cref="V1WorkflowActivity"/></returns>
        public static V1WorkflowActivity Transition(V1WorkflowInstance workflowInstance, StateDefinition state, JToken input, V1WorkflowActivity parent = null)
        {
            if (workflowInstance == null)
                throw new ArgumentNullException(nameof(workflowInstance));
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            return new V1WorkflowActivity(V1WorkflowActivityType.Transition, workflowInstance, input, new V1WorkflowActivityMetadata() { State = state.Name }, parent);
        }

        /// <summary>
        /// Creates a new <see cref="V1WorkflowActivity"/> that execute a transition from the specified <see cref="SwitchStateDefinition"/>'s condition
        /// </summary>
        /// <param name="workflowInstance">The <see cref="V1WorkflowInstance"/> that owns the <see cref="V1WorkflowActivity"/> to create</param>
        /// <param name="state">The <see cref="SwitchStateDefinition"/> to transition from</param>
        /// <param name="caseName">The name of the <see cref="SwitchCaseDefinition"/> to process the <see cref="TransitionDefinition"/> for</param>
        /// <param name="input">The input data</param>
        /// <param name="parent">The parent <see cref="V1WorkflowActivity"/>, if any</param>
        /// <returns>A new <see cref="V1WorkflowActivity"/></returns>
        public static V1WorkflowActivity Transition(V1WorkflowInstance workflowInstance, SwitchStateDefinition state, string caseName, JToken input, V1WorkflowActivity parent = null)
        {
            if (workflowInstance == null)
                throw new ArgumentNullException(nameof(workflowInstance));
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            if (string.IsNullOrWhiteSpace(caseName))
                throw new ArgumentNullException(nameof(caseName));
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            return new V1WorkflowActivity(V1WorkflowActivityType.Transition, workflowInstance, input, new V1WorkflowActivityMetadata() { State = state.Name, Case = caseName }, parent);
        }

        /// <summary>
        /// Creates a new <see cref="V1WorkflowActivity"/> for the specified <see cref="EndDefinition"/>
        /// </summary>
        /// <param name="workflowInstance">The <see cref="V1WorkflowInstance"/> that owns the <see cref="V1WorkflowActivity"/> to create</param>
        /// <param name="state">The <see cref="StateDefinition"/> that owns the <see cref="V1WorkflowActivity"/> to create</param>
        /// <param name="input">The input data</param>
        /// <param name="parent">The parent <see cref="V1WorkflowActivity"/>, if any</param>
        /// <returns>A new <see cref="V1WorkflowActivity"/></returns>
        public static V1WorkflowActivity End(V1WorkflowInstance workflowInstance, StateDefinition state, JToken input, V1WorkflowActivity parent = null)
        {
            return new V1WorkflowActivity(V1WorkflowActivityType.End, workflowInstance, input, new V1WorkflowActivityMetadata() { State = state.Name }, parent);
        }

        /// <summary>
        /// Creates a new <see cref="V1WorkflowActivity"/> for the specified <see cref="EndDefinition"/>
        /// </summary>
        /// <param name="workflowInstance">The <see cref="V1WorkflowInstance"/> that owns the <see cref="V1WorkflowActivity"/> to create</param>
        /// <param name="state">The <see cref="StateDefinition"/> that owns the <see cref="V1WorkflowActivity"/> to create</param>
        /// <param name="caseName">The name of the <see cref="SwitchCaseDefinition"/> to process the <see cref="EndDefinition"/> for</param>
        /// <param name="input">The input data</param>
        /// <param name="parent">The parent <see cref="V1WorkflowActivity"/>, if any</param>
        /// <returns>A new <see cref="V1WorkflowActivity"/></returns>
        public static V1WorkflowActivity End(V1WorkflowInstance workflowInstance, SwitchStateDefinition state, string caseName, JToken input, V1WorkflowActivity parent = null)
        {
            return new V1WorkflowActivity(V1WorkflowActivityType.End, workflowInstance, input, new V1WorkflowActivityMetadata() { State = state.Name, Case = caseName }, parent);
        }

        /// <summary>
        /// Creates a new <see cref="V1WorkflowActivity"/> for the specified <see cref="ActionDefinition"/>
        /// </summary>
        /// <param name="workflowInstance">The <see cref="V1WorkflowInstance"/> that owns the <see cref="V1WorkflowActivity"/> to create</param>
        /// <param name="state">The <see cref="StateDefinition"/> that owns the <see cref="V1WorkflowActivity"/> to create</param>
        /// <param name="action">The <see cref="ActionDefinition"/> to create the <see cref="V1WorkflowActivity"/> for</param>
        /// <param name="input">The input data</param>
        /// <param name="parent">The parent <see cref="V1WorkflowActivity"/></param>
        /// <returns>A new <see cref="V1WorkflowActivity"/></returns>
        public static V1WorkflowActivity Action(V1WorkflowInstance workflowInstance, StateDefinition state, ActionDefinition action, JToken input, V1WorkflowActivity parent)
        {
            if (workflowInstance == null)
                throw new ArgumentNullException(nameof(workflowInstance));
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            return new V1WorkflowActivity(V1WorkflowActivityType.Action, workflowInstance, input, new V1WorkflowActivityMetadata() { State = state.Name, Action = action.Name }, parent);
        }

        /// <summary>
        /// Creates a new <see cref="V1WorkflowActivity"/> for the specified <see cref="ActionDefinition"/>
        /// </summary>
        /// <param name="workflowInstance">The <see cref="V1WorkflowInstance"/> that owns the <see cref="V1WorkflowActivity"/> to create</param>
        /// <param name="state">The <see cref="EventStateDefinition"/> that owns the <see cref="V1WorkflowActivity"/> to create</param>
        /// <param name="trigger">The <see cref="EventStateTriggerDefinition"/> the <see cref="ActionDefinition"/> to process belongs to</param>
        /// <param name="action">The <see cref="ActionDefinition"/> to create the <see cref="V1WorkflowActivity"/> for</param>
        /// <param name="input">The input data</param>
        /// <param name="parent">The parent <see cref="V1WorkflowActivity"/></param>
        /// <returns>A new <see cref="V1WorkflowActivity"/></returns>
        public static V1WorkflowActivity Action(V1WorkflowInstance workflowInstance, EventStateDefinition state, EventStateTriggerDefinition trigger, ActionDefinition action, JToken input, V1WorkflowActivity parent)
        {
            if (workflowInstance == null)
                throw new ArgumentNullException(nameof(workflowInstance));
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            if (trigger == null)
                throw new ArgumentNullException(nameof(trigger));
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            return new V1WorkflowActivity(V1WorkflowActivityType.Action, workflowInstance, input, new V1WorkflowActivityMetadata() { State = state.Name, Action = action.Name, TriggerId = state.Triggers.ToList().IndexOf(trigger) }, parent);
        }

        /// <summary>
        /// Creates a new <see cref="V1WorkflowActivity"/> for the specified <see cref="EventStateTriggerDefinition"/>
        /// </summary>
        /// <param name="workflowInstance">The <see cref="V1WorkflowInstance"/> that owns the <see cref="V1WorkflowActivity"/> to create</param>
        /// <param name="state">The <see cref="EventStateDefinition"/> the <see cref="EventStateTriggerDefinition"/> to process belongs to</param>
        /// <param name="trigger">The <see cref="EventStateTriggerDefinition"/> to create the <see cref="V1WorkflowActivity"/> for</param>
        /// <param name="input">The input data</param>
        /// <param name="parent">The parent <see cref="V1WorkflowActivity"/></param>
        /// <returns>A new <see cref="V1WorkflowActivity"/></returns>
        public static V1WorkflowActivity EventStateTrigger(V1WorkflowInstance workflowInstance, EventStateDefinition state, EventStateTriggerDefinition trigger, JToken input, V1WorkflowActivity parent)
        {
            if (workflowInstance == null)
                throw new ArgumentNullException(nameof(workflowInstance));
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            if (trigger == null)
                throw new ArgumentNullException(nameof(trigger));
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            return new V1WorkflowActivity(V1WorkflowActivityType.EventStateTrigger, workflowInstance, input, new V1WorkflowActivityMetadata() { State = state.Name, TriggerId = state.Triggers.ToList().IndexOf(trigger) }, parent);
        }

        /// <summary>
        /// Creates a new <see cref="V1WorkflowActivity"/> for the specified <see cref="EventDefinition"/> to consume
        /// </summary>
        /// <param name="workflowInstance">The <see cref="V1WorkflowInstance"/> that owns the <see cref="V1WorkflowActivity"/> to create</param>
        /// <param name="state">The <see cref="StateDefinition"/> the <see cref="EventDefinition"/> to process belongs to</param>
        /// <param name="eventRef">The reference of the <see cref="EventDefinition"/> to consume</param>
        /// <param name="input">The input data</param>
        /// <param name="parent">The parent <see cref="V1WorkflowActivity"/></param>
        /// <returns>A new <see cref="V1WorkflowActivity"/></returns>
        public static V1WorkflowActivity ConsumeEvent(V1WorkflowInstance workflowInstance, StateDefinition state, string eventRef, JToken input, V1WorkflowActivity parent)
        {
            if (workflowInstance == null)
                throw new ArgumentNullException(nameof(workflowInstance));
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            if (parent == null)
                if (string.IsNullOrWhiteSpace(eventRef))
                throw new ArgumentNullException(nameof(eventRef));
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            return new V1WorkflowActivity(V1WorkflowActivityType.ConsumeEvent, workflowInstance, input, new V1WorkflowActivityMetadata() { State = state.Name, Event = eventRef }, parent);
        }

        #endregion

    }

}
