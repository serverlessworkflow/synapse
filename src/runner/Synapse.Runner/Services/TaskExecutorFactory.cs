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

using Synapse.Runner.Services.Executors;

namespace Synapse.Runner.Services;

/// <summary>
/// Represents the default implementation of the <see cref="ITaskExecutorFactory"/> interface
/// </summary>
public class TaskExecutorFactory
    : ITaskExecutorFactory
{

    /// <inheritdoc/>
    public virtual ITaskExecutor Create(IServiceProvider serviceProvider, ITaskExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(context);
        return context.Definition switch
        {
            CallTaskDefinition => this.CreateCallTaskExecutor(serviceProvider, (ITaskExecutionContext<CallTaskDefinition>)context),
            CompositeTaskDefinition => this.CreateCompositeTaskExecutor(serviceProvider, (ITaskExecutionContext<CompositeTaskDefinition>)context),
            EmitTaskDefinition => ActivatorUtilities.CreateInstance<EmitTaskExecutor>(serviceProvider, context),
            ExtensionTaskDefinition => ActivatorUtilities.CreateInstance<ExtensionTaskExecutor>(serviceProvider, context),
            ForTaskDefinition => ActivatorUtilities.CreateInstance<ForTaskExecutor>(serviceProvider, context),
            ListenTaskDefinition => ActivatorUtilities.CreateInstance<ListenTaskExecutor>(serviceProvider, context),
            RaiseTaskDefinition => ActivatorUtilities.CreateInstance<RaiseTaskExecutor>(serviceProvider, context),
            RunTaskDefinition => this.CreateRunTaskExecutor(serviceProvider, (ITaskExecutionContext<RunTaskDefinition>)context),
            SetTaskDefinition => ActivatorUtilities.CreateInstance<SetTaskExecutor>(serviceProvider, context),
            SwitchTaskDefinition => ActivatorUtilities.CreateInstance<SwitchTaskExecutor>(serviceProvider, context),
            TryTaskDefinition => ActivatorUtilities.CreateInstance<TryTaskExecutor>(serviceProvider, context),
            WaitTaskDefinition => ActivatorUtilities.CreateInstance<WaitTaskExecutor>(serviceProvider, context),
            _ => throw new NotSupportedException($"The specified task definition type '{context.Definition.GetType()}' is not supported")
        };
    }

    /// <inheritdoc/>
    public virtual ITaskExecutor<TDefinition> Create<TDefinition>(IServiceProvider serviceProvider, ITaskExecutionContext<TDefinition> context)
        where TDefinition : TaskDefinition
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(context);
        return (ITaskExecutor<TDefinition>)this.Create(serviceProvider, (ITaskExecutionContext)context);
    }

    /// <summary>
    /// Creates a new <see cref="CallTaskDefinition"/> executor for the specified context
    /// </summary>
    /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
    /// <param name="context">The context for which to create a new <see cref="CallTaskDefinition"/> executor</param>
    /// <returns>A new <see cref="ITaskExecutor"/> instance</returns>
    protected virtual ITaskExecutor CreateCallTaskExecutor(IServiceProvider serviceProvider, ITaskExecutionContext<CallTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(context);
        return context.Definition.Call switch
        {
            Function.Grpc => ActivatorUtilities.CreateInstance<GrpcCallExecutor>(serviceProvider, context),
            Function.Http => ActivatorUtilities.CreateInstance<HttpCallExecutor>(serviceProvider, context),
            Function.OpenApi => ActivatorUtilities.CreateInstance<OpenApiCallExecutor>(serviceProvider, context),
            _ => throw new NotSupportedException($"Unknown/unsupported function '{context.Definition.Call}'")
        };
    }

    /// <summary>
    /// Creates a new <see cref="CompositeTaskDefinition"/> executor for the specified context
    /// </summary>
    /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
    /// <param name="context">The context for which to create a new <see cref="CallTaskDefinition"/> executor</param>
    /// <returns>A new <see cref="ITaskExecutor"/> instance</returns>
    protected virtual ITaskExecutor CreateCompositeTaskExecutor(IServiceProvider serviceProvider, ITaskExecutionContext<CompositeTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(context);
        if (context.Definition.Execute.Concurrently?.Count > 1) return ActivatorUtilities.CreateInstance<ConcurrentCompositeTaskExecutor>(serviceProvider, context);
        else if (context.Definition.Execute.Sequentially?.Count > 1) return ActivatorUtilities.CreateInstance<SequentialCompositeTaskExecutor>(serviceProvider, context);
        else throw new ErrorRaisedException(Error.Configuration(context.Instance.Reference, "The execution strategy must be configured and define a minimum of two tasks"));
    }

    /// <summary>
    /// Creates a new <see cref="RunTaskDefinition"/> executor for the specified context
    /// </summary>
    /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
    /// <param name="context">The context for which to create a new <see cref="RunTaskDefinition"/> executor</param>
    /// <returns>A new <see cref="ITaskExecutor"/> instance</returns>
    protected virtual ITaskExecutor CreateRunTaskExecutor(IServiceProvider serviceProvider, ITaskExecutionContext<RunTaskDefinition> context)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(context);
        return context.Definition.Run.ProcessType switch
        {
            ProcessType.Container => ActivatorUtilities.CreateInstance<ContainerProcessExecutor>(serviceProvider, context),
            ProcessType.Extension => throw new NotImplementedException(), //todo: ActivatorUtilities.CreateInstance<ExtensionProcessExecutor>(serviceProvider, context),
            ProcessType.Script => ActivatorUtilities.CreateInstance<ScriptProcessExecutor>(serviceProvider, context),
            ProcessType.Shell => ActivatorUtilities.CreateInstance<ShellProcessExecutor>(serviceProvider, context),
            ProcessType.Workflow => ActivatorUtilities.CreateInstance<WorkflowProcessExecutor>(serviceProvider, context),
            _ => throw new NotSupportedException($"The specified process type '{context.Definition.Run.ProcessType}' is not supported")
        };

    }

}
