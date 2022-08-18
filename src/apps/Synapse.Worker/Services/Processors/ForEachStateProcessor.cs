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

using Synapse.Integration.Events.WorkflowActivities;
using System.Collections;
using System.Reactive.Linq;

namespace Synapse.Worker.Services.Processors
{

    /// <summary>
    /// Represents a <see cref="IWorkflowActivityProcessor"/> used to process <see cref="ForEachStateDefinition"/>s
    /// </summary>
    public class ForEachStateProcessor
        : StateProcessor<ForEachStateDefinition>
    {

        /// <inheritdoc/>
        public ForEachStateProcessor(ILoggerFactory loggerFactory, IWorkflowRuntimeContext context, IWorkflowActivityProcessorFactory activityProcessorFactory,
            IJsonSerializer jsonSerializer, IOptions<ApplicationOptions> options, V1WorkflowActivity activity, ForEachStateDefinition state)
            : base(loggerFactory, context, activityProcessorFactory, options, activity, state)
        {
            this.JsonSerializer = jsonSerializer;
        }

        /// <summary>
        /// Gets the service used to serialize/deserialize to/from JSON
        /// </summary>
        protected IJsonSerializer JsonSerializer { get; }

        /// <inheritdoc/>
        protected override IWorkflowActivityProcessor CreateProcessorFor(V1WorkflowActivity activity)
        {
            var cancellationToken = this.CancellationTokenSource.Token;
            var processor = (IterationProcessor)base.CreateProcessorFor(activity);
            processor.OfType<V1WorkflowActivityCompletedIntegrationEvent>().SubscribeAsync
            (
                async result => await this.OnIterationResultAsync(processor, result, cancellationToken),
                async ex => await this.OnErrorAsync(ex, cancellationToken),
                async () => await this.OnIterationCompletedAsync(processor, cancellationToken)
            );
            return processor;
        }

        /// <inheritdoc/>
        protected override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            if (this.Activity.Status == V1WorkflowActivityStatus.Pending)
            {
                var inputCollection = await this.GetInputCollectionAsync(cancellationToken);
                if (!inputCollection.Any())
                {
                    await this.OnNextAsync(new V1WorkflowActivityCompletedIntegrationEvent(this.Activity.Id, this.Activity.Input!.ToObject()!), cancellationToken);
                    await this.OnCompletedAsync(cancellationToken);
                    return;
                }
                var iterationParamValue = inputCollection.First();
                var input = (DynamicObject)this.Activity.Input!;
                var iterationParam = this.State.IterationParameter;
                if (string.IsNullOrWhiteSpace(iterationParam))
                    iterationParam = "item";
                input.Set(iterationParam, iterationParamValue);
                var metadata = new Dictionary<string, string>()
                {
                    { V1WorkflowActivityMetadata.State, this.State.Name! },
                    { V1WorkflowActivityMetadata.Iteration, "0" }
                };
                await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.Iteration, input.ToObject(), metadata, this.Activity, cancellationToken);
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
            foreach (var childProcessor in this.Processors)
            {
                await childProcessor.ProcessAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Gets the <see cref="ForEachStateDefinition"/>'s input <see cref="IEnumerable"/>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The <see cref="ForEachStateDefinition"/>'s input <see cref="IEnumerable"/></returns>
        protected virtual async Task<IEnumerable<object>> GetInputCollectionAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(this.State.InputCollection))
                return Array.Empty<object>();
            var inputCollection = await this.Context.EvaluateAsync(this.State.InputCollection, this.Activity.Input!.ToObject(), cancellationToken);
            if (inputCollection == null)
                throw new NullReferenceException($"Failed to resolve the input collection defined by expression '{this.State.InputCollection}'");
            var enumerable = inputCollection as IEnumerable;
            if (enumerable is Dynamic dyn)
                enumerable = (IEnumerable)dyn.ToObject()!;
            return enumerable!.OfType<object>();
        }

