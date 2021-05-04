using k8s.Models;
using Microsoft.AspNetCore.JsonPatch;
using Neuroglia.K8s;
using Newtonsoft.Json;
using ReflectionMagic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Domain
{

    /// <summary>
    /// Represents the base class for all <see cref="IAggregateRoot"/> <see cref="ICustomResource"/>s
    /// </summary>
    /// <typeparam name="TSpec"></typeparam>
    /// <typeparam name="TStatus"></typeparam>
    public abstract class CustomResourceAggregate<TSpec, TStatus>
        : CustomResource<TSpec, TStatus>, IAggregateRoot
    {

        /// <summary>
        /// Initializes a new <see cref="CustomResourceAggregate{TSpec, TStatus}"/>
        /// </summary>
        /// <param name="definition">The <see cref="CustomResourceAggregate{TSpec, TStatus}"/>'s <see cref="ICustomResourceDefinition"/></param>
        protected CustomResourceAggregate(ICustomResourceDefinition definition)
            : base(definition)
        {

        }

        /// <summary>
        /// Gets the <see cref="CustomResourceAggregate{TSpec, TStatus}"/>'s id
        /// </summary>
        [JsonIgnore]
        public string Id
        {
            get
            {
                return this.Name();
            }
        }

        /// <summary>
        /// Gets the <see cref="CustomResourceAggregate{TSpec, TStatus}"/>'s version
        /// </summary>
        [JsonIgnore]
        public string Version
        {
            get
            {
                return this.Metadata.ResourceVersion;
            }
        }

        private readonly List<IDomainEvent> _PendingEvents = new();
        /// <summary>
        /// Gets the <see cref="CustomResourceAggregate{TSpec, TStatus}"/>'s pending <see cref="IDomainEvent"/>s
        /// </summary>
        [JsonIgnore]
        public IReadOnlyCollection<IDomainEvent> PendingEvents
        {
            get
            {
                return this._PendingEvents.AsReadOnly();
            }
        }

        private IJsonPatchDocument _Patch;
        /// <summary>
        /// Gets the <see cref="CustomResourceAggregate{TSpec, TStatus}"/>'s pending <see cref="IJsonPatchDocument"/>
        /// </summary>
        [JsonIgnore]
        protected IJsonPatchDocument Patch
        {
            get 
            {
                if (this._Patch == null)
                    this._Patch = this.CreatePatch();
                return this._Patch;
            }
        }

        private IJsonPatchDocument _StatusPatch;
        /// <summary>
        /// Gets the <see cref="CustomResourceAggregate{TSpec, TStatus}"/>'s pending status <see cref="IJsonPatchDocument"/>
        /// </summary>
        [JsonIgnore]
        protected IJsonPatchDocument StatusPatch
        {
            get
            {
                if (this._StatusPatch == null)
                    this._StatusPatch = this.CreatePatch();
                return this._StatusPatch;
            }
        }

        /// <summary>
        /// Gets a boolean indicating whether or not the <see cref="CustomResourceAggregate{TSpec, TStatus}"/> has a pending <see cref="IJsonPatchDocument"/>
        /// </summary>
        [JsonIgnore]
        public bool HasPatch
        {
            get
            {
                return this._Patch != null && this._Patch.GetOperations().Any();
            }
        }

        /// <summary>
        /// Gets a boolean indicating whether or not the <see cref="CustomResourceAggregate{TSpec, TStatus}"/> has a pending status <see cref="IJsonPatchDocument"/>
        /// </summary>
        [JsonIgnore]
        public bool HasStatusPatch
        {
            get
            {
                return this._StatusPatch != null && this._StatusPatch.GetOperations().Any();

            }
        }

        /// <summary>
        /// Gets the <see cref="CustomResourceAggregate{TSpec, TStatus}"/>'s pending <see cref="IJsonPatchDocument"/>
        /// </summary>
        /// <returns>The pending <see cref="IJsonPatchDocument"/>, if any</returns>
        public IJsonPatchDocument GetPatch()
        {
            return this._Patch;
        }

        /// <summary>
        /// Gets the <see cref="CustomResourceAggregate{TSpec, TStatus}"/>'s pending status <see cref="IJsonPatchDocument"/>
        /// </summary>
        /// <returns>The pending status <see cref="IJsonPatchDocument"/>, if any</returns>
        public IJsonPatchDocument GetStatusPatch()
        {
            return this._StatusPatch;
        }

        /// <summary>
        /// Registers the specified pending <see cref="IDomainEvent"/>
        /// </summary>
        /// <typeparam name="TEvent">The type of <see cref="IDomainEvent"/> to register</typeparam>
        /// <param name="e">The <see cref="IDomainEvent"/> to register</param>
        /// <returns>The registered pending <see cref="IDomainEvent"/></returns>
        protected virtual TEvent RegisterEvent<TEvent>(TEvent e)
            where TEvent : class, IDomainEvent
        {
            this._PendingEvents.Add(e);
            return e;
        }

        /// <summary>
        /// Clears pending <see cref="IDomainEvent"/>s
        /// </summary>
        public virtual void ClearPendingEvents()
        {
            this._PendingEvents.Clear();
        }

        /// <summary>
        /// Replays the specified <see cref="IDomainEvent"/>s
        /// </summary>
        /// <param name="events">An array containing the <see cref="IDomainEvent"/>s to replay</param>
        public virtual void ReplayEvents(params IDomainEvent[] events)
        {
            dynamic self = this.AsDynamic();
            foreach (IDomainEvent e in events)
            {
                self.On(e);
            }
        }

        /// <summary>
        /// Creates a new <see cref="IJsonPatchDocument"/>
        /// </summary>
        /// <returns>A new <see cref="IJsonPatchDocument"/></returns>
        protected virtual IJsonPatchDocument CreatePatch()
        {
            Type patchType = typeof(JsonPatchDocument<>).MakeGenericType(this.GetType());
            IJsonPatchDocument patch = (IJsonPatchDocument)Activator.CreateInstance(patchType);
            return patch;
        }

    }

}
