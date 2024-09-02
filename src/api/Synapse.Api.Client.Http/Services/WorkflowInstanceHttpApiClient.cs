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

using Neuroglia.Data.Infrastructure;

namespace Synapse.Api.Client.Services;

/// <summary>
/// Represents the default HTTP implementation of the <see cref="IWorkflowInstanceApiClient"/> interface
/// </summary>
/// <inheritdoc/>
public class WorkflowInstanceHttpApiClient(IServiceProvider serviceProvider, ILogger<ResourceHttpApiClient<WorkflowInstance>> logger, IOptions<SynapseHttpApiClientOptions> options, IJsonSerializer jsonSerializer, HttpClient httpClient)
    : ResourceHttpApiClient<WorkflowInstance>(serviceProvider, logger, options, jsonSerializer, httpClient), IWorkflowInstanceApiClient
{

    /// <inheritdoc/>
    public virtual async Task<string> ReadLogsAsync(string name, string @namespace, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(@namespace);
        var resource = new WorkflowInstance();
        var uri = $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/{@namespace}/{name}/logs";
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Get, uri), cancellationToken).ConfigureAwait(false);
        using var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task<IAsyncEnumerable<ITextDocumentWatchEvent>> WatchLogsAsync(string name, string @namespace, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(@namespace);
        var resource = new WorkflowInstance();
        var uri = $"/api/{resource.Definition.Version}/{resource.Definition.Plural}/{@namespace}/{name}/logs/watch";
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Get, uri), cancellationToken).ConfigureAwait(false);
        request.EnableWebAssemblyStreamingResponse();
        var response = await this.HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return this.JsonSerializer.DeserializeAsyncEnumerable<TextDocumentWatchEvent>(responseStream, cancellationToken)!;
    }

}