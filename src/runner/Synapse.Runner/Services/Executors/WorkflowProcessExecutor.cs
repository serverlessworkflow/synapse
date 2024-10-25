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

using Neuroglia;
using Neuroglia.Data.Expressions;
using Neuroglia.Data.Infrastructure.ResourceOriented;
using System.Security.Cryptography;
using System.Text;

namespace Synapse.Runner.Services.Executors;

/// <summary>
/// Represents an <see cref="ITaskExecutor"/> implementation used to execute <see cref="WorkflowProcessDefinition"/>s
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="executionContextFactory">The service used to create <see cref="ITaskExecutionContext"/>s</param>
/// <param name="executorFactory">The service used to create <see cref="ITaskExecutor"/>s</param>
/// <param name="context">The current <see cref="ITaskExecutionContext"/></param>
/// <param name="schemaHandlerProvider">The service used to provide <see cref="ISchemaHandler"/> implementations</param>
/// <param name="serializer">The service used to serialize/deserialize objects to/from JSON</param>
/// <param name="api">The service used to interact with the Synapse API</param>
public class WorkflowProcessExecutor(IServiceProvider serviceProvider, ILogger<WorkflowProcessExecutor> logger, ITaskExecutionContextFactory executionContextFactory, ITaskExecutorFactory executorFactory, 
    ITaskExecutionContext<RunTaskDefinition> context, ISchemaHandlerProvider schemaHandlerProvider, IJsonSerializer serializer, ISynapseApiClient api)
    : TaskExecutor<RunTaskDefinition>(serviceProvider, logger, executionContextFactory, executorFactory, context, schemaHandlerProvider, serializer)
{

    /// <summary>
    /// Gets the service used to interact with the Synapse API
    /// </summary>
    protected ISynapseApiClient Api { get; } = api;

    /// <summary>
    /// Gets the definition of the shell process to run
    /// </summary>
    protected WorkflowProcessDefinition ProcessDefinition => this.Task.Definition.Run.Workflow!;

    /// <summary>
    /// Gets the subflow <see cref="WorkflowInstance"/>
    /// </summary>
    protected WorkflowInstance? Subflow { get; private set; }

    /// <summary>
    /// Gets a boolean indicating whether or not the execution of the subflow is being cancelled
    /// </summary>
    protected bool Cancelling { get; private set; }

    /// <inheritdoc/>
    protected override async Task DoExecuteAsync(CancellationToken cancellationToken)
    {
        var hash = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes($"{Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runner.Name)}{this.Task.Instance.Reference}"))).ToLowerInvariant();
        var workflowInstanceName = $"{this.ProcessDefinition.Name}-{hash}";
        var workflowInstanceNamespace = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runner.Namespace)!;
        try
        {
            this.Subflow = await this.Api.WorkflowInstances.GetAsync(workflowInstanceName, workflowInstanceNamespace, cancellationToken).ConfigureAwait(false);
            switch (this.Subflow.Status?.Phase)
            {
                case WorkflowInstanceStatusPhase.Cancelled:
                    await this.SetErrorAsync(new()
                    {
                        Type = ErrorType.Runtime,
                        Status = ErrorStatus.Runtime,
                        Title = ErrorTitle.Runtime,
                        Detail = $"The execution of workflow instance '{this.Subflow.GetQualifiedName()}' has been cancelled"
                    }, cancellationToken).ConfigureAwait(false);
                    return;
                case WorkflowInstanceStatusPhase.Faulted:
                    await this.SetErrorAsync(this.Subflow.Status.Error!, cancellationToken).ConfigureAwait(false);
                    return;
                case WorkflowInstanceStatusPhase.Completed:
                    var output = string.IsNullOrWhiteSpace(this.Subflow.Status?.OutputReference) ? null : (await this.Api.Documents.GetAsync(this.Subflow.Status.OutputReference, cancellationToken).ConfigureAwait(false)).Content;
                    await this.SetResultAsync(output, this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
                    return;
            }
        }
        catch
        {
            var workflow = await this.Api.Workflows.GetAsync(this.ProcessDefinition.Name, this.ProcessDefinition.Namespace, cancellationToken).ConfigureAwait(false);
            var workflowDefinition = this.ProcessDefinition.Version == "latest"
                ? workflow.Spec.Versions.Last()
                : workflow.Spec.Versions.Get(this.ProcessDefinition.Version) ?? throw new NullReferenceException($"Failed to find version '{this.ProcessDefinition.Version}' of workflow '{workflow.GetQualifiedName()}'");
            var input = await this.Task.Workflow.Expressions.EvaluateAsync<EquatableDictionary<string, object>>(this.ProcessDefinition.Input ?? new(), this.Task.Input, this.GetExpressionEvaluationArguments(), cancellationToken: cancellationToken).ConfigureAwait(false);
            this.Subflow = new WorkflowInstance()
            {
                Metadata = new()
                {
                    Namespace = workflowInstanceNamespace,
                    Name = workflowInstanceName
                },
                Spec = new()
                {
                    Definition = new()
                    {
                        Namespace = workflowDefinition.Document.Namespace,
                        Name = workflowDefinition.Document.Name,
                        Version = workflowDefinition.Document.Version
                    },
                    Input = input
                }
            };
            this.Subflow = await this.Api.WorkflowInstances.CreateAsync(this.Subflow, cancellationToken).ConfigureAwait(false);
        }
        if (this.Task.Definition.Run.Await == false) await this.SetResultAsync(new(), this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
        else
        {
            await foreach (var watchEvent in this.Api.WorkflowInstances.MonitorAsync(this.Subflow.GetName(), this.Subflow.GetNamespace()!, cancellationToken))
            {
                switch (watchEvent.Resource.Status?.Phase)
                {
                    case WorkflowInstanceStatusPhase.Cancelled:
                        if (!this.Cancelling)
                        {
                            await this.SetErrorAsync(new()
                            {
                                Type = ErrorType.Runtime,
                                Status = ErrorStatus.Runtime,
                                Title = ErrorTitle.Runtime,
                                Detail = $"The execution of workflow instance '{this.Subflow.GetQualifiedName()}' has been cancelled"
                            }, cancellationToken).ConfigureAwait(false);
                        }
                        break;
                    case WorkflowInstanceStatusPhase.Faulted:
                        await this.SetErrorAsync(watchEvent.Resource.Status.Error!, cancellationToken).ConfigureAwait(false);
                        return;
                    case WorkflowInstanceStatusPhase.Completed:
                        var output = string.IsNullOrWhiteSpace(watchEvent.Resource.Status?.OutputReference) ? null : (await this.Api.Documents.GetAsync(watchEvent.Resource.Status.OutputReference, cancellationToken).ConfigureAwait(false)).Content;
                        await this.SetResultAsync(output, this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
                        return;
                }
                if (this.Cancelling) break;
            }
        }
    }

    /// <inheritdoc/>
    public override async Task CancelAsync(CancellationToken cancellationToken = default)
    {
        if (this.Subflow != null && this.Task.Definition.Run.Await != false)
        {
            try
            {
                this.Cancelling = true;
                await this.Api.WorkflowInstances.CancelAsync(this.Subflow.GetName(), this.Subflow.GetNamespace()!, cancellationToken).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                this.Logger.LogError("An error occurred while cancelling the subflow instance '{subflow}': {ex}", this.Subflow.GetQualifiedName(), ex);
            }
        }
        await base.CancelAsync(cancellationToken).ConfigureAwait(false);
    }

}