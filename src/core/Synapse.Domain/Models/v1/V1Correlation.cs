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

using Synapse.Domain.Events.Correlations;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents an event correlation
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Models.V1Correlation))]
    public class V1Correlation
        : AggregateRoot<string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1Correlation"/>
        /// </summary>
        protected V1Correlation()
            : base(default!)
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1Correlation"/>
        /// </summary>
        /// <param name="lifetime">The <see cref="V1Correlation"/>'s lifetime</param>
        /// <param name="conditionType">A value determining the type of the <see cref="V1Correlation"/>'s <see cref="V1CorrelationCondition"/> evaluation</param>
        /// <param name="conditions">An <see cref="IReadOnlyCollection{T}"/> containing the <see cref="V1Correlation"/>'s conditions</param>
        /// <param name="outcome">The outcome of the <see cref="V1Correlation"/></param>
        /// <param name="context">The initial <see cref="V1CorrelationContext"/></param>
        public V1Correlation(V1CorrelationLifetime lifetime, V1CorrelationConditionType conditionType, IEnumerable<V1CorrelationCondition> conditions, V1CorrelationOutcome outcome, V1CorrelationContext? context = null)
            : base(Guid.NewGuid().ToString())
        {
            if(conditions == null 
                || !conditions.Any())
                throw DomainException.ArgumentNull(nameof(conditions));
            if(outcome == null)
                throw DomainException.ArgumentNull(nameof(outcome));
            this.On(this.RegisterEvent(new V1CorrelationCreatedDomainEvent(this.Id, lifetime, conditionType, conditions, outcome, context)));
        }

        /// <summary>
        /// Gets the <see cref="V1Correlation"/>'s lifetime
        /// </summary>
        public virtual V1CorrelationLifetime Lifetime { get; protected set; }

        /// <summary>
        /// Gets a value determining the type of the <see cref="V1Correlation"/>'s <see cref="V1CorrelationCondition"/> evaluation
        /// </summary>
        public virtual V1CorrelationConditionType ConditionType { get; protected set; }

        [Newtonsoft.Json.JsonProperty(nameof(Conditions))]
        [System.Text.Json.Serialization.JsonPropertyName(nameof(Conditions))]
        private List<V1CorrelationCondition> _Conditions = new();
        /// <summary>
        /// Gets an <see cref="IReadOnlyCollection{T}"/> containing the <see cref="V1Correlation"/>'s conditions
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IReadOnlyCollection<V1CorrelationCondition> Conditions => this._Conditions.AsReadOnly();

        /// <summary>
        /// Gets the outcome of the <see cref="V1Correlation"/>
        /// </summary>
        public virtual V1CorrelationOutcome Outcome { get; protected set; } = null!;

        [Newtonsoft.Json.JsonProperty(nameof(Contexts))]
        [System.Text.Json.Serialization.JsonPropertyName(nameof(Contexts))]
        private List<V1CorrelationContext> _Contexts = new();
        /// <summary>
        /// Gets an <see cref="IReadOnlyCollection{T}"/> containing the <see cref="V1CorrelationContext"/>s affected by the <see cref="V1Correlation"/>
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual IReadOnlyCollection<V1CorrelationContext> Contexts => this._Contexts.AsReadOnly();

        /// <summary>
        /// Adds a new <see cref="V1CorrelationContext"/> to the <see cref="V1Correlation"/>
        /// </summary>
        /// <param name="context">The <see cref="V1CorrelationContext"/> to add</param>
        public virtual void AddContext(V1CorrelationContext context)
        {
            if (context == null)
                throw DomainException.ArgumentNull(nameof(context));
            this.On(this.RegisterEvent(new V1ContextAddedToCorrelationDomainEvent(this.Id, context)));
        }

        /// <summary>
        /// Determines whether or not the specified <see cref="V1Event"/> matches one of the <see cref="V1Correlation"/>'s conditions
        /// </summary>
        /// <param name="e">The <see cref="V1Event"/> to check</param>
        /// <returns>A boolean indicating whether or not the specified <see cref="V1Event"/> matches one of the <see cref="V1Correlation"/>'s conditions</returns>
        public virtual bool AppliesTo(V1Event e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            return this.GetMatchingConditionFor(e) != null;
        }

        /// <summary>
        /// Gets the first matching <see cref="V1CorrelationCondition"/> for the specified <see cref="V1Event"/>
        /// </summary>
        /// <param name="e">The <see cref="V1Event"/> to get the <see cref="V1CorrelationCondition"/> for</param>
        /// <returns>The first matching <see cref="V1CorrelationCondition"/> for the specified <see cref="V1Event"/>, if any</returns>
        public virtual V1CorrelationCondition? GetMatchingConditionFor(V1Event e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            return this.Conditions.FirstOrDefault(c => c.Matches(e));
        }

        /// <summary>
        /// Correlates the specified <see cref="V1Event"/>
        /// </summary>
        /// <param name="e">The <see cref="V1Event"/> to correlate</param>
        /// <param name="mappings">An <see cref="IEnumerable{T}"/> containing the keys of the mappings used to correlate the specified <see cref="V1Event"/></param>
        public virtual void Correlate(V1Event e, IEnumerable<string> mappings)
        {
            if (e == null)
                throw DomainException.ArgumentNull(nameof(e));
            if (mappings == null)
                throw DomainException.ArgumentNull(nameof(mappings));
            switch (this.Lifetime)
            {
                case V1CorrelationLifetime.Singleton:
                    var exclusiveContext = this.Contexts.Single();
                    if (exclusiveContext == null || !exclusiveContext.CorrelatesTo(e))
                        throw new DomainNullReferenceException($"Failed to find the exclusive correlation's context");
                    this.On(this.RegisterEvent(new V1EventCorrelatedDomainEvent(this.Id, exclusiveContext.Id, e, mappings)));
                    break;
                case V1CorrelationLifetime.Transient:
                    foreach(var context in this.Contexts
                        .Where(c => c.CorrelatesTo(e)))
                    {
                        this.On(this.RegisterEvent(new V1EventCorrelatedDomainEvent(this.Id, context.Id, e, mappings)));
                    }
                    break;
                default:
                    throw new NotSupportedException($"The specified {nameof(V1CorrelationLifetime)} '{this.Lifetime}' is not supported");
            }
        }

        /// <summary>
        /// Releases the specified <see cref="V1CorrelationContext"/>
        /// </summary>
        /// <param name="context">The <see cref="V1CorrelationContext"/> to release</param>
        public virtual void ReleaseContext(V1CorrelationContext context)
        {
            if (context == null)
                throw DomainException.ArgumentNull(nameof(context));
            if (!this.Contexts.Any(c => c.Id == context.Id))
                throw DomainException.NullReference(typeof(V1CorrelationContext), context.Id);
            this.On(this.RegisterEvent(new V1CorrelationContextReleasedDomainEvent(this.Id, context.Id)));
        }

        /// <summary>
        /// Attempts to complete the <see cref="V1Correlation"/> in the specified <see cref="V1CorrelationContext"/>
        /// </summary>
        /// <param name="context">The <see cref="V1CorrelationContext"/> to attempt completing the <see cref="V1Correlation"/> in</param>
        /// <returns>A boolean indicating whether or not the <see cref="V1Correlation"/> could be completed in the specified <see cref="V1CorrelationContext"/></returns>
        public virtual bool TryComplete(V1CorrelationContext context)
        {
            if (context == null)
                throw DomainException.ArgumentNull(nameof(context));
            var isCompleted = false;
            isCompleted = this.ConditionType switch
            {
                V1CorrelationConditionType.AnyOf => this.Conditions.Any(c => c.MatchesAny(context)),
                V1CorrelationConditionType.AllOf => this.Conditions.All(c => c.MatchesAll(context)),
                _ => throw new NotSupportedException($"The specified {nameof(V1CorrelationConditionType)} '{this.ConditionType}' is not supported"),
            };
            return isCompleted;
        }

        /// <summary>
        /// Deletes the <see cref="V1Correlation"/>
        /// </summary>
        public virtual void Delete()
        {
            this.On(this.RegisterEvent(new V1CorrelationDeletedDomainEvent(this.Id)));
        }

        /// <summary>
        /// Handles the specified <see cref="V1CorrelationCreatedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1CorrelationCreatedDomainEvent"/> to handle</param>
        protected virtual void On(V1CorrelationCreatedDomainEvent e)
        {
            this.Id = e.AggregateId;
            this.CreatedAt = e.CreatedAt;
            this.LastModified = e.CreatedAt;
            this.Lifetime = e.Lifetime;
            this.ConditionType = e.ConditionType;
            this._Conditions = e.Conditions.ToList();
            this.Outcome = e.Outcome;
            if (e.Context != null)
                this._Contexts.Add(e.Context);
        }

        /// <summary>
        /// Handles the specified <see cref="V1ContextAddedToCorrelationDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1ContextAddedToCorrelationDomainEvent"/> to handle</param>
        protected virtual void On(V1ContextAddedToCorrelationDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this._Contexts.Add(e.Context);
        }

        /// <summary>
        /// Handles the specified <see cref="V1EventCorrelatedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1EventCorrelatedDomainEvent"/> to handle</param>
        protected virtual void On(V1EventCorrelatedDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            var context = this.Contexts.FirstOrDefault(c => c.Id == e.ContextId);
            if (context == null)
                throw DomainException.NullReference(typeof(V1CorrelationContext), e.ContextId);
            context.Correlate(e.Event, e.Mappings, true);
        }

        /// <summary>
        /// Handles the specified <see cref="V1CorrelationContextReleasedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1CorrelationContextReleasedDomainEvent"/> to handle</param>
        protected virtual void On(V1CorrelationContextReleasedDomainEvent e)
        {
            var context = this.Contexts.FirstOrDefault(c => c.Id == e.ContextId);
            if (context == null)
                throw DomainException.NullReference(typeof(V1CorrelationContext), e.ContextId);
            this._Contexts.Remove(context);
        }

        /// <summary>
        /// Handles the specified <see cref="V1CorrelationDeletedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1CorrelationDeletedDomainEvent"/> to handle</param>
        protected virtual void On(V1CorrelationDeletedDomainEvent e)
        {

        }

    }

}
