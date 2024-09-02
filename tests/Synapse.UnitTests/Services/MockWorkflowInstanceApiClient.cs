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

using Neuroglia.Data.Infrastructure;
using Neuroglia.Data.Infrastructure.ResourceOriented.Services;
using Neuroglia.Data.Infrastructure.Services;
using Synapse.Api.Client.Services;

namespace Synapse.UnitTests.Services;

internal class MockWorkflowInstanceApiClient(IResourceRepository resources, ITextDocumentRepository<string> logs)
    : MockNamespacedResourceApiClient<WorkflowInstance>(resources), IWorkflowInstanceApiClient
{

    public Task<string> ReadLogsAsync(string name, string @namespace, CancellationToken cancellationToken = default) => logs.ReadToEndAsync($"{name}.{@namespace}", cancellationToken);

    public async Task<IAsyncEnumerable<ITextDocumentWatchEvent>> WatchLogsAsync(string name, string @namespace, CancellationToken cancellationToken = default) => (await logs.WatchAsync($"{name}.{@namespace}", cancellationToken)).ToAsyncEnumerable();

}