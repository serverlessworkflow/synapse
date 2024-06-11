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
    public virtual async Task<Stream> ReadAsync(WorkflowDefinition workflow, ExternalResourceDefinition resource, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(resource);
        return resource.Uri.Scheme switch
        {
            "file" => new FileStream(resource.Uri.LocalPath, FileMode.Open),
            "http" or "https" => await this.ReadOverHttpAsync(workflow, resource, cancellationToken).ConfigureAwait(false),
            _ => throw new NotSupportedException($"Cannot retrieve resource at uri '{resource.Uri}': the scheme '{resource.Uri.Scheme}' is not supported")
        };
    }

    /// <summary>
    /// Reads the specified <see cref="ExternalResourceDefinition"/>
    /// </summary>
    /// <param name="workflow">The <see cref="WorkflowDefinition"/> in the context of which to read the specified resource</param>
    /// <param name="resource">A reference to the external resource to read</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The specified <see cref="ExternalResourceDefinition"/>'s content <see cref="Stream"/></returns>
    protected virtual async Task<Stream> ReadOverHttpAsync(WorkflowDefinition workflow, ExternalResourceDefinition resource, CancellationToken cancellationToken = default)
    {
        using var httpClient = this.HttpClientFactory.CreateClient();
        await httpClient.ConfigureAuthenticationAsync(resource.Authentication, this.ServiceProvider, cancellationToken).ConfigureAwait(false);
        return await httpClient.GetStreamAsync(resource.Uri, cancellationToken).ConfigureAwait(false);
    }

}
