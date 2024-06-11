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

namespace Synapse.Runner.Services.Executors;

/// <summary>
/// Represents an <see cref="ITaskExecutor"/> implementation used to execute <see cref="ListenTaskDefinition"/>s
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="executionContextFactory">The service used to create <see cref="ITaskExecutionContext"/>s</param>
/// <param name="executorFactory">The service used to create <see cref="ITaskExecutor"/>s</param>
/// <param name="context">The current <see cref="ITaskExecutionContext"/></param>
/// <param name="serializer">The service used to serialize/deserialize objects to/from JSON</param>
public class ListenTaskExecutor(IServiceProvider serviceProvider, ILogger<ListenTaskExecutor> logger, ITaskExecutionContextFactory executionContextFactory, 
    ITaskExecutorFactory executorFactory, ITaskExecutionContext<ListenTaskDefinition> context, IJsonSerializer serializer)
    : TaskExecutor<ListenTaskDefinition>(serviceProvider, logger, executionContextFactory, executorFactory, context, serializer)
{

    /// <summary>
    /// Gets the <see cref="ListenTaskExecutor"/>'s <see cref="ICloudEventBus"/> subscription
    /// </summary>
    protected IDisposable? Subscription { get; set; }

    /// <inheritdoc/>
    protected override async Task DoExecuteAsync(CancellationToken cancellationToken)
    {
        var context = await this.Task.CorrelateAsync(cancellationToken).ConfigureAwait(false);
        await this.SetResultAsync(context, this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
    }

}
