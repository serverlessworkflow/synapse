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
using ConcurrentCollections;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Synapse.Integration.Events.WorkflowActivities;
using System.Reactive.Linq;

namespace Synapse.Runtime.Services
{

    /// <summary>
    /// Represents the default implementation of the <see cref="IWorkflowRuntime"/> interface
    /// </summary>
    public class WorkflowRuntime
        : BackgroundService, IWorkflowRuntime
    {

        /// <summary>
        /// Initializes a new <see cref="WorkflowRuntime"/>
        /// </summary>
        /// <param name="logger">The service used to perform logging</param>
        /// <param name="hostApplicationLifetime">The service used to handle the lifetime of the <see cref="WorkflowRuntime"/>'s host application</param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="context">The current <see cref="IWorkflowRuntimeContext"/></param>
        public WorkflowRuntime(ILogger<WorkflowRuntime> logger, IHostApplicationLifetime hostApplicationLifetime,
            IWorkflowActivityProcessorFactory activityProcessorFactory, IWorkflowRuntimeContext context)
        {
            this.Logger = logger;
            this.HostApplicationLifetime = hostApplicationLifetime;
            this.ActivityProcessorFactory = activityProcessorFactory;
            this.Context = context;
            this.CancellationTokenSource = null!;
        }

        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the service used to handle the lifetime of the <see cref="WorkflowRuntime"/>'s host application
        /// </summary>
        protected IHostApplicationLifetime HostApplicationLifetime { get; }

        /// <summary>
        /// Gets the service used to create <see cref="IWorkflowActivityProcessor"/>s
        /// </summary>
        protected IWorkflowActivityProcessorFactory ActivityProcessorFactory { get; }

        /// <summary>
        /// Gets the current <see cref="IWorkflowRuntimeContext"/>
        /// </summary>
        protected IWorkflowRuntimeContext Context { get; }

        /// <summary>
        /// Gets the <see cref="WorkflowRuntime"/>'s <see cref="System.Threading.CancellationTokenSource"/>
        /// </summary>
        protected CancellationTokenSource CancellationTokenSource { get; private set; }

        /// <summary>
        /// Gets the <see cref="WorkflowRuntime"/>'s <see cref="System.Threading.CancellationToken"/>
        /// </summary>
        protected CancellationToken CancellationToken => this.CancellationTokenSource.Token;

        /// <summary>
        /// Gets a <see cref="ConcurrentHashSet{T}"/> containing all child <see cref="IWorkflowActivityProcessor"/>s
        /// </summary>
        protected ConcurrentHashSet<IWorkflowActivityProcessor> Processors { get; } = new ConcurrentHashSet<IWorkflowActivityProcessor>();

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            try
            {
                await this.Context.InitializeAsync(this.CancellationToken);
                switch (this.Context.Workflow.Instance.Status)
                {
                    case V1WorkflowInstanceStatus.Pending:
                    case V1WorkflowInstanceStatus.Scheduled:
                    case V1WorkflowInstanceStatus.Starting:
                    case V1WorkflowInstanceStatus.Resuming:
                        await this.Context.Workflow.StartAsync(this.CancellationToken);
                        if (!this.Context.Workflow.Definition.TryGetStartState(out StateDefinition startState))
                            throw new InvalidOperationException($"Failed to resolved the startup state for workflow '{this.Context.Workflow.Definition.Id}'");
                        await this.Context.Workflow.TransitionToAsync(startState, this.CancellationToken);
                        var metadata = new Dictionary<string, string>()
                        {
                            { V1WorkflowActivityMetadata.State, startState.Name }
                        };
                        await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.State, await this.Context.FilterInputAsync(startState, this.Context.Workflow.Instance.Input, this.CancellationToken), metadata, null, this.CancellationToken);
                        break;
                    default:
                        throw new InvalidOperationException($"The workflow instance '{this.Context.Workflow.Instance.Id}' is in an unexpected state '{this.Context.Workflow.Instance.Status}'");
                }
                foreach (var activity in await this.Context.Workflow.GetOperativeActivitiesAsync(this.CancellationToken))
                {
                    var processor = this.CreateActivityProcessor(activity);
                    await processor.ProcessAsync(this.CancellationToken);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    await this.Context.Workflow.FaultAsync(ex, this.CancellationToken);
                }
                catch (Exception iex)
                {
                    this.Logger.LogError("A critical exception occured while faulting the execution of workflow instance '{instanceId}': {ex}", ""/*todo: EnvironmentVariables.Workflows.Instance.Value*/, iex.ToString());
                    throw;
                }
                this.HostApplicationLifetime.StopApplication();
                throw;
            }
        }

        /// <summary>
        /// Creates a new child <see cref="IWorkflowActivityProcessor"/> for the specified <see cref="V1WorkflowActivityDto"/>
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivityDto"/> to create a child <see cref="IWorkflowActivityProcessor"/> for</param>
        protected virtual IWorkflowActivityProcessor CreateActivityProcessor(V1WorkflowActivityDto activity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            CancellationToken cancellationToken = this.CancellationTokenSource.Token;
            IWorkflowActivityProcessor processor = this.ActivityProcessorFactory.Create(activity);
            switch (processor)
            {
                case IStateProcessor stateProcessor:
                    processor.OfType<V1WorkflowActivityCompletedIntegrationEvent>().SubscribeAsync
                    (
                        async e => await this.OnStateCompletedAsync(stateProcessor, e, cancellationToken),
                        async ex => await this.OnProcessErrorAsync(stateProcessor, ex, cancellationToken),
                        async () => await this.OnProcessCompletedAsync(stateProcessor, cancellationToken)
                    );
                    break;
                case ITransitionProcessor transitionProcessor:
                    processor.OfType<V1WorkflowActivityCompletedIntegrationEvent>().SubscribeAsync
                    (
                        async e => await this.OnTransitionCompletedAsync(transitionProcessor, e, cancellationToken),
                        async ex => await this.OnProcessErrorAsync(transitionProcessor, ex, cancellationToken),
                        async () => await this.OnProcessCompletedAsync(transitionProcessor, cancellationToken)
                    );
                    break;
                case IEndProcessor endProcessor:
                    processor.OfType<V1WorkflowActivityCompletedIntegrationEvent>().SubscribeAsync
                    (
                        async e => await this.OnEndCompletedAsync(endProcessor, e, cancellationToken),
                        async ex => await this.OnProcessErrorAsync(endProcessor, ex, cancellationToken),
                        async () => await this.OnCompletedAsync(endProcessor, cancellationToken)
                    );
                    break;
            }
            this.Processors.Add(processor);
            return processor;
        }

        protected virtual async Task OnStateCompletedAsync(IStateProcessor processor, V1WorkflowActivityCompletedIntegrationEvent e, CancellationToken cancellationToken)
        {
            var metadata = new Dictionary<string, string>() { { V1WorkflowActivityMetadata.State, processor.State.Name } };
            if (processor.State is SwitchStateDefinition switchState)
            {
                if (!processor.Activity.Metadata.TryGetValue(V1WorkflowActivityMetadata.Case, out var caseName))
                    throw new InvalidOperationException($"Failed to retrieve the required switch state metadata with key '{V1WorkflowActivityMetadata.Case}'");
                if (!switchState.TryGetCase(caseName, out SwitchCaseDefinition switchCase))
                    throw new InvalidOperationException($"Failed to find a case with name '{caseName}' in the state '{processor.State.Name}' of workflow '{this.Context.Workflow.Definition.Id}'");
                metadata.Add(V1WorkflowActivityMetadata.Case, caseName);
                switch (switchCase.Type)
                {
                    case ConditionType.End:
                        await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.End, e.Output, metadata, null, cancellationToken);
                        break;
                    case ConditionType.Transition:
                        await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.Transition, e.Output, metadata, null, cancellationToken);
                        break;
                    default:
                        throw new NotSupportedException($"The specified condition type '{switchCase.Type}' is not supported in this context");
                }
            }
            else
            {
                if (processor.State.Transition != null
                    && string.IsNullOrWhiteSpace(processor.State.TransitionToStateName))
                    await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.Transition, e.Output, metadata, null, cancellationToken);
                else if (processor.State.End != null
                    || processor.State.IsEnd)
                    await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.End, e.Output, metadata, null, cancellationToken);
                else
                    throw new InvalidOperationException($"The state '{processor.State.Name}' must declare a transition definition or an end definition for it is part of the main execution logic of the workflow '{this.Context.Workflow.Definition.Id}'");
                foreach (var activity in await this.Context.Workflow.GetOperativeActivitiesAsync(cancellationToken))
                {
                    this.CreateActivityProcessor(activity);
                }
            }
        }

        /// <summary>
        /// Handles the next <see cref="V1WorkflowExecutionResult"/> returned by a <see cref="IWorkflowActivityProcessor"/>
        /// </summary>
        /// <param name="processor">The <see cref="IWorkflowActivityProcessor"/> that has returned an <see cref="V1WorkflowExecutionResult"/></param>
        /// <param name="result">The <see cref="V1WorkflowExecutionResult"/> to process</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnTransitionCompletedAsync(ITransitionProcessor processor, V1WorkflowActivityCompletedIntegrationEvent e, CancellationToken cancellationToken)
        {
            if (!this.Context.Workflow.Definition.TryGetState(processor.Transition.NextState, out StateDefinition nextState))
                throw new NullReferenceException($"Failed to find a state with name '{processor.Transition.NextState}' in workflow '{this.Context.Workflow.Definition.Id} {this.Context.Workflow.Definition.Version}'");
            await this.Context.Workflow.TransitionToAsync(nextState, cancellationToken);
            var metadata = new Dictionary<string, string>() { { V1WorkflowActivityMetadata.State, nextState.Name } };
            var activity = await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.State, e.Output, metadata, null, cancellationToken);
            this.CreateActivityProcessor(activity);
        }

        protected virtual async Task OnEndCompletedAsync(IEndProcessor processor, V1WorkflowActivityCompletedIntegrationEvent e, CancellationToken cancellationToken)
        {
            await this.Context.Workflow.SetOutputAsync(e.Output, cancellationToken);
        }

        protected virtual async Task OnProcessErrorAsync(IWorkflowActivityProcessor processor, Exception ex, CancellationToken cancellationToken)
        {
            try
            {
                this.Logger.LogWarning("An error occured while executing the workflow instance: {ex}", ex.ToString());
                await this.Context.Workflow.FaultAsync(ex, cancellationToken);
                this.HostApplicationLifetime.StopApplication();
            }
            catch (Exception cex)
            {
                this.Logger.LogError("A critical exception occured while faulting the execution of the workflow instance: {ex}", cex.ToString());
                throw;
            }
        }

        protected virtual async Task OnProcessCompletedAsync(IWorkflowActivityProcessor processor, CancellationToken cancellationToken)
        {
            this.Processors.TryRemove(processor);
            processor.Dispose();
            foreach (IWorkflowActivityProcessor childProcessor in this.Processors)
            {
                await childProcessor.ProcessAsync(cancellationToken);
            }
        }

        protected virtual async Task OnCompletedAsync(IEndProcessor processor, CancellationToken cancellationToken)
        {
            await this.OnProcessCompletedAsync(processor, cancellationToken);
            this.Logger.LogInformation("Workflow executed");
            this.HostApplicationLifetime.StopApplication();
        }

    }

}
