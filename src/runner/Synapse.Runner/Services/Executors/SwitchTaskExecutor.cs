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
/// Represents an <see cref="ITaskExecutor"/> implementation used to execute <see cref="SwitchTaskDefinition"/>s
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="executionContextFactory">The service used to create <see cref="ITaskExecutionContext"/>s</param>
/// <param name="executorFactory">The service used to create <see cref="ITaskExecutor"/>s</param>
/// <param name="context">The current <see cref="ITaskExecutionContext"/></param>
/// <param name="serializer">The service used to serialize/deserialize objects to/from JSON</param>
public class SwitchTaskExecutor(IServiceProvider serviceProvider, ILogger<SwitchTaskExecutor> logger, ITaskExecutionContextFactory executionContextFactory, ITaskExecutorFactory executorFactory, ITaskExecutionContext<SwitchTaskDefinition> context, IJsonSerializer serializer)
    : TaskExecutor<SwitchTaskDefinition>(serviceProvider, logger, executionContextFactory, executorFactory, context, serializer)
{

    /// <inheritdoc/>
    protected override async Task DoExecuteAsync(CancellationToken cancellationToken)
    {
        var matches = new List<KeyValuePair<string, SwitchCaseDefinition>>();
        var defaultCase = this.Task.Definition.Switch.FirstOrDefault(kvp => string.IsNullOrWhiteSpace(kvp.Value.When));
        foreach (var @case in this.Task.Definition.Switch!.Where(c => !string.IsNullOrWhiteSpace(c.Value.When)))
        {
            if (await this.Task.Workflow.Expressions.EvaluateConditionAsync(@case.Value.When!, this.Task.Input, this.GetExpressionEvaluationArguments(), cancellationToken).ConfigureAwait(false))
            {
                matches.Add(@case);
            }
        }
        if (matches.Count == 1) await this.SetResultAsync(this.Task.Input, matches.First().Value.Then, cancellationToken).ConfigureAwait(false);
        else if (matches.Count > 1) await this.SetErrorAsync(Error.Configuration(this.Task.Instance.Reference, $"At most one matching case is allowed, but cases {string.Join(", ", matches.Select(m => m.Key))} have been matched."), cancellationToken).ConfigureAwait(false);
        else if (defaultCase.Key != default) await this.SetResultAsync(this.Task.Input, defaultCase.Value.Then, cancellationToken).ConfigureAwait(false);
        else await this.SetResultAsync(this.Task.Input, this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
    }

}