        /// <summary>
        /// Gets the <see cref="ForEachStateDefinition"/>'s output <see cref="IEnumerable"/>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The <see cref="ForEachStateDefinition"/>'s output <see cref="IEnumerable"/></returns>
        protected virtual async Task<List<object>> GetOutputCollectionAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(this.State.OutputCollection))
                return new();
            var outputCollection = await this.Context.EvaluateAsync(this.State.OutputCollection, this.Activity.Input!.ToObject(), cancellationToken);
            if (outputCollection == null)
                return new();
            var enumerable = outputCollection as IEnumerable;
            if (enumerable is Dynamic dyn)
                enumerable = (IEnumerable)dyn.ToObject()!;
            return enumerable!.OfType<object>().ToList();
        }

        /// <summary>
        /// Handles the <see cref="V1WorkflowActivityCompletedIntegrationEvent"/> produced by the specified <see cref="IterationProcessor"/>
        /// </summary>
        /// <param name="processor">The <see cref="IterationProcessor"/> that has produced the <see cref="V1WorkflowActivityCompletedIntegrationEvent"/> to handle</param>
        /// <param name="e">The <see cref="V1WorkflowActivityCompletedIntegrationEvent"/> to handle</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnIterationResultAsync(IterationProcessor processor, V1WorkflowActivityCompletedIntegrationEvent e, CancellationToken cancellationToken)
        {
            var iterationIndex = int.Parse(processor.Activity.Metadata[V1WorkflowActivityMetadata.Iteration]);
            var inputCollection = await this.GetInputCollectionAsync(cancellationToken);
            if ((this.State.BatchSize.HasValue && this.State.BatchSize.Value <= iterationIndex)
               || inputCollection.Count() - 1 <= iterationIndex)
            {
                var outputCollection = await this.GetOutputCollectionAsync(cancellationToken);
                foreach (var iterationActivity in (await this.Context.Workflow.GetActivitiesAsync(this.Activity, cancellationToken))
                    .Where(a => a.Type == V1WorkflowActivityType.Iteration && a.Status == V1WorkflowActivityStatus.Completed && a.Output != null)
                    .ToList())
                {
                    outputCollection.Add(iterationActivity.Output!.ToObject()!);
                }
                var output = this.Activity.Input.ToObject();
                var expression = this.State.IterationParameter;
                if (string.IsNullOrWhiteSpace(expression))
                    expression = "item";
                if (expression.StartsWith("${"))
                    expression = expression[2..^1];
                expression = $". |= del(.{expression})";
                if (output != null)
                    output = await this.Context.EvaluateAsync(expression, output, cancellationToken);
                if (!string.IsNullOrWhiteSpace(this.State.OutputCollection))
                {
                    expression = this.State.OutputCollection.Trim();
                    var outputCollectionJson = await this.JsonSerializer.SerializeAsync(outputCollection, cancellationToken);
                    if (expression.StartsWith("${"))
                        expression = expression[2..^1];
                    expression = $"{expression} = {outputCollectionJson}";
                    output = await this.Context.EvaluateAsync(expression, output, cancellationToken);
                }
                await this.OnNextAsync(new V1WorkflowActivityCompletedIntegrationEvent(this.Activity.Id, output), cancellationToken);
                return;
            }
            iterationIndex += 1;
            var iterationParameterValue = inputCollection.ElementAt(iterationIndex);
            var input = DynamicObject.FromObject(await this.Context.Workflow.GetActivityStateDataAsync(this.Activity, cancellationToken))!;
            input.Set(this.State.IterationParameter!, iterationParameterValue);
            var metadata = new Dictionary<string, string>()
            {
                { V1WorkflowActivityMetadata.State, this.State.Name! },
                { V1WorkflowActivityMetadata.Iteration, iterationIndex.ToString() }
            };
            await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.Iteration, input.ToObject(), metadata, this.Activity, cancellationToken);
        }

        /// <summary>
        /// Handles the completion of the specified <see cref="IterationProcessor"/>
        /// </summary>
        /// <param name="processor">The <see cref="IterationProcessor"/> to handle the completion of</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnIterationCompletedAsync(IterationProcessor processor, CancellationToken cancellationToken)
        {
            var iterationIndex = int.Parse(processor.Activity.Metadata[V1WorkflowActivityMetadata.Iteration]);
            this.Processors.TryRemove(processor);
            processor.Dispose();
            var inputCollection = await this.GetInputCollectionAsync(cancellationToken);
            if ((this.State.BatchSize.HasValue && this.State.BatchSize.Value - 1 <= iterationIndex)
                || inputCollection.Count() - 1 <= iterationIndex)
            {
                await this.OnCompletedAsync(cancellationToken);
                return;
            }
            var childActivities = await this.Context.Workflow.GetOperativeActivitiesAsync(this.Activity, cancellationToken);
            foreach (var childActivity in childActivities
                .Where(p => p.Status == V1WorkflowActivityStatus.Pending))
            {
                var nextProcessor = this.CreateProcessorFor(childActivity);
                await nextProcessor.ProcessAsync(cancellationToken);
            }
        }

    }

}
