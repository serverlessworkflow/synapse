using CloudNative.CloudEvents;
using k8s.Models;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json.Linq;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Domain.Events.WorkflowInstances;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents an instance of a <see cref="V1WorkflowDefinition"/>
    /// </summary>
    public class V1WorkflowInstance
        : CustomResourceAggregate<V1WorkflowInstanceSpec, V1WorkflowInstanceStatus>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstance"/>
        /// </summary>
        public V1WorkflowInstance() 
            : base(new V1WorkflowInstanceDefinition())
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstance"/>
        /// </summary>
        /// <param name="spec">The <see cref="V1WorkflowInstance"/>'s spec</param>
        public V1WorkflowInstance(V1WorkflowInstanceSpec spec)
            : this()
        {
            this.Spec = spec ?? throw new ArgumentNullException(nameof(spec));
        }

        /// <inheritdoc/>
        protected new JsonPatchDocument<V1WorkflowInstance> Patch
        {
            get
            {
                return (JsonPatchDocument<V1WorkflowInstance>)base.Patch;
            }
        }

        /// <inheritdoc/>
        protected new JsonPatchDocument<V1WorkflowInstance> StatusPatch
        {
            get
            {
                return (JsonPatchDocument<V1WorkflowInstance>)base.StatusPatch;
            }
        }

        /// <summary>
        /// Initializes the <see cref="V1WorkflowInstance"/>
        /// </summary>
        public virtual void Initialize()
        {
            this.On(this.RegisterEvent(new V1WorkflowInstanceInitializingDomainEvent(this.Id)));
        }

        /// <summary>
        /// Sets the version of the <see cref="V1WorkflowInstance"/>'s <see cref="WorkflowDefinition"/>
        /// </summary>
        /// <param name="version">The version of the <see cref="V1WorkflowInstance"/>'s <see cref="WorkflowDefinition"/></param>
        /// <returns>A boolean indicating whether or not the operation was successfull</returns>
        public virtual void SetVersion(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
                throw DomainException.ArgumentNullOrWhitespace(nameof(version));
            if (this.Status.Type != V1WorkflowActivityStatus.Initializing)
                throw DomainException.UnexpectedState(typeof(V1Workflow), this.Name(), this.Status.Type);
            this.Patch.Replace(w => w.Spec.Definition.Version, version);
            this.Patch.ApplyTo(this);
        }

        /// <summary>
        /// Labels the <see cref="V1WorkflowInstance"/>
        /// </summary>
        /// <returns>A boolean indicating whether or not the operation was successfull</returns>
        public virtual bool Label()
        {
            if (this.Status.Type != V1WorkflowActivityStatus.Initializing)
                throw DomainException.UnexpectedState(typeof(V1Workflow), this.Name(), this.Status.Type);
            bool updated = false;
            IDictionary<string, string> labels = this.Metadata.Labels;
            if (labels != null && labels.ContainsKey(SynapseConstants.Labels.Scheduled))
                labels.Remove(SynapseConstants.Labels.Scheduled);
            if (labels == null || !labels.ContainsKey(SynapseConstants.Labels.Workflows.Id))
            {
                this.SetLabel(SynapseConstants.Labels.Workflows.Id, this.Spec.Definition.Id);
                updated = true;
            }
            if (labels == null || !labels.ContainsKey(SynapseConstants.Labels.Workflows.Version))
            {
                this.SetLabel(SynapseConstants.Labels.Workflows.Version, this.Spec.Definition.Version);
                updated = true;
            }
            if (!updated)
                return false;
            this.Patch.Replace(w => w.Metadata.Labels, this.Metadata.Labels);
            return true;
        }

        /// <summary>
        /// Deploys the <see cref="V1WorkflowInstance"/>
        /// </summary>
        /// <param name="podName">The name of the <see cref="V1Pod"/> to deploy the <see cref="V1WorkflowInstance"/> to</param>
        public virtual void Deploy(string podName)
        {
            if (string.IsNullOrWhiteSpace(podName))
                throw DomainException.ArgumentNull(nameof(podName));
            this.On(this.RegisterEvent(new V1WorkflowInstanceDeployedDomainEvent(this.Id, new V1ObjectReference(V1Pod.KubeApiVersion, kind: V1Pod.KubeKind, name: podName, namespaceProperty: SynapseConstants.EnvironmentVariables.Kubernetes.Namespace.Value))));
        }

        /// <summary>
        /// Starts the <see cref="V1WorkflowInstance"/>
        /// </summary>
        public virtual void Start()
        {
            if (this.Status.Type != V1WorkflowActivityStatus.Deployed)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status.Type);
            this.On(this.RegisterEvent(new V1WorkflowInstanceStartedDomainEvent(this.Id)));
        }

        /// <summary>
        /// Suspends the <see cref="V1WorkflowInstance"/>'s execution
        /// </summary>
        public virtual void Suspend()
        {
            if (this.Status.Type != V1WorkflowActivityStatus.Executing)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status.Type);
            this.On(this.RegisterEvent(new V1WorkflowInstanceSuspendedDomainEvent(this.Id)));
        }

        /// <summary>
        /// Resumes the <see cref="V1WorkflowInstance"/>'s execution
        /// </summary>
        public virtual void Resume()
        {
            if (this.Status.Type != V1WorkflowActivityStatus.Awakened)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status.Type);
            this.On(this.RegisterEvent(new V1WorkflowInstanceResumedDomainEvent(this.Id)));
        }

        /// <summary>
        /// Wakes the <see cref="V1WorkflowInstance"/> up
        /// </summary>
        public virtual void WakeUp()
        {
            if (this.Status.Type != V1WorkflowActivityStatus.Suspended
               && this.Status.Type != V1WorkflowActivityStatus.Waiting)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status.Type);
            this.On(this.RegisterEvent(new V1WorkflowInstanceAwakingDomainEvent(this.Id)));
        }

        /// <summary>
        /// Marks the <see cref="V1WorkflowInstance"/> as awake
        /// </summary>
        public virtual void Awaken()
        {
            if (this.Status.Type != V1WorkflowActivityStatus.Awakening)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status.Type);
            this.On(this.RegisterEvent(new V1WorkflowInstanceAwakenedDomainEvent(this.Id)));
        }

        /// <summary>
        /// Executes the <see cref="V1WorkflowInstance"/>
        /// </summary>
        public virtual void Execute()
        {
            switch (this.Status.Type)
            {
                case V1WorkflowActivityStatus.Deployed:
                    this.Start();
                    break;
                case V1WorkflowActivityStatus.Waiting:
                case V1WorkflowActivityStatus.Awakened:
                    this.Resume();
                    break;
                default:
                    throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status.Type);
            }
        }

        /// <summary>
        /// Puts the <see cref="V1WorkflowInstance"/> while correlating it to consumed <see cref="CloudEvent"/>s
        /// </summary>
        public virtual void WaitForEvents()
        {
            if (this.Status.Type != V1WorkflowActivityStatus.Executing)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status.Type);
            this.On(this.RegisterEvent(new V1WorkflowInstanceWaitingForEventsDomainEvent(this.Id)));
        }

        /// <summary>
        /// Sets the <see cref="V1Workflow"/>'s <see cref="V1CorrelationContext"/>
        /// </summary>
        /// <param name="correlationContext">The <see cref="V1CorrelationContext"/> to set</param>
        public virtual void SetCorrelationContext(V1CorrelationContext correlationContext)
        {
            if (correlationContext == null)
                throw DomainException.ArgumentNull(nameof(correlationContext));
            if (this.Status.Type != V1WorkflowActivityStatus.Waiting)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status.Type);
            this.On(this.RegisterEvent(new V1WorkflowCorrelationContextUpdatedDomainEvent(this.Id, correlationContext)));
        }

        /// <summary>
        /// Sets the specified correlation key
        /// </summary>
        /// <param name="key">The correlation key to set</param>
        /// <param name="value">The value of the correlation key to set</param>
        public virtual void SetCorrelationKey(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw DomainException.ArgumentNull(nameof(key));
            if (string.IsNullOrWhiteSpace(value))
                throw DomainException.ArgumentNull(nameof(value));
            if (this.Status.Type != V1WorkflowActivityStatus.Executing)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status.Type);
            this.On(this.RegisterEvent(new V1WorkflowCorrelationKeySetDomainEvent(this.Id, key, value)));
        }

        /// <summary>
        /// Correlates the specified <see cref="CloudEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1CloudEvent"/> to correlate</param>
        /// <param name="contextAttributes">An <see cref="IEnumerable{T}"/> containing the context attributes used to correlate the specified <see cref="V1CloudEvent"/></param>
        public virtual void Correlate(V1CloudEvent e, IEnumerable<string> contextAttributes)
        {
            if (e == null)
                throw DomainException.ArgumentNull(nameof(e));
            if(contextAttributes == null)
                contextAttributes = Array.Empty<string>();
            if (this.Status.Type != V1WorkflowActivityStatus.Executing)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status.Type);
            this.On(this.RegisterEvent(new V1WorkflowInstanceCloudEventCorrelatedDomainEvent(this.Id, e, contextAttributes)));
        }

        /// <summary>
        /// Consumes the specified <see cref="V1CloudEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1CloudEvent"/> to consume</param>
        public virtual void ConsumeBootstrapEvent(V1CloudEvent e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            int index = this.Status.CorrelationContext.BootstrapEvents.IndexOf(e);
            if (index < 0)
                return;
            this.StatusPatch.Remove(w => w.Status.CorrelationContext.BootstrapEvents, index);
            this.StatusPatch.ApplyTo(this);
        }

        /// <summary>
        /// Adds the specified <see cref="V1WorkflowActivity"/> to the <see cref="V1WorkflowInstance"/>'s activity log
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to add</param>
        public virtual void AddActivity(V1WorkflowActivity activity)
        {
            if (activity == null)
                throw DomainException.ArgumentNull(nameof(activity));
            if (this.Status.Type != V1WorkflowActivityStatus.Executing)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status.Type);
            this.StatusPatch.Add(w => w.Status.ActivityLog, activity);
            this.StatusPatch.ApplyTo(this);
        }

        /// <summary>
        /// Initializes the specified <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to initialize</param>
        public virtual void InitializeActivity(V1WorkflowActivity activity)
        {
            if (activity == null)
                throw DomainException.ArgumentNull(nameof(activity));
            if (this.Status.Type != V1WorkflowActivityStatus.Executing)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status.Type);
            activity.Initialize();
            this.UpdateActivity(activity);
        }

        /// <summary>
        /// Executes the specified <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to execute</param>
        public virtual void ExecuteActivity(V1WorkflowActivity activity)
        {
            if (activity == null)
                throw DomainException.ArgumentNull(nameof(activity));
            if (this.Status.Type != V1WorkflowActivityStatus.Executing)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status.Type);
            activity.Execute();
            this.UpdateActivity(activity);
        }

        /// <summary>
        /// Suspends the specified <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to suspend</param>
        public virtual void SuspendActivity(V1WorkflowActivity activity)
        {
            if (activity == null)
                throw DomainException.ArgumentNull(nameof(activity));
            if (activity.Status != V1WorkflowActivityStatus.Executing)
                throw DomainException.UnexpectedState(typeof(V1WorkflowActivity), activity.Id, activity.Status);
            activity.Suspend();
            this.UpdateActivity(activity);
        }

        /// <summary>
        /// Faults the specified <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> that has faulted</param>
        /// <param name="ex">The <see cref="Exception"/> that has occured during the specified <see cref="V1WorkflowActivity"/>'s processing</param>
        public virtual void FaultActivity(V1WorkflowActivity activity, Exception ex)
        {
            if (activity == null)
                throw DomainException.ArgumentNull(nameof(activity));
            if (ex == null)
                throw DomainException.ArgumentNull(nameof(ex));
            if (activity.Status >= V1WorkflowActivityStatus.Faulted)
                throw DomainException.UnexpectedState(typeof(V1WorkflowActivity), activity.Id, activity.Status);
            activity.Fault(ex);
            this.UpdateActivity(activity);
        }

        /// <summary>
        /// Terminates the specified <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to terminate</param>
        public virtual void TerminateActivity(V1WorkflowActivity activity)
        {
            if (activity == null)
                throw DomainException.ArgumentNull(nameof(activity));
            if (activity.Status >= V1WorkflowActivityStatus.Faulted)
                throw DomainException.UnexpectedState(typeof(V1WorkflowActivity), activity.Id, activity.Status);
            activity.Terminate();
            this.UpdateActivity(activity);
        }

        /// <summary>
        /// Updates the data of the specified <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to update the data of</param>
        /// <param name="data">The updated <see cref="V1WorkflowActivity"/> data</param>
        public virtual void UpdateActivityData(V1WorkflowActivity activity, JToken data)
        {
            if (activity == null)
                throw DomainException.ArgumentNull(nameof(activity));
            if (data == null)
                throw DomainException.ArgumentNull(nameof(data));
            if (activity.Status >= V1WorkflowActivityStatus.Faulted)
                throw DomainException.UnexpectedState(typeof(V1WorkflowActivity), activity.Id, activity.Status);
            activity.UpdateData(data);
            this.UpdateActivity(activity);
        }

        /// <summary>
        /// Set the specified <see cref="V1WorkflowActivity"/>'s <see cref="V1WorkflowExecutionResult"/>
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to set the <see cref="V1WorkflowExecutionResult"/> for</param>
        /// <param name="result">The specified <see cref="V1WorkflowActivity"/>'s <see cref="V1WorkflowExecutionResult"/></param>
        public virtual void SetActivityResult(V1WorkflowActivity activity, V1WorkflowExecutionResult result)
        {
            if (activity == null)
                throw DomainException.ArgumentNull(nameof(activity));
            if (result == null)
                throw DomainException.ArgumentNull(nameof(result));
            if (activity.Status >= V1WorkflowActivityStatus.Faulted)
                throw DomainException.UnexpectedState(typeof(V1WorkflowActivity), activity.Id, activity.Status);
            activity.SetResult(result);
            this.UpdateActivity(activity);
        }

        /// <summary>
        /// Updates the specified <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to update</param>
        protected virtual void UpdateActivity(V1WorkflowActivity activity)
        {
            V1WorkflowActivity match = this.Status.ActivityLog.FirstOrDefault(a => a.Id == activity.Id);
            if (match == null)
                throw DomainException.NullReference(typeof(V1WorkflowActivity), activity.Id);
            int index = this.Status.ActivityLog.IndexOf(match);
            this.StatusPatch.Replace(w => w.Status.ActivityLog, activity, index);
            this.StatusPatch.ApplyTo(this);
        }

        /// <summary>
        /// Operates a transition to the specified <see cref="StateDefinition"/>
        /// </summary>
        /// <param name="state">The <see cref="StateDefinition"/> to transition to</param>
        public virtual void TransitionTo(StateDefinition state)
        {
            if (this.Status.Type != V1WorkflowActivityStatus.Executing)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status);
            if (state == null)
                throw DomainException.ArgumentNull(nameof(state));
            this.RegisterEvent(new V1WorkflowInstanceTransitionedDomainEvent(this.Id, state.Name));
        }

        /// <summary>
        /// Faults the <see cref="V1WorkflowInstance"/>
        /// </summary>
        /// <param name="ex">The <see cref="Exception"/> that has occured during the <see cref="V1WorkflowInstance"/>'s execution</param>
        public virtual void Fault(Exception ex)
        {
            if (ex == null)
                throw DomainException.ArgumentNull(nameof(ex));
            if (this.Status.Type != V1WorkflowActivityStatus.Executing
              && this.Status.Type != V1WorkflowActivityStatus.Suspended)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1WorkflowInstanceFaultedDomainEvent(this.Id, new V1Error[] { ex.ToV1Error() })));
        }

        /// <summary>
        /// Times out the <see cref="V1WorkflowInstance"/>'s execution
        /// </summary>
        public virtual void TimeOut()
        {
            if (this.Status.Type != V1WorkflowActivityStatus.Pending
                && this.Status.Type != V1WorkflowActivityStatus.Executing)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1WorkflowInstanceTimedOutDomainEvent(this.Id)));
        }

        /// <summary>
        /// Terminates the <see cref="V1WorkflowInstance"/>'s execution
        /// </summary>
        public virtual void Terminate()
        {
            if (this.Status.Type != V1WorkflowActivityStatus.Pending
               && this.Status.Type != V1WorkflowActivityStatus.Executing
               && this.Status.Type != V1WorkflowActivityStatus.Suspended)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1WorkflowInstanceTerminatedDomainEvent(this.Id)));
        }

        /// <summary>
        /// Sets the <see cref="V1WorkflowInstance"/>'s output
        /// </summary>
        /// <param name="output">The <see cref="V1WorkflowInstance"/>'s output</param>
        public virtual void SetOutput(JToken output)
        {
            if (output == null)
                throw DomainException.ArgumentNull(nameof(output));
            if (this.Status.Type >= V1WorkflowActivityStatus.Faulted)
                throw DomainException.UnexpectedState(typeof(V1WorkflowInstance), this.Id, this.Status);
            this.On(this.RegisterEvent(new V1WorkflowInstanceExecutedDomainEvent(this.Id, output)));
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowInstanceInitializingDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowInstanceInitializingDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowInstanceInitializingDomainEvent e)
        {
            V1WorkflowInstanceStatus status = new() { Type = V1WorkflowActivityStatus.Initializing };
            if (this.Spec.CorrelationContext != null)
                status.CorrelationContext = this.Spec.CorrelationContext;
            this.StatusPatch.Replace(w => w.Status, status);
            this.StatusPatch.Replace(w => w.Status.InitializedAt, DateTimeOffset.Now);
            this.StatusPatch.ApplyTo(this);
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowInstanceDeployedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowInstanceDeployedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowInstanceDeployedDomainEvent e)
        {
            this.StatusPatch.Replace(w => w.Status.Type, V1WorkflowActivityStatus.Deployed);
            this.StatusPatch.Replace(w => w.Status.DeployedAt, DateTimeOffset.Now);
            this.StatusPatch.Replace(w => w.Status.Pod, e.Pod);
            this.StatusPatch.ApplyTo(this);
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowInstanceStartedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowInstanceStartedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowInstanceStartedDomainEvent e)
        {
            this.StatusPatch.Replace(w => w.Status.Type, V1WorkflowActivityStatus.Executing);
            this.StatusPatch.ApplyTo(this);
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowInstanceSuspendedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowInstanceSuspendedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowInstanceSuspendedDomainEvent e)
        {
            this.StatusPatch.Replace(w => w.Status.Type, V1WorkflowActivityStatus.Suspended);
            this.StatusPatch.Add(w => w.Status.Interruptions, new V1ExecutionInterruption(DateTimeOffset.Now));
            this.StatusPatch.ApplyTo(this);
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowInstanceResumedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowInstanceResumedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowInstanceResumedDomainEvent e)
        {
            V1ExecutionInterruption interruption = this.Status.Interruptions.Last(i => !i.HasBeenResumed);
            int interruptionIndex = this.Status.Interruptions.IndexOf(interruption);
            interruption.Resume(DateTimeOffset.Now);
            this.StatusPatch.Replace(w => w.Status.Type, V1WorkflowActivityStatus.Executing);
            this.StatusPatch.Replace(w => w.Status.Interruptions, interruption, interruptionIndex);
            this.StatusPatch.ApplyTo(this);
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowInstanceAwakingDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowInstanceAwakingDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowInstanceAwakingDomainEvent e)
        {
            this.StatusPatch.Replace(w => w.Status.Type, V1WorkflowActivityStatus.Awakening);
            this.StatusPatch.ApplyTo(this);
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowInstanceAwakenedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowInstanceAwakenedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowInstanceAwakenedDomainEvent e)
        {
            this.StatusPatch.Replace(w => w.Status.Type, V1WorkflowActivityStatus.Awakened);
            this.StatusPatch.ApplyTo(this);
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowInstanceWaitingForEventsDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowInstanceWaitingForEventsDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowInstanceWaitingForEventsDomainEvent e)
        {
            this.StatusPatch.Replace(w => w.Status.Type, V1WorkflowActivityStatus.Waiting);
            this.StatusPatch.Add(w => w.Status.Interruptions, new V1ExecutionInterruption(DateTimeOffset.Now));
            this.StatusPatch.ApplyTo(this);
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowCorrelationContextUpdatedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowCorrelationContextUpdatedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowCorrelationContextUpdatedDomainEvent e)
        {
            this.StatusPatch.Replace(w => w.Status.CorrelationContext, e.CorrelationContext);
            this.StatusPatch.ApplyTo(this);
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowCorrelationKeySetDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowCorrelationKeySetDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowCorrelationKeySetDomainEvent e)
        {
            this.Status.CorrelationContext.ContextAttributes[e.Key] = e.Value;
            this.StatusPatch.Replace(w => w.Status.CorrelationContext.ContextAttributes, this.Status.CorrelationContext.ContextAttributes);
            this.StatusPatch.ApplyTo(this);
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowInstanceCloudEventCorrelatedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowInstanceCloudEventCorrelatedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowInstanceCloudEventCorrelatedDomainEvent e)
        {
            this.Status.CorrelationContext.Correlate(e.CloudEvent, e.ContextAttributes);
            this.StatusPatch.Replace(w => w.Status.CorrelationContext, this.Status.CorrelationContext);
            this.StatusPatch.ApplyTo(this);
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowInstanceFaultedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowInstanceFaultedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowInstanceFaultedDomainEvent e)
        {
            this.Status.Errors = e.Errors.ToList();
            this.StatusPatch.Replace(w => w.Status.Type, V1WorkflowActivityStatus.Faulted);
            this.StatusPatch.Replace(w => w.Status.ExecutedAt, e.CreatedAt);
            this.StatusPatch.ApplyTo(this);
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowInstanceExecutedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowInstanceExecutedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowInstanceExecutedDomainEvent e)
        {
            this.StatusPatch.Replace(w => w.Status.Type, V1WorkflowActivityStatus.Executed);
            this.StatusPatch.Replace(w => w.Status.ExecutedAt, e.CreatedAt);
            this.StatusPatch.Replace(w => w.Status.Output, e.Output);
            this.StatusPatch.ApplyTo(this);
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowInstanceTimedOutDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowInstanceTimedOutDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowInstanceTimedOutDomainEvent e)
        {
            this.StatusPatch.Replace(w => w.Status.Type, V1WorkflowActivityStatus.TimedOut);
            this.StatusPatch.Replace(w => w.Status.ExecutedAt, e.CreatedAt);
            this.StatusPatch.ApplyTo(this);
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowInstanceTerminatedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowInstanceTerminatedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowInstanceTerminatedDomainEvent e)
        {
            this.StatusPatch.Replace(w => w.Status.Type, V1WorkflowActivityStatus.Terminated);
            this.StatusPatch.Replace(w => w.Status.ExecutedAt, e.CreatedAt);
            this.StatusPatch.ApplyTo(this);
        }

    }

}
