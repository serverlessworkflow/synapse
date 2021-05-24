using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using Neuroglia.K8s;
using Newtonsoft.Json.Linq;
using Synapse.Domain;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Synapse.Services
{

    /// <summary>
    /// Represents an <see cref="IRepository{TResource}"/> implementation for Kubernetes
    /// </summary>
    /// <typeparam name="TResource">The type of <see cref="ICustomResource"/> to manage</typeparam>
    public class K8sRepository<TResource>
        : IRepository<TResource>
        where TResource : class, ICustomResource, IAggregateRoot, new()
    {

        /// <summary>
        /// Initializes a new <see cref="K8sRepository{TResource}"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="kubernetes">The service used to interact with the Kubernetes API</param>
        public K8sRepository(ILoggerFactory loggerFactory, IKubernetes kubernetes)
        {
            this.Logger = loggerFactory.CreateLogger(this.GetType());
            this.Kubernetes = kubernetes;
        }

        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the service used to interact with the Kubernetes API
        /// </summary>
        protected IKubernetes Kubernetes { get; }

        /// <summary>
        /// Gets the <see cref="ICustomResourceDefinition"/> of the <see cref="ICustomResource"/>s to manage
        /// </summary>
        protected ICustomResourceDefinition ResourceDefinition { get; } = new TResource().Definition;

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<TResource>> FilterAsync(string labelSelector = null, string fieldSelector = null, string @namespace = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(@namespace))
                @namespace = SynapseConstants.EnvironmentVariables.Kubernetes.Namespace.Value;
            KubernetesList<TResource> resources = await this.Kubernetes.ListNamespacedCustomObjectAsync<TResource>(this.ResourceDefinition.Group, this.ResourceDefinition.Version, @namespace, this.ResourceDefinition.Plural, fieldSelector: fieldSelector, labelSelector: labelSelector, cancellationToken: cancellationToken);
            return resources.Items;
        }

        /// <inheritdoc/>
        public virtual async Task<TResource> FindByNameAsync(string name, string @namespace, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            return ((JObject)await this.Kubernetes.GetNamespacedCustomObjectAsync(this.ResourceDefinition.Group, this.ResourceDefinition.Version, @namespace, this.ResourceDefinition.Plural, name, cancellationToken: cancellationToken)).ToObject<TResource>();
        }

        /// <inheritdoc/>
        public virtual async Task<TResource> FindByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            return await this.FindByNameAsync(name, SynapseConstants.EnvironmentVariables.Kubernetes.Namespace.Value, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<TResource> AddAsync(TResource resource, CancellationToken cancellationToken = default)
        {
            if (resource == null)
                throw new ArgumentNullException(nameof(resource));
            return ((JObject)await this.Kubernetes.CreateNamespacedCustomObjectAsync(resource, resource.Definition.Group, resource.Definition.Version, resource.Namespace(), resource.Definition.Plural , cancellationToken: cancellationToken)).ToObject<TResource>();
        }

        /// <inheritdoc/>
        public virtual async Task<TResource> UpdateAsync(TResource resource, CancellationToken cancellationToken = default)
        {
            if (resource == null)
                throw new ArgumentNullException(nameof(resource));
            IEnumerable<IDomainEvent> pendingEvents = resource.PendingEvents;
            bool updated = false;
            TResource updatedResource = resource;
            if (resource.HasPatch)
            {
                updatedResource = ((JObject)await this.Kubernetes.PatchNamespacedCustomObjectAsync(new V1Patch(updatedResource.GetPatch(), V1Patch.PatchType.JsonPatch), this.ResourceDefinition.Group, this.ResourceDefinition.Version, resource.Namespace(), this.ResourceDefinition.Plural, updatedResource.Name(), cancellationToken: cancellationToken)).ToObject<TResource>();
                updated = true;
            }
            if (resource.HasStatusPatch)
            {
                updatedResource = ((JObject)await this.Kubernetes.PatchNamespacedCustomObjectStatusAsync(new V1Patch(resource.GetStatusPatch(), V1Patch.PatchType.JsonPatch), this.ResourceDefinition.Group, this.ResourceDefinition.Version, updatedResource.Namespace(), this.ResourceDefinition.Plural, updatedResource.Name(), cancellationToken: cancellationToken)).ToObject<TResource>();
                updated = true;
            }
            if(!updated)
                return resource;
            V1ObjectReference resourceReference = new()
            {
                ApiVersion = this.ResourceDefinition.ApiVersion,
                Kind = this.ResourceDefinition.Kind,
                Name = resource.Name(),
                NamespaceProperty = resource.Namespace(),
                ResourceVersion = resource.Metadata.ResourceVersion,
                Uid = resource.Metadata.Uid
            };
            V1EventSource eventSource = new($"{SynapseCustomResources.Workflow.Group}/operator", SynapseConstants.EnvironmentVariables.Kubernetes.PodName.Value);
            string eventNamespace = SynapseConstants.EnvironmentVariables.Kubernetes.Namespace.Value;
            foreach (IDomainEvent e in pendingEvents)
            {
                string eventTypeName = e.GetType().Name.Replace("DomainEvent", string.Empty);
                V1ObjectMeta k8sEventMetadata = new()
                {
                    GenerateName = $"{eventTypeName}-",
                    Labels = updatedResource.Labels()
                };
                Corev1Event k8sEvent = new(resourceReference, k8sEventMetadata)
                {
                    Type = "Normal",
                    Reason = eventTypeName,
                    Action = eventTypeName,
                    ReportingComponent = $"{SynapseCustomResources.Workflow.Group}/operator",
                    ReportingInstance = SynapseConstants.EnvironmentVariables.Kubernetes.PodName.Value,
                    Source = eventSource,
                    EventTime = DateTime.Now,
                    FirstTimestamp = DateTime.UtcNow,
                    LastTimestamp = DateTime.UtcNow
                };
                await this.Kubernetes.CreateNamespacedEventAsync(k8sEvent, eventNamespace, cancellationToken: cancellationToken);
            }
            return updatedResource;
        }

        /// <inheritdoc/>
        public virtual async Task RemoveAsync(TResource resource, CancellationToken cancellationToken = default)
        {
            if (resource == null)
                throw new ArgumentNullException(nameof(resource));
            await this.Kubernetes.DeleteNamespacedCustomObjectAsync(resource.Definition.Group, resource.Definition.Version, resource.Namespace(), resource.Definition.Plural, resource.Name(), cancellationToken: cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<TResource>> ToListAsync(CancellationToken cancellationToken = default)
        {
            return await this.FilterAsync(cancellationToken: cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task ClearAsync(string @namespace, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(@namespace))
                throw new ArgumentNullException(nameof(@namespace));
            await this.Kubernetes.DeleteCollectionNamespacedCustomObjectAsync(this.ResourceDefinition.Group, this.ResourceDefinition.Version, @namespace, this.ResourceDefinition.Plural, cancellationToken: cancellationToken);
        }

    }

}
