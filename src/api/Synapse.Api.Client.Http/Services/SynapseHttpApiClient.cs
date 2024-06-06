// Copyright © 2024-Present Neuroglia SRL. All rights reserved.
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

namespace Synapse.Api.Client.Services;

/// <summary>
/// Represents the default HTTP implementation of the <see cref="ISynapseApiClient"/> interface
/// </summary>
public class SynapseHttpApiClient
    : ISynapseApiClient
{

    /// <summary>
    /// Initializes a new <see cref="SynapseHttpApiClient"/>
    /// </summary>
    /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
    /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
    /// <param name="serializer">The service used to serialize/deserialize objects to/from JSON</param>
    /// <param name="httpClient">The service used to perform http requests</param>
    public SynapseHttpApiClient(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IJsonSerializer serializer, HttpClient httpClient)
    {
        this.ServiceProvider = serviceProvider;
        this.Logger = loggerFactory.CreateLogger(GetType());
        this.Serializer = serializer;
        this.HttpClient = httpClient;
        this.WorkflowData = ActivatorUtilities.CreateInstance<DocumentHttpApiClient>(this.ServiceProvider, this.HttpClient);
        foreach (var apiProperty in GetType().GetProperties().Where(p => p.CanRead && p.PropertyType.GetGenericType(typeof(IResourceApiClient<>)) != null))
        {
            var apiType = apiProperty.PropertyType.GetGenericType(typeof(IResourceApiClient<>))!;
            var resourceType = apiType.GetGenericArguments()[0];
            var api = ActivatorUtilities.CreateInstance(this.ServiceProvider, typeof(ResourceHttpApiClient<>).MakeGenericType(resourceType), this.HttpClient);
            apiProperty.SetValue(this, api);
        }
    }

    /// <summary>
    /// Gets the current <see cref="IServiceProvider"/>
    /// </summary>
    protected IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Gets the service used to serialize/deserialize objects to/from JSON
    /// </summary>
    protected IJsonSerializer Serializer { get; }

    /// <summary>
    /// Gets the service used to perform http requests
    /// </summary>
    protected HttpClient HttpClient { get; }

    /// <inheritdoc/>
    public INamespacedResourceApiClient<Correlation> Correlations { get; private set; } = null!;

    /// <inheritdoc/>
    public INamespacedResourceApiClient<Correlator> Correlators { get; private set; } = null!;

    /// <inheritdoc/>
    public IClusterResourceApiClient<Namespace> Namespaces { get; private set; } = null!;

    /// <inheritdoc/>
    public INamespacedResourceApiClient<Operator> Operators { get; private set; } = null!;

    /// <inheritdoc/>
    public IDocumentApiClient WorkflowData { get; }

    /// <inheritdoc/>
    public INamespacedResourceApiClient<Workflow> Workflows { get; private set; } = null!;

    /// <inheritdoc/>
    public INamespacedResourceApiClient<WorkflowInstance> WorkflowInstances { get; private set; } = null!;

}