using CloudNative.CloudEvents;
using k8s.Models;
using Microsoft.AspNetCore.JsonPatch;
using Synapse.Domain.Events.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents an instance of a <see cref="V1TriggerDefinition"/>
    /// </summary>
    public class V1Trigger
        : CustomResourceAggregate<V1TriggerSpec, V1TriggerStatus>
    {

        /// <summary>
        /// Initializes a new <see cref="V1Trigger"/>
        /// </summary>
        public V1Trigger() 
            : base(new V1TriggerDefinition())
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1Trigger"/>
        /// </summary>
        /// <param name="spec">The <see cref="V1Trigger"/>'s <see cref="V1TriggerSpec"/></param>
        public V1Trigger(V1TriggerSpec spec)
            : this()
        {
            this.Spec = spec;
        }

        /// <inheritdoc/>
        protected new JsonPatchDocument<V1Trigger> Patch
        {
            get
            {
                return (JsonPatchDocument<V1Trigger>)base.Patch;
            }
        }

        /// <inheritdoc/>
        protected new JsonPatchDocument<V1Trigger> StatusPatch
        {
            get
            {
                return (JsonPatchDocument<V1Trigger>)base.StatusPatch;
            }
        }

        /// <summary>
        /// Initializes the <see cref="V1Trigger"/>
        /// </summary>
        public virtual void Initialize()
        {
            this.StatusPatch.Replace(t => t.Status, new V1TriggerStatus());
            this.StatusPatch.ApplyTo(this);
        }

        /// <summary>
        /// Determines whether or not the specified <see cref="CloudEvent"/> matches one of the <see cref="V1Trigger"/>'s conditions
        /// </summary>
        /// <param name="e">The <see cref="CloudEvent"/> to check</param>
        /// <returns>A boolean indicating whether or not the specified <see cref="CloudEvent"/> matches one of the <see cref="V1Trigger"/>'s conditions</returns>
        public virtual bool TriggeredBy(CloudEvent e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            return this.GetMatchingConditionFor(e) != null;
        }

        /// <summary>
        /// Attempts to fire the <see cref="V1Trigger"/> in the specified <see cref="V1CorrelationContext"/>
        /// </summary>
        /// <param name="context">The <see cref="V1CorrelationContext"/> to attempt firing the <see cref="V1Trigger"/> in</param>
        /// <returns>A boolean indicating whether or not the <see cref="V1Trigger"/> fired in the specified <see cref="V1CorrelationContext"/></returns>
        public virtual bool TryFireIn(V1CorrelationContext context)
        {
            if (context == null)
                throw DomainException.ArgumentNull(nameof(context));
            bool fires = false;
            fires = this.Spec.ConditionType switch
            {
                V1TriggerConditionType.AnyOf => this.Spec.Conditions.Any(c => c.Matches(context)),
                V1TriggerConditionType.AllOf => this.Spec.Conditions.All(c => c.Matches(context)),
                _ => throw new NotSupportedException($"The specified {nameof(V1TriggerConditionType)} '{this.Spec.ConditionType}' is not supported"),
            };
            if (!fires)
                return false;
            //TODO
            return true;
        }

        /// <summary>
        /// Gets the first matching <see cref="V1TriggerCondition"/> for the specified <see cref="CloudEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="CloudEvent"/> to get the <see cref="V1TriggerCondition"/> for</param>
        /// <returns>The first matching <see cref="V1TriggerCondition"/> for the specified <see cref="CloudEvent"/>, if any</returns>
        public virtual V1TriggerCondition GetMatchingConditionFor(CloudEvent e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            return this.Spec.Conditions.FirstOrDefault(c => c.Matches(e));
        }

        /// <summary>
        /// Adds a new <see cref="V1CorrelationContext"/> to the <see cref="V1Trigger"/>
        /// </summary>
        /// <param name="correlationContext">The <see cref="V1CorrelationContext"/> to add</param>
        public virtual void AddCorrelationContext(V1CorrelationContext correlationContext)
        {
            if (correlationContext == null)
                throw DomainException.ArgumentNull(nameof(correlationContext));
            this.StatusPatch.Add(t => t.Status.CorrelationContexts, correlationContext);
            this.StatusPatch.ApplyTo(this);
        }

        /// <summary>
        /// Creates a new <see cref="V1CorrelationContext"/> for the specified <see cref="CloudEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="CloudEvent"/> to create a new <see cref="V1CorrelationContext"/> for</param>
        /// <param name="contextAttributes">An <see cref="IEnumerable{T}"/> containing the keys of the context attributes to use during correlation</param>
        /// <param name="formatter">The service used to encode the specified <see cref="CloudEvent"/></param>
        /// <returns>A new <see cref="V1CorrelationContext"/></returns>
        public virtual V1CorrelationContext CreateCorrelationContextFor(CloudEvent e, IEnumerable<string> contextAttributes, ICloudEventFormatter formatter)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));
            V1CorrelationContext correlationContext = V1CorrelationContext.CreateFor(e, contextAttributes, formatter);
            this.On(new V1TriggerCorrelationContextCreatedDomainEvent(this.Id, correlationContext));
            return correlationContext;
        }

        /// <summary>
        /// Correlates the specified <see cref="CloudEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="CloudEvent"/> to correlate</param>
        /// <param name="contextAttributes">An <see cref="IEnumerable{T}"/> containing the context attributes used to correlate the specified <see cref="CloudEvent"/></param>
        /// <param name="formatter">The <see cref="ICloudEventFormatter"/> used to encode the specified <see cref="CloudEvent"/></param>
        public virtual void Correlate(CloudEvent e, IEnumerable<string> contextAttributes, ICloudEventFormatter formatter)
        {
            if (e == null)
                throw DomainException.ArgumentNull(nameof(e));
            if (contextAttributes == null)
                throw DomainException.ArgumentNull(nameof(contextAttributes));
            if (formatter == null)
                throw DomainException.ArgumentNull(nameof(formatter));
            V1CorrelationContext context = this.Status.CorrelationContexts.FirstOrDefault(c => c.CorrelatesTo(e));
            if (context == null)
                throw new NullReferenceException($"Failed to find a context that correlates to the cloud event with id '{e.Id}' in trigger '{this.Name()}'");
            int contextIndex = this.Status.CorrelationContexts.IndexOf(context);
            context.CorrelateBoostrapEvent(e, contextAttributes, formatter);
            this.StatusPatch.Replace(t => t.Status.CorrelationContexts, context, contextIndex);
        }

        /// <summary>
        /// Releases the specified <see cref="V1CorrelationContext"/>
        /// </summary>
        /// <param name="context">The <see cref="V1CorrelationContext"/> to release</param>
        public virtual void ReleaseContext(V1CorrelationContext context)
        {
            if (context == null)
                throw DomainException.ArgumentNull(nameof(context));
            if (!this.Status.CorrelationContexts.Contains(context))
                throw new DomainException($"The specified correlation context does not belong to the trigger '{this.Name()}'");
            this.On(new V1TriggerCorrelationContextReleasedDomainEvent(this.Id, context.Id));
        }

        /// <summary>
        /// Handles the specified <see cref="V1TriggerCorrelationContextCreatedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1TriggerCorrelationContextCreatedDomainEvent"/> to handle</param>
        protected virtual void On(V1TriggerCorrelationContextCreatedDomainEvent e)
        {
            this.StatusPatch.Add(t => t.Status.CorrelationContexts, e.CorrelationContext);
            this.StatusPatch.ApplyTo(this);
        }

        /// <summary>
        /// Handles the specified <see cref="V1TriggerCorrelationContextReleasedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1TriggerCorrelationContextReleasedDomainEvent"/> to handle</param>
        protected virtual void On(V1TriggerCorrelationContextReleasedDomainEvent e)
        {
            V1CorrelationContext context = this.Status.CorrelationContexts.FirstOrDefault(c => c.Id == e.CorrelationContextId);
            if (context == null)
                return;
            int index = this.Status.CorrelationContexts.IndexOf(context);
            this.StatusPatch.Remove(t => t.Status.CorrelationContexts, index);
            this.StatusPatch.ApplyTo(this);
        }

    }

}
