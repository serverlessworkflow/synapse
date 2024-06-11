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

namespace Synapse.Api.Client.Services;

/// <summary>
/// Represents the HTTP implementation of the <see cref="IResourceApiClient{TResource}"/> interface
/// </summary>
/// <param name="logger">The service used to perform logging</param>
/// <param name="jsonSerializer">The service used to serialize/deserialize objects to/from JSON</param>
/// <param name="httpClient">The service used to perform HTTP requests</param>
public class ResourceHttpApiClient<TResource>(ILogger<ResourceHttpApiClient<TResource>> logger, IJsonSerializer jsonSerializer, HttpClient httpClient)
    : IClusterResourceApiClient<TResource>, INamespacedResourceApiClient<TResource>
    where TResource : class, IResource, new()
{

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger Logger { get; } = logger;

    /// <summary>
    /// Gets the service used to serialize/deserialize objects to/from JSON
    /// </summary>
    protected IJsonSerializer JsonSerializer { get; } = jsonSerializer;

    /// <summary>
    /// Gets the service used to perform HTTP requests
    /// </summary>
    protected HttpClient HttpClient { get; } = httpClient;

    /// <inheritdoc/>
    public virtual async Task<TResource> CreateAsync(TResource resource, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(resource);
        var json = this.JsonSerializer.SerializeToText(resource);
        using var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
        var uri = $"/api/{resource.Definition.Version}/{resource.Definition.Plural}";
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Post, uri) { Content = content }, cancellationToken).ConfigureAwait(false);
        using var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return this.JsonSerializer.Deserialize<TResource>(json)!;
    }

    /// <inheritdoc/>
    public virtual async Task<TResource> GetAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var resource = new TResource();
        var uri = $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/{name}";
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Get, uri), cancellationToken).ConfigureAwait(false);
        using var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return this.JsonSerializer.Deserialize<TResource>(json)!;
    }

    /// <inheritdoc/>
    public virtual async Task<TResource> GetAsync(string name, string @namespace, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(@namespace);
        var resource = new TResource();
        var uri = $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/{@namespace}/{name}";
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Get, uri), cancellationToken).ConfigureAwait(false);
        using var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return this.JsonSerializer.Deserialize<TResource>(json)!;
    }

    /// <inheritdoc/>
    public virtual async Task<ResourceDefinition> GetDefinitionAsync(CancellationToken cancellationToken = default)
    {
        var resource = new TResource();
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Get, $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/definition"), cancellationToken).ConfigureAwait(false);
        using var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return this.JsonSerializer.Deserialize<ResourceDefinition>(json)!;
    }

    /// <inheritdoc/>
    public virtual async Task<IAsyncEnumerable<TResource>> ListAsync(string? @namespace = null, IEnumerable<LabelSelector>? labelSelectors = null, CancellationToken cancellationToken = default)
    {
        var resource = new TResource();
        var uri = string.IsNullOrWhiteSpace(@namespace) ? $"/api/{resource.Definition.Version}/{resource.Definition.Plural}" : $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/{@namespace}";
        var queryStringArguments = new Dictionary<string, string>();
        if (labelSelectors?.Any() == true) queryStringArguments.Add(nameof(labelSelectors), labelSelectors.Select(s => s.ToString()).Join(','));
        if (queryStringArguments.Count != 0) uri += $"?{queryStringArguments.Select(kvp => $"{kvp.Key}={kvp.Value}").Join('&')}";
        var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Get, uri), cancellationToken).ConfigureAwait(false);
        var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return this.JsonSerializer.DeserializeAsyncEnumerable<TResource>(responseStream, cancellationToken: cancellationToken)!;
    }

    /// <inheritdoc/>
    public virtual async Task<IAsyncEnumerable<TResource>> ListAsync(IEnumerable<LabelSelector>? labelSelectors = null, CancellationToken cancellationToken = default)
    {
        var resource = new TResource();
        var uri = $"/api/{resource.Definition.Version}/{resource.Definition.Plural}";
        var queryStringArguments = new Dictionary<string, string>();
        if (labelSelectors?.Any() == true) queryStringArguments.Add(nameof(labelSelectors), labelSelectors.Select(s => s.ToString()).Join(','));
        if (queryStringArguments.Count != 0) uri += $"?{queryStringArguments.Select(kvp => $"{kvp.Key}={kvp.Value}").Join('&')}";
        var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Get, uri), cancellationToken).ConfigureAwait(false);
        var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return this.JsonSerializer.DeserializeAsyncEnumerable<TResource>(responseStream, cancellationToken: cancellationToken)!;
    }

    /// <inheritdoc/>
    public virtual async Task<IAsyncEnumerable<IResourceWatchEvent<TResource>>> WatchAsync(string? @namespace = null, IEnumerable<LabelSelector>? labelSelectors = null, CancellationToken cancellationToken = default)
    {
        var resource = new TResource();
        var uri = string.IsNullOrWhiteSpace(@namespace) ? $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/watch" : $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/{@namespace}/watch";
        var queryStringArguments = new Dictionary<string, string>();
        if (labelSelectors?.Any() == true) queryStringArguments.Add(nameof(labelSelectors), labelSelectors.Select(s => s.ToString()).Join(','));
        if (queryStringArguments.Count != 0) uri += $"?{queryStringArguments.Select(kvp => $"{kvp.Key}={kvp.Value}").Join('&')}";
        var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Get, uri), cancellationToken).ConfigureAwait(false);
        var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return this.JsonSerializer.DeserializeAsyncEnumerable<ResourceWatchEvent<TResource>>(responseStream, cancellationToken: cancellationToken)!;
    }

    /// <inheritdoc/>
    public virtual async Task<IAsyncEnumerable<IResourceWatchEvent<TResource>>> WatchAsync(IEnumerable<LabelSelector>? labelSelectors = null, CancellationToken cancellationToken = default)
    {
        var resource = new TResource();
        var uri = $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/watch";
        var queryStringArguments = new Dictionary<string, string>();
        if (labelSelectors?.Any() == true) queryStringArguments.Add(nameof(labelSelectors), labelSelectors.Select(s => s.ToString()).Join(','));
        if (queryStringArguments.Count != 0) uri += $"?{queryStringArguments.Select(kvp => $"{kvp.Key}={kvp.Value}").Join('&')}";
        var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Get, uri), cancellationToken).ConfigureAwait(false);
        var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return this.JsonSerializer.DeserializeAsyncEnumerable<ResourceWatchEvent<TResource>>(responseStream, cancellationToken: cancellationToken)!;
    }

    /// <inheritdoc/>
    public virtual async Task<IAsyncEnumerable<IResourceWatchEvent<TResource>>> MonitorAsync(string name, string @namespace, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(@namespace);
        var resource = new TResource();
        var uri = $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/{@namespace}/{name}/monitor";
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Get, uri), cancellationToken).ConfigureAwait(false);
        using var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return this.JsonSerializer.DeserializeAsyncEnumerable<ResourceWatchEvent<TResource>>(responseStream, cancellationToken: cancellationToken)!;

    }

    /// <inheritdoc/>
    public virtual async Task<IAsyncEnumerable<IResourceWatchEvent<TResource>>> MonitorAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var resource = new TResource();
        var uri = $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/{name}/monitor";
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Get, uri), cancellationToken).ConfigureAwait(false);
        using var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return this.JsonSerializer.DeserializeAsyncEnumerable<ResourceWatchEvent<TResource>>(responseStream, cancellationToken: cancellationToken)!;
    }

    /// <inheritdoc/>
    public virtual async Task<TResource> PatchAsync(string name, Patch patch, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(patch);
        var resource = new TResource();
        var uri = $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/{name}";
        var json = this.JsonSerializer.SerializeToText(patch);
        using var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Patch, uri) { Content = content }, cancellationToken).ConfigureAwait(false);
        using var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return this.JsonSerializer.Deserialize<TResource>(json)!;
    }

    /// <inheritdoc/>
    public virtual async Task<TResource> PatchAsync(string name, string @namespace, Patch patch, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(@namespace);
        ArgumentNullException.ThrowIfNull(patch);
        var resource = new TResource();
        var uri = $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/{@namespace}/{name}";
        var json = this.JsonSerializer.SerializeToText(patch);
        using var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Patch, uri) { Content = content }, cancellationToken).ConfigureAwait(false);
        using var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return this.JsonSerializer.Deserialize<TResource>(json)!;
    }

    /// <inheritdoc/>
    public virtual async Task<TResource> PatchStatusAsync(string name, Patch patch, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(patch);
        var resource = new TResource();
        var uri = $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/{name}/status";
        var json = this.JsonSerializer.SerializeToText(patch);
        using var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Patch, uri) { Content = content }, cancellationToken).ConfigureAwait(false);
        using var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return this.JsonSerializer.Deserialize<TResource>(json)!;
    }

    /// <inheritdoc/>
    public virtual async Task<TResource> PatchStatusAsync(string name, string @namespace, Patch patch, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(@namespace);
        ArgumentNullException.ThrowIfNull(patch);
        var resource = new TResource();
        var uri = $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/{@namespace}/{name}/status";
        var json = this.JsonSerializer.SerializeToText(patch);
        using var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Patch, uri) { Content = content }, cancellationToken).ConfigureAwait(false);
        using var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return this.JsonSerializer.Deserialize<TResource>(json)!;
    }

    /// <inheritdoc/>
    public virtual async Task<TResource> ReplaceAsync(TResource resource, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(resource);
        var uri = $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/{resource.GetNamespace()!}/{resource.GetName()}";
        var json = this.JsonSerializer.SerializeToText(resource);
        using var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Put, uri) { Content = content }, cancellationToken).ConfigureAwait(false);
        using var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return this.JsonSerializer.Deserialize<TResource>(json)!;
    }

    /// <inheritdoc/>
    public virtual async Task<TResource> ReplaceStatusAsync(TResource resource, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(resource);
        var uri = $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/{resource.GetNamespace()!}/{resource.GetName()}/status";
        var json = this.JsonSerializer.SerializeToText(resource);
        using var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Put, uri) { Content = content }, cancellationToken).ConfigureAwait(false);
        using var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return this.JsonSerializer.Deserialize<TResource>(json)!;
    }

    /// <inheritdoc/>
    public virtual async Task DeleteAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var resource = new TResource();
        var uri = $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/{name}";
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Delete, uri), cancellationToken).ConfigureAwait(false);
        await ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task DeleteAsync(string name, string @namespace, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(@namespace);
        var resource = new TResource();
        var uri = $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/{@namespace}/{name}";
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Delete, uri), cancellationToken).ConfigureAwait(false);
        await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Processes the specified <see cref="HttpRequestMessage"/> before sending it
    /// </summary>
    /// <param name="request">the <see cref="HttpRequestMessage"/> to process</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The processed <see cref="HttpRequestMessage"/></returns>
    protected virtual Task<HttpRequestMessage> ProcessRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult(request);
    }

    /// <summary>
    /// Processes the specified <see cref="HttpResponseMessage"/>
    /// </summary>
    /// <param name="response">the <see cref="HttpResponseMessage"/> to process</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The processed <see cref="HttpResponseMessage"/></returns>
    protected virtual async Task<HttpResponseMessage> ProcessResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);
        if (response.IsSuccessStatusCode) return response;
        var content = string.Empty;
        if (response.Content != null) content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        this.Logger.LogError("The remote server responded with a non-success status code '{statusCode}': {errorDetails}", response.StatusCode, content);
        if (!response.IsSuccessStatusCode)
        {
            if (string.IsNullOrWhiteSpace(content)) response.EnsureSuccessStatusCode();
            else throw new ProblemDetailsException(this.JsonSerializer.Deserialize<ProblemDetails>(content)!);
        }
        return response;
    }

}
