// Copyright © 2024-Present The Synapse Authors
//
// Licensed under the Apache License, Version 2.0 (the "License"),
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Synapse.Api.Client;

/// <summary>
/// Defines extensions for <see cref="ISynapseApiClient"/>s
/// </summary>
public static class ISynapseApiClientExtensions
{

    /// <summary>
    /// Gets the <see cref="IResourceApiClient{TResource}"/> for the specified <see cref="IResource"/> type
    /// </summary>
    /// <typeparam name="TResource">The type of <see cref="IResource"/> to get the <see cref="IResourceApiClient{TResource}"/> for</typeparam>
    /// <param name="client">The extended <see cref="ISynapseApiClient"/></param>
    /// <returns>The <see cref="IResourceApiClient{TResource}"/> for the specified <see cref="IResource"/> type</returns>
    public static INamespacedResourceApiClient<TResource> ManageNamespaced<TResource>(this ISynapseApiClient client)
        where TResource : class, IResource, new()
    {
        var apiProperty = client.GetType().GetProperties().SingleOrDefault(p => p.CanRead && typeof(INamespacedResourceApiClient<>).MakeGenericType(typeof(TResource)).IsAssignableFrom(p.PropertyType)) ?? throw new NullReferenceException($"Failed to find a management API for the specified resource type '{new TResource().Definition}'");
        return (INamespacedResourceApiClient<TResource>)apiProperty.GetValue(client)!;
    }

    /// <summary>
    /// Gets the <see cref="IResourceApiClient{TResource}"/> for the specified <see cref="IResource"/> type
    /// </summary>
    /// <typeparam name="TResource">The type of <see cref="IResource"/> to get the <see cref="IResourceApiClient{TResource}"/> for</typeparam>
    /// <param name="client">The extended <see cref="ISynapseApiClient"/></param>
    /// <returns>The <see cref="IResourceApiClient{TResource}"/> for the specified <see cref="IResource"/> type</returns>
    public static IClusterResourceApiClient<TResource> ManageCluster<TResource>(this ISynapseApiClient client)
        where TResource : class, IResource, new()
    {
        var apiProperty = client.GetType().GetProperties().SingleOrDefault(p => p.CanRead && typeof(IClusterResourceApiClient<>).MakeGenericType(typeof(TResource)).IsAssignableFrom(p.PropertyType)) ?? throw new NullReferenceException($"Failed to find a management API for the specified resource type '{new TResource().Definition}'");
        return (IClusterResourceApiClient<TResource>)apiProperty.GetValue(client)!;
    }

}