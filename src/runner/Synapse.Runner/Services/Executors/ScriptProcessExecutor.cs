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
/// Represents an <see cref="ITaskExecutor"/> implementation used to execute <see cref="RunTaskDefinition"/>s
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="executionContextFactory">The service used to create <see cref="ITaskExecutionContext"/>s</param>
/// <param name="executorFactory">The service used to create <see cref="ITaskExecutor"/>s</param>
/// <param name="context">The current <see cref="ITaskExecutionContext"/></param>
/// <param name="serializer">The service used to serialize/deserialize objects to/from JSON</param>
/// <param name="externalResourceProvider">The service used to resolve external resources</param>
/// <param name="scriptExecutorProvider">The service used to provide <see cref="IScriptExecutor"/>s</param>
public class ScriptProcessExecutor(IServiceProvider serviceProvider, ILogger<ScriptProcessExecutor> logger, ITaskExecutionContextFactory executionContextFactory, ITaskExecutorFactory executorFactory, 
    ITaskExecutionContext<RunTaskDefinition> context, IJsonSerializer serializer, IExternalResourceProvider externalResourceProvider, IScriptExecutorProvider scriptExecutorProvider)
    : TaskExecutor<RunTaskDefinition>(serviceProvider, logger, executionContextFactory, executorFactory, context, serializer)
{

    /// <summary>
    /// Gets the service used to resolve external resources
    /// </summary>
    protected IExternalResourceProvider ExternalResourceProvider { get; } = externalResourceProvider;

    /// <summary>
    /// Gets the service used to provide <see cref="IScriptExecutor"/>s
    /// </summary>
    protected IScriptExecutorProvider ScriptExecutorProvider { get; } = scriptExecutorProvider;

    /// <summary>
    /// Gets the definition of the script process to run
    /// </summary>
    protected ScriptProcessDefinition ProcessDefinition => this.Task.Definition.Run.Script!;

    /// <inheritdoc/>
    protected override async Task DoExecuteAsync(CancellationToken cancellationToken)
    {
        var executor = this.ScriptExecutorProvider.GetExecutor(this.ProcessDefinition.Language) ?? throw new NullReferenceException($"Failed to find a script executor for the specified language '{this.ProcessDefinition.Language}'");
        var script = this.ProcessDefinition.Code;
        if (string.IsNullOrWhiteSpace(script))
        {
            if (this.ProcessDefinition.Source == null) throw new NullReferenceException("The script's code or resource must be set");
            using var stream = await this.ExternalResourceProvider.ReadAsync(this.Task.Workflow.Definition, this.ProcessDefinition.Source, cancellationToken).ConfigureAwait(false);
            using var streamReader = new StreamReader(stream);
            script = await streamReader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        }
        using var process = await executor.ExecuteAsync(script, null, this.ProcessDefinition.Environment, cancellationToken).ConfigureAwait(false);
        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        var stdOut = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        var stdErr = await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        //todo: urgent: fix the script process so that we provide:
        //1. (evaluated) arguments
        //2. (evaluated) environment variables
        //3. a way to decide, like for shell, where to read the output from: stdOut, file? or (exit) code
    }

}
