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
/// Represents an <see cref="ITaskExecutor"/> implementation used to execute <see cref="RaiseTaskDefinition"/>s
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="executionContextFactory">The service used to create <see cref="ITaskExecutionContext"/>s</param>
/// <param name="executorFactory">The service used to create <see cref="ITaskExecutor"/>s</param>
/// <param name="context">The current <see cref="ITaskExecutionContext"/></param>
/// <param name="schemaHandlerProvider">The service used to provide <see cref="ISchemaHandler"/> implementations</param>
/// <param name="serializer">The service used to serialize/deserialize objects to/from JSON</param>
public class RaiseTaskExecutor(IServiceProvider serviceProvider, ILogger<RaiseTaskExecutor> logger, ITaskExecutionContextFactory executionContextFactory, ITaskExecutorFactory executorFactory, ITaskExecutionContext<RaiseTaskDefinition> context, ISchemaHandlerProvider schemaHandlerProvider, IJsonSerializer serializer)
    : TaskExecutor<RaiseTaskDefinition>(serviceProvider, logger, executionContextFactory, executorFactory, context, schemaHandlerProvider, serializer)
{

    /// <inheritdoc/>
    protected override async Task DoExecuteAsync(CancellationToken cancellationToken)
    {
        var input = this.Task.Input;
        var errorDefinition = this.Task.Definition.Raise.Error;
        if (errorDefinition == null)
        {
            if (string.IsNullOrWhiteSpace(this.Task.Definition.Raise.ErrorReference)) throw new NullReferenceException("The error to raise must be defined (or referenced)");
            if (!this.Task.Workflow.Definition.Use?.Errors?.TryGetValue(this.Task.Definition.Raise.ErrorReference, out errorDefinition) == true || errorDefinition == null) throw new NullReferenceException($"Failed to find the referenced error definition '{this.Task.Definition.Raise.ErrorReference}'");
        }
        var status = errorDefinition.Status is string expression 
            ? expression.IsRuntimeExpression()
                ? await this.Task.Workflow.Expressions.EvaluateAsync<ushort>(errorDefinition.Status, input, this.GetExpressionEvaluationArguments(), cancellationToken).ConfigureAwait(false)
                : ushort.Parse(expression)
            : ushort.Parse(errorDefinition.Status.ToString()!);
        var type = errorDefinition.Type.IsRuntimeExpression()
            ? (await this.Task.Workflow.Expressions.EvaluateAsync<Uri>(errorDefinition.Type, input, this.GetExpressionEvaluationArguments(), cancellationToken).ConfigureAwait(false))!
            : new(errorDefinition.Type, UriKind.RelativeOrAbsolute);
        var title = errorDefinition.Title.IsRuntimeExpression()
            ? (await this.Task.Workflow.Expressions.EvaluateAsync<string>(errorDefinition.Title, input, this.GetExpressionEvaluationArguments(), cancellationToken).ConfigureAwait(false))!
            : errorDefinition.Title;
        var detail = string.IsNullOrWhiteSpace(errorDefinition.Detail) ? null : errorDefinition.Detail!.IsRuntimeExpression()
            ? await this.Task.Workflow.Expressions.EvaluateAsync<string>(errorDefinition.Detail!, input, this.GetExpressionEvaluationArguments(), cancellationToken).ConfigureAwait(false)
            : errorDefinition.Detail;
        var errorInstance = new Error()
        {
            Status = status,
            Type = type,
            Title = title,
            Detail = detail,
            Instance = this.Task.Instance.Reference
        };
        await this.SetErrorAsync(errorInstance, cancellationToken).ConfigureAwait(false);
    }

}
