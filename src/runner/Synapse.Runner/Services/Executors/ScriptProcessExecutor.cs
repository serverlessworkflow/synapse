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

using Neuroglia.Data.Expressions;

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
/// <param name="schemaHandlerProvider">The service used to provide <see cref="ISchemaHandler"/> implementations</param>
/// <param name="scriptExecutorProvider">The service used to provide <see cref="IScriptExecutor"/>s</param>
public class ScriptProcessExecutor(IServiceProvider serviceProvider, ILogger<ScriptProcessExecutor> logger, ITaskExecutionContextFactory executionContextFactory, ITaskExecutorFactory executorFactory, 
    ITaskExecutionContext<RunTaskDefinition> context, ISchemaHandlerProvider schemaHandlerProvider, IJsonSerializer serializer, IExternalResourceProvider externalResourceProvider, IScriptExecutorProvider scriptExecutorProvider)
    : TaskExecutor<RunTaskDefinition>(serviceProvider, logger, executionContextFactory, executorFactory, context, schemaHandlerProvider, serializer)
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
        var arguments = this.ProcessDefinition.Arguments == null
            ? null
            : (await this.ProcessDefinition.Arguments.ToAsyncEnumerable().ToDictionaryAwaitAsync(kvp => ValueTask.FromResult(kvp.Key), async kvp => await this.EvaluateAndSerializeAsync(kvp.Value, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false))
                .Where(arg => !string.IsNullOrWhiteSpace(arg.Value))
                .SelectMany(arg => new string[] { $"--{arg.Key}", arg.Value! })
                .Where(arg => arg != "True" && arg != "False");
        var environment = this.ProcessDefinition.Environment == null 
            ? null 
            : await this.ProcessDefinition.Environment.ToAsyncEnumerable().ToDictionaryAwaitAsync(kvp => ValueTask.FromResult(kvp.Key), async kvp => (await this.EvaluateAndSerializeAsync(kvp.Value, cancellationToken).ConfigureAwait(false))!, cancellationToken).ConfigureAwait(false);
        using var process = await executor.ExecuteAsync(script, arguments, environment, cancellationToken).ConfigureAwait(false);
        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        var rawOutput = (await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false)).Trim();
        var errorMessage = (await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false)).Trim();
        if (process.ExitCode == 0) await this.SetResultAsync(rawOutput, this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
        else await this.SetErrorAsync(new()
        {
            Status = (int)HttpStatusCode.InternalServerError,
            Type = ErrorType.Runtime,
            Title = ErrorTitle.Runtime,
            Detail = errorMessage,
            Instance = this.Task.Instance.Reference
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Evaluates and serializes the specified value
    /// </summary>
    /// <param name="value">The value to serialize and evaluate</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The evaluated and serialized value</returns>
    protected virtual async Task<string?> EvaluateAndSerializeAsync(object? value, CancellationToken cancellationToken = default)
    {
        if (value == null) return null;
        var evaluated = value is string str && str.IsRuntimeExpression()
            ? await this.Task.Workflow.Expressions.EvaluateAsync(str, this.Task.Input, this.GetExpressionEvaluationArguments(), null, cancellationToken).ConfigureAwait(false)
            : value.GetType().IsValueType ? value : await this.Task.Workflow.Expressions.EvaluateAsync(value, this.Task.Input, this.GetExpressionEvaluationArguments(), null, cancellationToken).ConfigureAwait(false);
        if (evaluated == null) return null;
        else if (evaluated.GetType().IsValueType) return evaluated.ToString();
        else return this.JsonSerializer.SerializeToText(evaluated);
    }

}
