using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Synapse.Runner.Application.Services.Processors
{

    /// <summary>
    /// Represents a <see cref="IWorkflowActivityProcessor"/> used to process <see cref="EventStateDefinition"/>s
    /// </summary>
    public class EventStateProcessor
        : StateProcessor<EventStateDefinition>
    {

        /// <summary>
        /// Initializes a new <see cref="EventStateProcessor"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="executionContext">The current <see cref="IWorkflowExecutionContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to process</param>
        /// <param name="state">The <see cref="EventStateDefinition"/> to process</param>
        public EventStateProcessor(ILoggerFactory loggerFactory, IWorkflowExecutionContext executionContext, IWorkflowActivityProcessorFactory activityProcessorFactory, V1WorkflowActivity activity, EventStateDefinition state)
            : base(loggerFactory, executionContext, activityProcessorFactory, activity, state)
        {

        }

        /// <inheritdoc/>
        protected override IWorkflowActivityProcessor CreateProcessorFor(V1WorkflowActivity activity)
        {
            EventStateTriggerProcessor processor = base.CreateProcessorFor(activity) as EventStateTriggerProcessor;
            processor.SubscribeAsync
            (
                async result => await this.OnTriggerResultAsync(processor, result, this.CancellationTokenSource.Token),
                async ex => await this.OnErrorAsync(ex, this.CancellationTokenSource.Token),
                async () => await this.OnTriggerCompletedAsync(processor, this.CancellationTokenSource.Token)
            );
            return processor;
        }

        /// <inheritdoc/>
        protected override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            if (this.Activity.Status <= V1WorkflowActivityStatus.Initializing)
            {
                foreach (EventStateTriggerDefinition trigger in this.State.Triggers)
                {
                    await this.ExecutionContext.CreateActivityAsync(V1WorkflowActivity.EventStateTrigger(this.ExecutionContext.Instance, this.State, trigger, this.Activity.Data, this.Activity), cancellationToken);
                }
            }
            foreach (V1WorkflowActivity childActivity in await this.ExecutionContext.ListActiveChildActivitiesAsync(this.Activity, cancellationToken))
            {
                this.CreateProcessorFor(childActivity);
            }
        }

        /// <inheritdoc/>
        protected override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            foreach (IWorkflowActivityProcessor childProcessor in this.Processors.ToList())
            {
                await childProcessor.ProcessAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Handles the next <see cref="IWorkflowActivityProcessor"/>'s <see cref="V1WorkflowExecutionResult"/>
        /// </summary>
        /// <param name="processor">The <see cref="IWorkflowActivityProcessor"/> that returned the <see cref="V1WorkflowExecutionResult"/></param>
        /// <param name="result">The <see cref="V1WorkflowExecutionResult"/> to handle</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnTriggerResultAsync(IWorkflowActivityProcessor processor, V1WorkflowExecutionResult result, CancellationToken cancellationToken)
        {
            using(await this.Lock.LockAsync(cancellationToken))
            {
                if (this.Activity.Status == V1WorkflowActivityStatus.Executed)
                    return;
                List<V1WorkflowActivity> childActivities = (await this.ExecutionContext.ListChildActivitiesAsync(this.Activity, true, cancellationToken))
                    .Where(p => p.Type == V1WorkflowActivityType.EventStateTrigger && p.Status == V1WorkflowActivityStatus.Executed)
                    .ToList();
                if (this.State.Exclusive
                    || childActivities.Count == this.State.Triggers.Count)
                {
                    JObject output = new();
                    foreach (V1WorkflowActivity childPointer in childActivities
                        .Where(p => p.Result.Type == V1WorkflowExecutionResultType.Next && p.Result.Output != null))
                    {
                        output.Merge(childPointer.Result.Output);
                    }
                    await this.OnResultAsync(new V1WorkflowExecutionResult(V1WorkflowExecutionResultType.Next, output), cancellationToken);
                    await this.OnCompletedAsync(cancellationToken);
                }
            }
        }

        /// <summary>
        /// Handles the completion of the specified <see cref="IWorkflowActivityProcessor"/>
        /// </summary>
        /// <param name="processor">The <see cref="IWorkflowActivityProcessor"/> to handle the completion of</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual Task OnTriggerCompletedAsync(IWorkflowActivityProcessor processor, CancellationToken cancellationToken)
        {
            this.Processors.TryRemove(processor);
            processor.Dispose();
            return Task.CompletedTask;
        }

    }

}
