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

using Neuroglia.Eventing.CloudEvents;
using Neuroglia.Eventing.CloudEvents.Infrastructure.Services;
using Neuroglia.Reactive;

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
/// <param name="cloudEventBus">The service used to stream both input and output <see cref="CloudEvent"/>s</param>
public class ListenTaskExecutor(IServiceProvider serviceProvider, ILogger<ListenTaskExecutor> logger, ITaskExecutionContextFactory executionContextFactory, 
    ITaskExecutorFactory executorFactory, ITaskExecutionContext<ListenTaskDefinition> context, IJsonSerializer serializer, ICloudEventBus cloudEventBus)
    : TaskExecutor<ListenTaskDefinition>(serviceProvider, logger, executionContextFactory, executorFactory, context, serializer)
{

    /// <summary>
    /// Gets the service used to stream both input and output <see cref="CloudEvent"/>s
    /// </summary>
    protected ICloudEventBus CloudEventBus { get; } = cloudEventBus;

    /// <summary>
    /// Gets the <see cref="ListenTaskExecutor"/>'s <see cref="ICloudEventBus"/> subscription
    /// </summary>
    protected IDisposable? Subscription { get; set; }

    /// <inheritdoc/>
    protected override async Task DoExecuteAsync(CancellationToken cancellationToken)
    {
        this.Subscription = this.CloudEventBus.InputStream.SubscribeAsync(async e => await this.OnCloudEventAsync(e, cancellationToken).ConfigureAwait(false));
        foreach(var e in await this.Task.Workflow.ConsumeOrBeginCorrelateAsync(this.Task.Definition.Listen, cancellationToken).ConfigureAwait(false)) await this.OnCloudEventAsync(e, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the specified <see cref="CloudEvent"/>
    /// </summary>
    /// <param name="e">The <see cref="CloudEvent"/> to handle</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual Task OnCloudEventAsync(CloudEvent e, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        //if (this.Task.Definition.Listen.To.One != null)
        //{
        //    await this.SetResultAsync(e, this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
        //}
        //else if (this.Task.Definition.Listen.To.Any != null)
        //{
        //    if (!string.IsNullOrWhiteSpace(this.Task.Definition.Listen.While) && !await this.Task.Workflow.Expressions.EvaluateConditionAsync(this.Task.Definition.Listen.While, e, this.GetExpressionEvaluationArguments(), cancellationToken).ConfigureAwait(false))
        //    {
        //        await this.SetResultAsync(e, this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
        //        return;
        //    }
        //    if (this.Task.Definition.Listen.Until)
        //    {

        //    }
        //}
        //else if (this.Task.Definition.Listen.To.All != null)
        //{
        //}
        //else await this.SetErrorAsync(Dsl.Resources.Error.Configuration(this.Task.Instance.Reference, "The 'listen' task requires one of the following properties to be set: 'one', 'any' or 'all' "), cancellationToken).ConfigureAwait(false);
    }

}
