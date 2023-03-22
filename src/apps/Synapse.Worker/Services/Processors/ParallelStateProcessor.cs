/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

namespace Synapse.Worker.Services.Processors
{

    /// <summary>
    /// Represents a <see cref="IWorkflowActivityProcessor"/> used to process <see cref="ParallelStateDefinition"/>s
    /// </summary>
    public class ParallelStateProcessor
        : StateProcessor<ParallelStateDefinition>
    {

        /// <inheritdoc/>
        public ParallelStateProcessor(ILoggerFactory loggerFactory, IWorkflowRuntimeContext context, IWorkflowActivityProcessorFactory activityProcessorFactory,
            IOptions<ApplicationOptions> options, V1WorkflowActivity activity, ParallelStateDefinition state)
            : base(loggerFactory, context, activityProcessorFactory, options, activity, state)
        {

        }

        /// <summary>
        /// Gets a boolean indicating whether or not child <see cref="BranchDefinition"/>s have been executed according to the configured <see cref="ParallelCompletionType"/>
        /// </summary>
        protected bool BranchesExecuted { get; private set; }

        /// <summary>
        /// Gets a boolean indicating whether or not child <see cref="BranchDefinition"/>s have been completed according to the configured <see cref="ParallelCompletionType"/>
        /// </summary>
        protected bool BranchesCompleted { get; private set; }

        /// <inheritdoc/>
        protected override IWorkflowActivityProcessor CreateProcessorFor(V1WorkflowActivity activity)
        {
            var cancellationToken = this.CancellationTokenSource.Token;
            var processor = (BranchProcessor)base.CreateProcessorFor(activity);
            processor.OfType<V1WorkflowActivityCompletedIntegrationEvent>().SubscribeAsync
            (
                async e => await this.OnBranchExecutedAsync(processor, e, cancellationToken),
                async ex => await this.OnErrorAsync(ex, cancellationToken),
                async () => await this.OnBranchCompletedAsync(processor, cancellationToken)
            );
            return processor;
        }

        /// <inheritdoc/>
        protected override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            if (this.Activity.Status == V1WorkflowActivityStatus.Pending)
            {
                foreach (var branch in this.State.Branches)
                {
                    var metadata = new Dictionary<string, string>()
                    {
                        { V1WorkflowActivityMetadata.State, this.State.Name },
                        { V1WorkflowActivityMetadata.Branch, branch.Name }
                    };
                    await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.Branch, this.Activity.Input!.ToObject()!, metadata, this.Activity, cancellationToken);
                }
            }
            foreach (var activity in await this.Context.Workflow.GetOperativeActivitiesAsync(this.Activity, cancellationToken))
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                this.CreateProcessorFor(activity);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        /// <inheritdoc/>
        protected override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            foreach (var processor in this.Processors)
            {
                await processor.ProcessAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Handles the <see cref="BranchProcessor"/>'s execution
        /// </summary>
        /// <param name="processor">The <see cref="BranchProcessor"/> that has produced the <see cref="V1WorkflowActivityCompletedIntegrationEvent"/> to process</param>
        /// <param name="e">The <see cref="V1WorkflowActivityCompletedIntegrationEvent"/> to handle</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnBranchExecutedAsync(BranchProcessor processor, V1WorkflowActivityCompletedIntegrationEvent e, CancellationToken cancellationToken)
        {
            if (this.BranchesExecuted)
                return;
            var output = null as object;
            var childActivities = (await this.Context.Workflow.GetActivitiesAsync(this.Activity, cancellationToken))
                .Where(a => a.Type == V1WorkflowActivityType.Branch)
                .ToList();
            var executed = false;
            switch (this.State.CompletionType)
            {
                case ParallelCompletionType.AtLeastN:
                    var executedActivities = childActivities
                        .Where(p => p.Status >= V1WorkflowActivityStatus.Faulted)
                        .ToList();
                    if (executedActivities.Count() >= this.State.N)
                    {
                        executed = true;
                        output = new();
                        foreach (var executedActivity in executedActivities)
                        {
                            output = output.Merge(executedActivity.Output!.ToObject()!);
                        }
                    }
                    break;
                case ParallelCompletionType.AllOf:
                    if (childActivities.All(p => p.Status >= V1WorkflowActivityStatus.Faulted))
                    {
                        executed = true;
                        output = new();
                        foreach (var executedActivity in childActivities)
                        {
                            output = output.Merge(executedActivity.Output!.ToObject()!);
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException($"The specified {nameof(ParallelCompletionType)} '{this.State.CompletionType}' is not supported");
            }
            if (!executed)
                return;
            using (await this.Lock.LockAsync(cancellationToken))
            {
                if (this.BranchesExecuted)
                    return;
                else
                    this.BranchesExecuted = true;
            }
            await this.OnNextAsync(new V1WorkflowActivityCompletedIntegrationEvent(this.Activity.Id, output), cancellationToken);
        }

        /// <summary>
        /// Handles the completion of the specified <see cref="BranchProcessor"/>
        /// </summary>
        /// <param name="processor">The <see cref="BranchProcessor"/> to handle the completion of</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnBranchCompletedAsync(BranchProcessor processor, CancellationToken cancellationToken)
        {
            this.Processors.TryRemove(processor);
            processor.Dispose();
            if (this.BranchesCompleted)
                return;
            using (await this.Lock.LockAsync(cancellationToken))
            {
                var childActivities = (await this.Context.Workflow.GetActivitiesAsync(this.Activity, cancellationToken))
                    .Where(a => a.Type == V1WorkflowActivityType.Branch)
                    .ToList();
                var completed = false;
                switch (this.State.CompletionType)
                {
                    case ParallelCompletionType.AllOf:
                        completed = childActivities.All(p => p.Status >= V1WorkflowActivityStatus.Faulted);
                        break;
                    case ParallelCompletionType.AtLeastN:
                        if (childActivities.Where(p => p.Status >= V1WorkflowActivityStatus.Faulted).Count() >= this.State.N)
                            completed = true;
                        break;
                    default:
                        throw new NotSupportedException($"The specified {nameof(ParallelCompletionType)} '{this.State.CompletionType}' is not supported");
                }
                if (!completed)
                    return;
                if (completed && !this.BranchesCompleted)
                    this.BranchesCompleted = true;
                else
                    return;
            }
            await this.OnCompletedAsync(cancellationToken);
        }

    }

}
