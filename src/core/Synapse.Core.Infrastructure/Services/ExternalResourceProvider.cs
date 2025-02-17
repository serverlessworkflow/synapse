﻿// Copyright © 2024-Present The Synapse Authors
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

namespace Synapse.Core.Infrastructure.Services;

/// <summary>
/// Represents the default implementation of the <see cref="IExternalResourceProvider"/> interface
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="httpClientFactory">The service used to create <see cref="HttpClient"/>s</param>
public class ExternalResourceProvider(IServiceProvider serviceProvider, IHttpClientFactory httpClientFactory)
    : IExternalResourceProvider
{

    /// <summary>
    /// Gets the current <see cref="IServiceProvider"/>
    /// </summary>
    protected IServiceProvider ServiceProvider { get; } = serviceProvider;

    /// <summary>
    /// Gets the service used to create <see cref="HttpClient"/>s
    /// </summary>
    protected IHttpClientFactory HttpClientFactory { get; } = httpClientFactory;

    /// <inheritdoc/>
    public virtual async Task<Stream> ReadAsync(ExternalResourceDefinition resource, WorkflowDefinition? workflow = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return resource.EndpointUri.Scheme switch
        {
            "file" => new FileStream(resource.EndpointUri.LocalPath, FileMode.Open),
            "http" or "https" => await this.ReadOverHttpAsync(resource, workflow, cancellationToken).ConfigureAwait(false),
            _ => throw new NotSupportedException($"Cannot retrieve resource at uri '{resource.EndpointUri}': the scheme '{resource.EndpointUri.Scheme}' is not supported")
        };
    }

    /// <summary>
    /// Reads the specified <see cref="ExternalResourceDefinition"/>
    /// </summary>
    /// <param name="resource">A reference to the external resource to read</param>
    /// <param name="workflow">The <see cref="WorkflowDefinition"/>, if any, in the context of which to read the specified resource</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The specified <see cref="ExternalResourceDefinition"/>'s content <see cref="Stream"/></returns>
    protected virtual async Task<Stream> ReadOverHttpAsync(ExternalResourceDefinition resource, WorkflowDefinition? workflow = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(resource);
        using var httpClient = this.HttpClientFactory.CreateClient();
        await httpClient.ConfigureAuthenticationAsync(resource.Endpoint.Authentication, this.ServiceProvider, workflow, cancellationToken).ConfigureAwait(false);
        return await httpClient.GetStreamAsync(resource.EndpointUri, cancellationToken).ConfigureAwait(false);
    }

}
