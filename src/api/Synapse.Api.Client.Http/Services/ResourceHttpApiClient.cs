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

using System.Runtime.CompilerServices;

namespace Synapse.Api.Client.Services;

/// <summary>
/// Represents the HTTP implementation of the <see cref="IResourceApiClient{TResource}"/> interface
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="options">The service used to access the current <see cref="SynapseHttpApiClientOptions"/></param>
/// <param name="jsonSerializer">The service used to serialize/deserialize objects to/from JSON</param>
/// <param name="httpClient">The service used to perform HTTP requests</param>
public class ResourceHttpApiClient<TResource>(IServiceProvider serviceProvider, ILogger<ResourceHttpApiClient<TResource>> logger, IOptions<SynapseHttpApiClientOptions> options, IJsonSerializer jsonSerializer, HttpClient httpClient)
    : ApiClientBase(serviceProvider, logger, jsonSerializer, options, httpClient), IClusterResourceApiClient<TResource>, INamespacedResourceApiClient<TResource>
    where TResource : class, IResource, new()
{

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
    public virtual async Task<IAsyncEnumerable<TResource>> GetAllAsync(string? @namespace = null, IEnumerable<LabelSelector>? labelSelectors = null, CancellationToken cancellationToken = default)
    {
        var resource = new TResource();
        var uri = string.IsNullOrWhiteSpace(@namespace) ? $"/api/{resource.Definition.Version}/{resource.Definition.Plural}" : $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/{@namespace}";
        var queryStringArguments = new Dictionary<string, string>();
        if (labelSelectors?.Any() == true) queryStringArguments.Add("labelSelector", labelSelectors.Select(s => s.ToString()).Join(','));
        if (queryStringArguments.Count != 0) uri += $"?{queryStringArguments.Select(kvp => $"{kvp.Key}={kvp.Value}").Join('&')}";
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Get, uri), cancellationToken).ConfigureAwait(false);
        request.EnableWebAssemblyStreamingResponse();
        var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return this.JsonSerializer.DeserializeAsyncEnumerable<TResource>(responseStream, cancellationToken: cancellationToken)!;
    }

    /// <inheritdoc/>
    public virtual async Task<IAsyncEnumerable<TResource>> GetAllAsync(IEnumerable<LabelSelector>? labelSelectors = null, CancellationToken cancellationToken = default)
    {
        var resource = new TResource();
        var uri = $"/api/{resource.Definition.Version}/{resource.Definition.Plural}";
        var queryStringArguments = new Dictionary<string, string>();
        if (labelSelectors?.Any() == true) queryStringArguments.Add("labelSelector", labelSelectors.Select(s => s.ToString()).Join(','));
        if (queryStringArguments.Count != 0) uri += $"?{queryStringArguments.Select(kvp => $"{kvp.Key}={kvp.Value}").Join('&')}";
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Get, uri), cancellationToken).ConfigureAwait(false);
        request.EnableWebAssemblyStreamingResponse();
        var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return this.JsonSerializer.DeserializeAsyncEnumerable<TResource>(responseStream, cancellationToken: cancellationToken)!;
    }

    /// <inheritdoc/>
    public virtual async Task<Neuroglia.Data.Infrastructure.ResourceOriented.ICollection<TResource>> ListAsync(string? @namespace = null, IEnumerable<LabelSelector>? labelSelectors = null, ulong? maxResults = null, string? continuationToken = null, CancellationToken cancellationToken = default)
    {
        var resource = new TResource();
        var uri = string.IsNullOrWhiteSpace(@namespace) ? $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/list" : $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/{@namespace}/list";
        var queryStringArguments = new Dictionary<string, string>();
        if (labelSelectors?.Any() == true) queryStringArguments.Add("labelSelector", labelSelectors.Select(s => s.ToString()).Join(','));
        if (maxResults.HasValue) queryStringArguments.Add("maxResults", maxResults.Value.ToString());
        if (!string.IsNullOrWhiteSpace(continuationToken)) queryStringArguments.Add("continuationToken", continuationToken);
        if (queryStringArguments.Count != 0) uri += $"?{queryStringArguments.Select(kvp => $"{kvp.Key}={kvp.Value}").Join('&')}";
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Get, uri), cancellationToken).ConfigureAwait(false);
        request.EnableWebAssemblyStreamingResponse();
        var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return this.JsonSerializer.Deserialize<Collection<TResource>>(responseStream)!;
    }

    /// <inheritdoc/>
    public virtual async Task<Neuroglia.Data.Infrastructure.ResourceOriented.ICollection<TResource>> ListWithContinuationAsync(IEnumerable<LabelSelector>? labelSelectors = null, ulong? maxResults = null, string? continuationToken = null, CancellationToken cancellationToken = default)
    {
        var resource = new TResource();
        var uri = $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/list";
        var queryStringArguments = new Dictionary<string, string>();
        if (labelSelectors?.Any() == true) queryStringArguments.Add("labelSelector", labelSelectors.Select(s => s.ToString()).Join(','));
        if (maxResults.HasValue) queryStringArguments.Add("maxResults", maxResults.Value.ToString());
        if (!string.IsNullOrWhiteSpace(continuationToken)) queryStringArguments.Add("continuationToken", continuationToken);
        if (queryStringArguments.Count != 0) uri += $"?{queryStringArguments.Select(kvp => $"{kvp.Key}={kvp.Value}").Join('&')}";
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Get, uri), cancellationToken).ConfigureAwait(false);
        request.EnableWebAssemblyStreamingResponse();
        var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return this.JsonSerializer.Deserialize<Neuroglia.Data.Infrastructure.ResourceOriented.ICollection<TResource>>(responseStream)!;
    }

    /// <inheritdoc/>
    public virtual async IAsyncEnumerable<IResourceWatchEvent<TResource>> WatchAsync(string? @namespace = null, IEnumerable<LabelSelector>? labelSelectors = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var resource = new TResource();
        var uri = string.IsNullOrWhiteSpace(@namespace) ? $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/watch/sse" : $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/{@namespace}/watch";
        var queryStringArguments = new Dictionary<string, string>();
        if (labelSelectors?.Any() == true) queryStringArguments.Add("labelSelector", labelSelectors.Select(s => s.ToString()).Join(','));
        if (queryStringArguments.Count != 0) uri += $"?{queryStringArguments.Select(kvp => $"{kvp.Key}={kvp.Value}").Join('&')}";
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Get, uri), cancellationToken).ConfigureAwait(false);
        var response = await this.HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var streamReader = new StreamReader(await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false));
        while (!streamReader.EndOfStream)
        {
            var sseMessage = await streamReader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(sseMessage)) continue;
            var json = sseMessage["data: ".Length..].Trim();
            var e = JsonSerializer.Deserialize<ResourceWatchEvent<TResource>>(json)!;
            yield return e;
        }
    }

    /// <inheritdoc/>
    public virtual async IAsyncEnumerable<IResourceWatchEvent<TResource>> WatchAsync(IEnumerable<LabelSelector>? labelSelectors = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var resource = new TResource();
        var uri = $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/watch/sse";
        var queryStringArguments = new Dictionary<string, string>();
        if (labelSelectors?.Any() == true) queryStringArguments.Add("labelSelector", labelSelectors.Select(s => s.ToString()).Join(','));
        if (queryStringArguments.Count != 0) uri += $"?{queryStringArguments.Select(kvp => $"{kvp.Key}={kvp.Value}").Join('&')}";
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Get, uri), cancellationToken).ConfigureAwait(false);
        request.EnableWebAssemblyStreamingResponse();
        var response = await this.HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var streamReader = new StreamReader(await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false));
        while (!streamReader.EndOfStream)
        {
            var sseMessage = await streamReader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(sseMessage)) continue;
            var json = sseMessage["data: ".Length..].Trim();
            var e = JsonSerializer.Deserialize<ResourceWatchEvent<TResource>>(json)!;
            yield return e;
        }
    }

    /// <inheritdoc/>
    public virtual async IAsyncEnumerable<IResourceWatchEvent<TResource>> MonitorAsync(string name, string @namespace, [EnumeratorCancellation]CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(@namespace);
        var resource = new TResource();
        var uri = $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/{@namespace}/{name}/monitor/sse";
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Get, uri), cancellationToken).ConfigureAwait(false);
        var response = await this.HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var streamReader = new StreamReader(await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false));
        while (!streamReader.EndOfStream)
        {
            var sseMessage = await streamReader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(sseMessage)) continue;
            var json = sseMessage["data: ".Length..].Trim();
            var e = JsonSerializer.Deserialize<ResourceWatchEvent<TResource>>(json)!;
            yield return e;
        }
    }

    /// <inheritdoc/>
    public virtual async IAsyncEnumerable<IResourceWatchEvent<TResource>> MonitorAsync(string name, [EnumeratorCancellation]CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var resource = new TResource();
        var uri = $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/{name}/monitor/sse";
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Get, uri), cancellationToken).ConfigureAwait(false);
        var response = await this.HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var streamReader = new StreamReader(await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false));
        while (!streamReader.EndOfStream)
        {
            var sseMessage = await streamReader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(sseMessage)) continue;
            var json = sseMessage["data: ".Length..].Trim();
            var e = JsonSerializer.Deserialize<ResourceWatchEvent<TResource>>(json)!;
            yield return e;
        }
    }

    /// <inheritdoc/>
    public virtual async Task<TResource> PatchAsync(string name, Patch patch, string? resourceVersion = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(patch);
        var resource = new TResource();
        var uri = $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/{name}";
        if (!string.IsNullOrWhiteSpace(resourceVersion)) uri += $"?resourceVersion={resourceVersion}";
        var json = this.JsonSerializer.SerializeToText(patch);
        using var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Patch, uri) { Content = content }, cancellationToken).ConfigureAwait(false);
        using var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return this.JsonSerializer.Deserialize<TResource>(json)!;
    }

    /// <inheritdoc/>
    public virtual async Task<TResource> PatchAsync(string name, string @namespace, Patch patch, string? resourceVersion = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(@namespace);
        ArgumentNullException.ThrowIfNull(patch);
        var resource = new TResource();
        var uri = $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/{@namespace}/{name}";
        if (!string.IsNullOrWhiteSpace(resourceVersion)) uri += $"?resourceVersion={resourceVersion}";
        var json = this.JsonSerializer.SerializeToText(patch);
        using var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Patch, uri) { Content = content }, cancellationToken).ConfigureAwait(false);
        using var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return this.JsonSerializer.Deserialize<TResource>(json)!;
    }

    /// <inheritdoc/>
    public virtual async Task<TResource> PatchStatusAsync(string name, Patch patch, string? resourceVersion = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(patch);
        var resource = new TResource();
        var uri = $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/{name}/status";
        if (!string.IsNullOrWhiteSpace(resourceVersion)) uri += $"?resourceVersion={resourceVersion}";
        var json = this.JsonSerializer.SerializeToText(patch);
        using var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Patch, uri) { Content = content }, cancellationToken).ConfigureAwait(false);
        using var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return this.JsonSerializer.Deserialize<TResource>(json)!;
    }

    /// <inheritdoc/>
    public virtual async Task<TResource> PatchStatusAsync(string name, string @namespace, Patch patch, string? resourceVersion = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(@namespace);
        ArgumentNullException.ThrowIfNull(patch);
        var resource = new TResource();
        var uri = $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/{@namespace}/{name}/status";
        if (!string.IsNullOrWhiteSpace(resourceVersion)) uri += $"?resourceVersion={resourceVersion}";
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

}
