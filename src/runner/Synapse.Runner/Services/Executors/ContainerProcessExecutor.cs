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
/// Represents an <see cref="ITaskExecutor"/> implementation used to execute <see cref="ContainerProcessDefinition"/>s
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="executionContextFactory">The service used to create <see cref="ITaskExecutionContext"/>s</param>
/// <param name="executorFactory">The service used to create <see cref="ITaskExecutor"/>s</param>
/// <param name="containers">The service used to manage <see cref="IContainer"/>s</param>
/// <param name="context">The current <see cref="ITaskExecutionContext"/></param>
/// <param name="serializer">The service used to serialize/deserialize objects to/from JSON</param>
public class ContainerProcessExecutor(IServiceProvider serviceProvider, ILogger<ContainerProcessExecutor> logger, ITaskExecutionContextFactory executionContextFactory, ITaskExecutorFactory executorFactory, IContainerPlatform containers, ITaskExecutionContext<RunTaskDefinition> context, IJsonSerializer serializer)
    : TaskExecutor<RunTaskDefinition>(serviceProvider, logger, executionContextFactory, executorFactory, context, serializer)
{

    /// <summary>
    /// Gets the service used to manage <see cref="IContainer"/>s
    /// </summary>
    protected IContainerPlatform Containers { get; } = containers;

    /// <summary>
    /// Gets the definition of the container process to run
    /// </summary>
    protected ContainerProcessDefinition ProcessDefinition => this.Task.Definition.Run.Container!;

    /// <summary>
    /// Gets the <see cref="IContainer"/> to run
    /// </summary>
    protected IContainer? Container { get; set; }

    /// <inheritdoc/>
    protected override async Task DoInitializeAsync(CancellationToken cancellationToken)
    {
        this.Container = await this.Containers.CreateAsync(this.ProcessDefinition, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task DoExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            await this.Container!.StartAsync(cancellationToken).ConfigureAwait(false);
            await this.Container.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            var standardOutput = (this.Container.StandardOutput == null ? null : await this.Container.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false))?.Trim()[8..];
            var standardError = (this.Container.StandardError == null ? null : await this.Container.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false))?.Trim();
            var result = standardOutput; //todo: do something with return data encoding (ex: plain-text, json);
            await this.SetResultAsync(result, this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
        }
        catch(Exception ex)
        {
            var message = ex.Message;
            try { if (this.Container?.StandardError != null) message = await this.Container.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false); } catch { }
            var error = ex.ToError(this.Task.Instance.Reference);
            error.Detail = message;
            await this.SetErrorAsync(error, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    protected override Task DoSuspendAsync(CancellationToken cancellationToken) => this.Container == null ? System.Threading.Tasks.Task.CompletedTask : this.Container.StopAsync(cancellationToken);

    /// <inheritdoc/>
    protected override Task DoCancelAsync(CancellationToken cancellationToken) => this.Container == null ? System.Threading.Tasks.Task.CompletedTask : this.Container.StopAsync(cancellationToken);

}
