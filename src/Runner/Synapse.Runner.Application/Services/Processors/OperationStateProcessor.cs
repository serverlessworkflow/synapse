using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Domain.Models;
using Synapse.Runner.Application.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Synapse.Runner.Application.Services.Processors
{

    /// <summary>
    /// Represents a <see cref="IWorkflowActivityProcessor"/> used to process <see cref="OperationStateDefinition"/>s
    /// </summary>
    public class OperationStateProcessor
        : StateProcessor<OperationStateDefinition>
    {

        /// <summary>
        /// Initializes a new <see cref="OperationStateProcessor"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="executionContext">The current <see cref="IWorkflowExecutionContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="options">The service used to access the current <see cref="ApplicationOptions"/></param>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to process</param>
        /// <param name="state">The <see cref="OperationStateDefinition"/> to process</param>
        public OperationStateProcessor(ILoggerFactory loggerFactory, IWorkflowExecutionContext executionContext, IWorkflowActivityProcessorFactory activityProcessorFactory, IOptions<ApplicationOptions> options, V1WorkflowActivity activity, OperationStateDefinition state) 
            : base(loggerFactory, executionContext, activityProcessorFactory, options, activity, state)
        {

        }

        /// <inheritdoc/>
        protected override IWorkflowActivityProcessor CreateProcessorFor(V1WorkflowActivity activity)
        {
            ActionProcessor processor = base.CreateProcessorFor(activity) as ActionProcessor;
            processor.SubscribeAsync
            (
                async result => await this.OnActionExecutedAsync(processor, result, this.CancellationTokenSource.Token),
                async ex => await this.OnActionErrorAsync(processor, ex, this.CancellationTokenSource.Token),
                async () => await this.OnActionCompletedAsync(processor, this.CancellationTokenSource.Token)
            );
            return processor;
        }

        /// <inheritdoc/>
        protected override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            if (this.Activity.Status == V1WorkflowActivityStatus.Initializing)
            {
                switch (this.State.ActionMode)
                {
                    case ActionExecutionMode.Parallel:
                        foreach (ActionDefinition action in this.State.Actions)
                        {
                            await this.ExecutionContext.CreateActivityAsync(V1WorkflowActivity.Action(this.ExecutionContext.Instance, this.State, action, this.ExecutionContext.ExpressionEvaluator.FilterInput(action, this.Activity.Data), this.Activity), cancellationToken);
                        }
                        break;
                    case ActionExecutionMode.Sequential:
                        ActionDefinition firstAction = this.State.Actions.First();
                        await this.ExecutionContext.CreateActivityAsync(V1WorkflowActivity.Action(this.ExecutionContext.Instance, this.State, firstAction, this.ExecutionContext.ExpressionEvaluator.FilterInput(firstAction, this.Activity.Data), this.Activity), cancellationToken);
                        break;
                    default:
                        throw new NotSupportedException($"The specified {nameof(ActionExecutionMode)} '{this.State.ActionMode}' is not supported");
                }
            }
            foreach (V1WorkflowActivity activity in await this.ExecutionContext.ListActiveChildActivitiesAsync(this.Activity, cancellationToken))
            {
                this.CreateProcessorFor(activity);
            }
        }

        /// <inheritdoc/>
        protected override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            foreach (IWorkflowActivityProcessor processor in this.Processors.ToList())
            {
                await processor.ProcessAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Handles an <see cref="ActionDefinition"/>'s <see cref="V1WorkflowExecutionResult"/>
        /// </summary>
        /// <param name="processor">The <see cref="ActionProcessor"/> that has returned the <see cref="V1WorkflowExecutionResult"/></param>
        /// <param name="result">The returned <see cref="V1WorkflowExecutionResult"/></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnActionExecutedAsync(ActionProcessor processor, V1WorkflowExecutionResult result, CancellationToken cancellationToken)
        {
            using (await this.Lock.LockAsync(cancellationToken))
            {
                if (this.State.ActionMode == ActionExecutionMode.Sequential)
                {
                    JToken output = result.Output;
                    if (!string.IsNullOrWhiteSpace(processor.Action.DataFilter?.ToStateData)
                        && this.Activity.Data is JObject data)
                    {
                        string expression = processor.Action.DataFilter.ToStateData.Trim();
                        if (expression.StartsWith("${"))
                            expression = expression[2..^1];
                        expression = $"{expression} = {output.ToString(Formatting.None)}";
                        data = this.ExecutionContext.ExpressionEvaluator.Evaluate(expression, data) as JObject;
                        await this.ExecutionContext.UpdateActivityDataAsync(this.Activity, data, cancellationToken);
                        output = data;
                    }
                    if (this.State.TryGetNextAction(processor.Activity.Metadata.Action, out ActionDefinition action))
                    {
                        V1WorkflowActivity nextActivity = await this.ExecutionContext.CreateActivityAsync(V1WorkflowActivity.Action(this.ExecutionContext.Instance, this.State, action, this.ExecutionContext.ExpressionEvaluator.FilterInput(action, output), this.Activity), cancellationToken);
                        this.CreateProcessorFor(nextActivity);
                    }
                    else
                    {
                        await this.OnResultAsync(V1WorkflowExecutionResult.Next(output), cancellationToken);
                    }
                }
                else
                {
                    List<V1WorkflowActivity> activities = (await this.ExecutionContext.ListActiveChildActivitiesAsync(this.Activity, cancellationToken))
                        .Where(a => a.Type == V1WorkflowActivityType.Action)
                        .Where(a => a.Status == V1WorkflowActivityStatus.Executed
                            && a.Result.Type == V1WorkflowExecutionResultType.Next
                            && a.Result.Output != null)
                        .ToList();
                    if (activities.Count == this.State.Actions.Count)
                    {
                        if (this.Activity.Data is not JObject output)
                            output = new JObject();
                        foreach (V1WorkflowActivity activity in activities
                            .Where(p => p.Result.Type == V1WorkflowExecutionResultType.Next && p.Result?.Output != null))
                        {
                            output.Merge(activity.Result.Output);
                        }
                        await this.OnResultAsync(new V1WorkflowExecutionResult(V1WorkflowExecutionResultType.Next, output), cancellationToken);
                    }
                }
            }
        }

        /// <summary>
        /// Handles an <see cref="ActionDefinition"/>'s <see cref="Exception"/>
        /// </summary>
        /// <param name="processor">The <see cref="ActionProcessor"/> that has thrown the specified <see cref="Exception"/></param>
        /// <param name="ex">The thrown <see cref="Exception"/></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnActionErrorAsync(ActionProcessor processor, Exception ex, CancellationToken cancellationToken)
        {
            using (await this.Lock.LockAsync(cancellationToken))
            {
                await base.OnErrorAsync(ex, cancellationToken);
            }
        }

        /// <summary>
        /// Handles the completion of the specified <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <param name="processor">The <see cref="ActionProcessor"/> that has returned the <see cref="V1WorkflowExecutionResult"/></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnActionCompletedAsync(ActionProcessor processor, CancellationToken cancellationToken)
        {
            using (await this.Lock.LockAsync(cancellationToken))
            {
                this.Processors.TryRemove(processor);
                processor.Dispose();
                IEnumerable<V1WorkflowActivity> activities = (await this.ExecutionContext.ListActiveChildActivitiesAsync(this.Activity, cancellationToken)).Where(a => a.Type == V1WorkflowActivityType.Action);
                if (activities.All(p => p.Status == V1WorkflowActivityStatus.Executed))
                {
                    await base.OnCompletedAsync(cancellationToken);
                    return;
                }
                foreach (V1WorkflowActivity activity in activities
                    .Where(p => p.Status == V1WorkflowActivityStatus.Pending))
                {
                    IWorkflowActivityProcessor nextProcessor = this.CreateProcessorFor(activity);
                    await nextProcessor.ProcessAsync(cancellationToken);
                }
            }
        }

    }

}
