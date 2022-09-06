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

using CloudNative.CloudEvents;
using ConcurrentCollections;
using Microsoft.Extensions.Hosting;
using Synapse.Apis.Runtime;
using Synapse.Integration.Events.WorkflowActivities;
using System.Reactive.Linq;

namespace Synapse.Worker.Services
{

    /// <summary>
    /// Represents the default implementation of the <see cref="IWorkflowRuntime"/> interface
    /// </summary>
    public class WorkflowRuntime
        : BackgroundService, IWorkflowRuntime
    {

        private IDisposable? _ServerSignalStreamSubscription;
        private IDisposable? _OutboundEventStreamSubscription;

        /// <summary>
        /// Initializes a new <see cref="WorkflowRuntime"/>
        /// </summary>
        /// <param name="logger">The service used to perform logging</param>
        /// <param name="hostApplicationLifetime">The service used to handle the lifetime of the <see cref="WorkflowRuntime"/>'s host application</param>
        /// <param name="integrationEventBus">The service used to publish and subscribe to integration events</param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="synapseRuntimeApi">The service used to interact with the Synapse Runtime API</param>
        /// <param name="context">The current <see cref="IWorkflowRuntimeContext"/></param>
        public WorkflowRuntime(ILogger<WorkflowRuntime> logger, IHostApplicationLifetime hostApplicationLifetime, IIntegrationEventBus integrationEventBus,
            IWorkflowActivityProcessorFactory activityProcessorFactory, ISynapseRuntimeApi synapseRuntimeApi, IWorkflowRuntimeContext context)
        {
            this.Logger = logger;
            this.HostApplicationLifetime = hostApplicationLifetime;
            this.IntegrationEventBus = integrationEventBus;
            this.ActivityProcessorFactory = activityProcessorFactory;
            this.RuntimeApi = synapseRuntimeApi;
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
        /// Gets the service used to interact with the Synapse Runtime API
        /// </summary>
        protected ISynapseRuntimeApi RuntimeApi { get; }

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

        /// <summary>
        /// Gets the service used to publish and subscribe to integration events
        /// </summary>
        protected IIntegrationEventBus IntegrationEventBus { get; }

        /// <summary>
        /// Gets an <see cref="IObservable{T}"/> that represents the inbound, server <see cref="V1RuntimeSignal"/> stream
        /// </summary>
        protected IObservable<V1RuntimeSignal> ServerStream { get; private set; } = null!;

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            try
            {
                await this.Context.InitializeAsync(this.CancellationToken);
                this.ServerStream = this.RuntimeApi.Connect(this.Context.Workflow.Instance.Id).ToObservable();
                this._ServerSignalStreamSubscription = this.ServerStream.SubscribeAsync(this.OnServerSignalAsync, this.OnServerConnectionErrorAsync, this.OnDisconnectedFromServerAsync);
                this._OutboundEventStreamSubscription = this.IntegrationEventBus.OutboundStream.SubscribeAsync(this.OnPublishEventAsync);
                switch (this.Context.Workflow.Instance.Status)
                {
                    case V1WorkflowInstanceStatus.Pending:
                    case V1WorkflowInstanceStatus.Scheduled:
                    case V1WorkflowInstanceStatus.Starting:
                        await this.StartAsync();
                        break;
                    case V1WorkflowInstanceStatus.Resuming:
                        await this.ResumeAsync();
                        break;
                    default:
                        throw new InvalidOperationException($"The workflow instance '{this.Context.Workflow.Instance.Id}' is in an unexpected state '{this.Context.Workflow.Instance.Status}'");
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError("A critical exception occured while executing the workflow instance with id '{instanceId}': {ex}", EnvironmentVariables.Runtime.WorkflowInstanceId.Value, ex.ToString());
                try
                {
                    await this.Context.Workflow.FaultAsync(ex, this.CancellationToken);
                }
                catch (Exception iex)
                {
                    this.Logger.LogError("A critical exception occured while faulting the execution of workflow instance with id '{instanceId}': {ex}", EnvironmentVariables.Runtime.WorkflowInstanceId.Value, iex.ToString());
                    throw;
                }
                this.HostApplicationLifetime.StopApplication();
                throw;
            }
        }

        /// <summary>
        /// Starts the <see cref="WorkflowRuntime"/>'s execution
        /// </summary>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task StartAsync()
        {
            this.Logger.LogInformation("Starting the workflow's execution...");
            await this.Context.Workflow.StartAsync(this.CancellationToken);
            if (!this.Context.Workflow.Definition.TryGetStartState(out StateDefinition startState))
                throw new InvalidOperationException($"Failed to resolved the startup state for workflow '{this.Context.Workflow.Definition.Id}'");
            await this.Context.Workflow.TransitionToAsync(startState, this.CancellationToken);
            var metadata = new Dictionary<string, string>() { { V1WorkflowActivityMetadata.State, startState.Name } };
            var activity = await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.State, await this.Context.FilterInputAsync(startState, this.Context.Workflow.Instance.Input, this.CancellationToken), metadata, null, this.CancellationToken);
            var processor = this.CreateActivityProcessor(activity);
            await processor.ProcessAsync(this.CancellationToken);
        }

        /// <summary>
        /// Resumes the <see cref="WorkflowRuntime"/>'s execution
        /// </summary>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task ResumeAsync()
        {
            this.Logger.LogInformation("Resuming the workflow's execution...");
            await this.Context.Workflow.StartAsync(this.CancellationToken);
            var activities = await this.Context.Workflow.GetOperativeActivitiesAsync(this.CancellationToken);
            if (!activities.Any())
            {
                activities = await this.Context.Workflow.GetActivitiesAsync(this.CancellationToken);
                var lastTopLevelActivity = activities
                    .OrderBy(a => a.CreatedAt)
                    .LastOrDefault(a => a.Type == V1WorkflowActivityType.State || a.Type == V1WorkflowActivityType.Transition || a.Type == V1WorkflowActivityType.End);
                if (lastTopLevelActivity == null)
                {
                    await this.StartAsync();
                    return;
                }
                switch (lastTopLevelActivity.Type)
                {
                    case V1WorkflowActivityType.Start:
                        await this.ResumeFromStartAsync(lastTopLevelActivity);
                        break;
                    case V1WorkflowActivityType.State:
                        await this.ResumeFromStateAsync(lastTopLevelActivity);
                        break;
                    case V1WorkflowActivityType.Transition:
                        await this.ResumeFromTransitionAsync(lastTopLevelActivity);
                        break;
                    case V1WorkflowActivityType.End:
                        await this.ResumeFromEndAsync(lastTopLevelActivity);
                        break;
                    default:
                        throw new NotSupportedException($"The specified {nameof(V1WorkflowActivityType)} '{nameof(lastTopLevelActivity.Type)}' is not supported in this context");
                }
            }
            foreach(var activity in activities)
            {
                var processor = this.CreateActivityProcessor(activity);
                await processor.ProcessAsync(this.CancellationToken);
            }
        }

        /// <summary>
        /// Resumes the execution of the workflow from the <see cref="StartDefinition"/>
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to resume from</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual Task ResumeFromStartAsync(V1WorkflowActivity activity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            //todo: implement
            throw new NotImplementedException();
        }

        /// <summary>
        /// Resumes the execution of the workflow from the specified <see cref="StateDefinition"/>
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to resume from</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task ResumeFromStateAsync(V1WorkflowActivity activity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            if (!activity.Metadata.TryGetValue(V1WorkflowActivityMetadata.State, out var stateName))
                throw new ArgumentException($"The specified activity '{activity.Id}' is missing the required metadata field '{V1WorkflowActivityMetadata.State}'");
            if (!this.Context.Workflow.Definition.TryGetState(stateName, out var state))
                throw new NullReferenceException($"Failed to find the workflow state with the specified name '{stateName}'");
            V1WorkflowActivity nextActivity;
            var metadata = new Dictionary<string, string>() { { V1WorkflowActivityMetadata.State, state.Name } };
            if (state.Transition != null
                || !string.IsNullOrWhiteSpace(state.TransitionToStateName))
                nextActivity = await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.Transition, activity.Output!.ToObject()!, metadata, null, this.CancellationToken);
            else if (state.End != null
                || state.IsEnd)
                nextActivity = await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.End, activity.Output!.ToObject()!, metadata, null, this.CancellationToken);
            else
                throw new InvalidOperationException($"The state '{state.Name}' must declare a transition definition or an end definition for it is part of the main execution logic of the workflow '{this.Context.Workflow.Definition.Id}'");
            var processor = this.CreateActivityProcessor(nextActivity);
            await processor.ProcessAsync(this.CancellationToken);
        }

        /// <summary>
        /// Resumes the execution of the workflow from the specified <see cref="TransitionDefinition"/>
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to resume from</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task ResumeFromTransitionAsync(V1WorkflowActivity activity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            if (!activity.Metadata.TryGetValue(V1WorkflowActivityMetadata.State, out var stateName))
                throw new ArgumentException($"The specified activity '{activity.Id}' is missing the required metadata field '{V1WorkflowActivityMetadata.State}'");
            if (!this.Context.Workflow.Definition.TryGetState(stateName, out var state))
                throw new NullReferenceException($"Failed to find the workflow state with the specified name '{stateName}'");
            TransitionDefinition transition;
            if (state is SwitchStateDefinition @switch)
            {
                if (!activity.Metadata.TryGetValue(V1WorkflowActivityMetadata.Case, out var caseName))
                    throw new ArgumentException($"The specified activity '{activity.Id}' is missing the required metadata field '{V1WorkflowActivityMetadata.Case}'");
                if (!@switch.TryGetCase(caseName, out SwitchCaseDefinition dataCondition))
                    throw new NullReferenceException($"Failed to find a condition with the specified name '{caseName}'");
                transition = dataCondition.Transition!;
                if (transition == null)
                    transition = new() { NextState = dataCondition.TransitionToStateName! };
            }
            else
            {
                transition = state.Transition!;
                if (transition == null)
                    transition = new() { NextState = state.TransitionToStateName! };
            }
            if (!this.Context.Workflow.Definition.TryGetState(transition.NextState, out StateDefinition nextState))
                throw new NullReferenceException($"Failed to find a state with name '{transition.NextState}' in workflow '{this.Context.Workflow.Definition.Id} {this.Context.Workflow.Definition.Version}'");
            await this.Context.Workflow.TransitionToAsync(nextState, this.CancellationToken);
            var metadata = new Dictionary<string, string>() { { V1WorkflowActivityMetadata.State, nextState.Name } };
            var nextActivity = await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.State, activity.Output, metadata, null, this.CancellationToken);
            var processor = this.CreateActivityProcessor(nextActivity);
            await processor.ProcessAsync(this.CancellationToken);
        }

        /// <summary>
        /// Resumes the execution of the workflow from the <see cref="EndDefinition"/>
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to resume from</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task ResumeFromEndAsync(V1WorkflowActivity activity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            await this.Context.Workflow.SetOutputAsync(activity.Output, this.CancellationToken);
        }

        /// <summary>
        /// Suspends the <see cref="WorkflowRuntime"/>'s execution
        /// </summary>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task SuspendAsync()
        {
            this.Logger.LogInformation("Suspending execution...");
            foreach (var processor in this.Processors.ToList())
            {
                await processor.SuspendAsync(this.CancellationToken);
            }
            await this.Context.Workflow.SuspendAsync(this.CancellationToken);
            this.Logger.LogInformation("Execution has been successfully suspended. The application will now shutdown....");
            this.HostApplicationLifetime.StopApplication();
        }

        /// <summary>
        /// Cancels the <see cref="WorkflowRuntime"/>'s execution
        /// </summary>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task CancelAsync()
        {
            this.Logger.LogInformation("Cancelling execution...");
            foreach (var processor in this.Processors.ToList())
            {
                await processor.TerminateAsync(this.CancellationToken);
            }
            await this.Context.Workflow.CancelAsync(this.CancellationToken);
            this.Logger.LogInformation("Execution has been successfully cancelled. The application will now shutdown....");
            this.HostApplicationLifetime.StopApplication();
        }

        /// <summary>
        /// Creates a new child <see cref="IWorkflowActivityProcessor"/> for the specified <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to create a child <see cref="IWorkflowActivityProcessor"/> for</param>
        protected virtual IWorkflowActivityProcessor CreateActivityProcessor(V1WorkflowActivity activity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            IWorkflowActivityProcessor processor = this.ActivityProcessorFactory.Create(activity);
            switch (processor)
            {
                case IStateProcessor stateProcessor:
                    processor.OfType<V1WorkflowActivityCompletedIntegrationEvent>().SubscribeAsync
                    (
                        async e => await this.OnStateCompletedAsync(stateProcessor, e),
                        async ex => await this.OnStateProcessingErrorAsync(stateProcessor, ex),
                        async () => await this.OnActivityProcessingCompletedAsync(stateProcessor)
                    );
                    break;
                case ITransitionProcessor transitionProcessor:
                    processor.OfType<V1WorkflowActivityCompletedIntegrationEvent>().SubscribeAsync
                    (
                        async e => await this.OnTransitionCompletedAsync(transitionProcessor, e),
                        async ex => await this.OnActivityProcessingErrorAsync(transitionProcessor, ex),
                        async () => await this.OnActivityProcessingCompletedAsync(transitionProcessor)
                    );
                    break;
                case IEndProcessor endProcessor:
                    processor.OfType<V1WorkflowActivityCompletedIntegrationEvent>().SubscribeAsync
                    (
                        async e => await this.OnEndCompletedAsync(endProcessor, e),
                        async ex => await this.OnActivityProcessingErrorAsync(endProcessor, ex),
                        async () => await this.OnCompletedAsync(endProcessor)
                    );
                    break;
            }
            this.Processors.Add(processor);
            return processor;
        }

        /// <summary>
        /// Handles the specified <see cref="V1RuntimeSignal"/>
        /// </summary>
        /// <param name="signal">The <see cref="V1RuntimeSignal"/> to handle</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnServerSignalAsync(V1RuntimeSignal signal)
        {
            if (signal == null)
                return;
            try
            {
                switch (signal.Type)
                {
                    case V1RuntimeSignalType.Correlate:
                        var correlationContext = signal.Data!.ToObject<V1CorrelationContext>();
                        foreach (var e in correlationContext.PendingEvents)
                        {
                            this.IntegrationEventBus.InboundStream.OnNext(e.ToCloudEvent());
                        }
                        break;
                    case V1RuntimeSignalType.Suspend:
                        await this.SuspendAsync();
                        break;
                    case V1RuntimeSignalType.Cancel:
                        await this.CancelAsync();
                        break;
                    default:
                        this.Logger.LogWarning("The specified server signal type '{signal.Type}' is not supported", signal.Type);
                        break;
                }
            }
            catch(Exception ex)
            {
                this.Logger.LogError("An error occured while handling a server runtime signal: {ex}", ex.ToString());
                await this.Context.Workflow.FaultAsync(ex, this.CancellationToken);
                this.HostApplicationLifetime.StopApplication();
            }
        }

        /// <summary>
        /// Handles the specified <see cref="Exception"/> that occured during communication with the server
        /// </summary>
        /// <param name="ex">The <see cref="Exception"/> to handle</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnServerConnectionErrorAsync(Exception ex)
        {
            this.Logger.LogCritical("The connection to the remote server has been lost");
            this.HostApplicationLifetime.StopApplication();
            await Task.CompletedTask;
        }

        /// <summary>
        /// Handles the <see cref="WorkflowRuntime"/>'s disconnection from the server
        /// </summary>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnDisconnectedFromServerAsync()
        {
            this.Logger.LogCritical("The connection to the remote server has been lost");
            this.HostApplicationLifetime.StopApplication();
            await Task.CompletedTask;
        }

        /// <summary>
        /// Publishes the specified <see cref="V1Event"/>
        /// </summary>
        /// <param name="e">The <see cref="V1Event"/> to publish</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnPublishEventAsync(CloudEvent e)
        {
            try
            {
                //todo: publish the event to the server
                await Task.CompletedTask;
            }
            catch(Exception ex)
            {
                this.Logger.LogError("An error occured while publishing the specified event: {ex}", ex.ToString());
            }
        }

        /// <summary>
        /// Handles the completion of a state
        /// </summary>
        /// <param name="processor">The <see cref="IStateProcessor"/> that has finished processing the state</param>
        /// <param name="e">The <see cref="V1WorkflowActivityCompletedIntegrationEvent"/> that describes the processed state's output</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnStateCompletedAsync(IStateProcessor processor, V1WorkflowActivityCompletedIntegrationEvent e)
        {
            var metadata = new Dictionary<string, string>() { { V1WorkflowActivityMetadata.State, processor.State.Name } };
            if (processor.Activity.Metadata.TryGetValue(V1WorkflowActivityMetadata.CompensationSource, out var compensationSource))
                metadata.Add(V1WorkflowActivityMetadata.CompensationSource, compensationSource);
            if (processor.State is SwitchStateDefinition switchState)
            {
                if (!processor.Activity.Metadata.TryGetValue(V1WorkflowActivityMetadata.Case, out var caseName))
                    throw new InvalidOperationException($"Failed to retrieve the required switch state metadata with key '{V1WorkflowActivityMetadata.Case}'");
                if (!switchState.TryGetCase(caseName, out SwitchCaseDefinition switchCase))
                    throw new InvalidOperationException($"Failed to find a case with name '{caseName}' in the state '{processor.State.Name}' of workflow '{this.Context.Workflow.Definition.Id}'");
                metadata.Add(V1WorkflowActivityMetadata.Case, caseName);
                V1WorkflowActivity activity = switchCase.Type switch
                {
                    ConditionType.End => await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.End, e.Output, metadata, null, this.CancellationToken),
                    ConditionType.Transition => await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.Transition, e.Output, metadata, null, this.CancellationToken),
                    _ => throw new NotSupportedException($"The specified condition type '{switchCase.Type}' is not supported in this context"),
                };
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                this.CreateActivityProcessor(activity);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            else
            {
                if (processor.State.Transition != null
                    || !string.IsNullOrWhiteSpace(processor.State.TransitionToStateName))
                    await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.Transition, e.Output!.ToObject()!, metadata, null, this.CancellationToken);
                else if (processor.State.End != null
                    || processor.State.IsEnd)
                    await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.End, e.Output!.ToObject()!, metadata, null, this.CancellationToken);
                else
                    throw new InvalidOperationException($"The state '{processor.State.Name}' must declare a transition definition or an end definition for it is part of the main execution logic of the workflow '{this.Context.Workflow.Definition.Id}'");
                foreach (var activity in await this.Context.Workflow.GetOperativeActivitiesAsync(this.CancellationToken))
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    this.CreateActivityProcessor(activity);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
        }

        /// <summary>
        /// Handles an <see cref="Exception"/> that has occured during the processing of a <see cref="StateDefinition"/>
        /// </summary>
        /// <param name="processor">The <see cref="IWorkflowActivityProcessor"/> that has thrown the <see cref="Exception"/> to handle</param>
        /// <param name="ex">The <see cref="Exception"/> to handle</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnStateProcessingErrorAsync(IStateProcessor processor, Exception ex)
        {
            if (string.IsNullOrWhiteSpace(processor.State.CompensatedBy))
            {
                await this.OnActivityProcessingErrorAsync(processor, ex);
                return;
            }
            await this.Context.Workflow.CompensateActivityAsync(processor.Activity, this.CancellationToken);
            var metadata = new Dictionary<string, string>()
            {
                { V1WorkflowActivityMetadata.State, processor.State.Name },
                { V1WorkflowActivityMetadata.NextState, processor.State.CompensatedBy },
                { V1WorkflowActivityMetadata.CompensationSource, processor.Activity.Id }
            };
            var activity = await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.Transition, await this.Context.Workflow.GetActivityStateDataAsync(processor.Activity, this.CancellationToken), metadata, null, this.CancellationToken);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            this.CreateActivityProcessor(activity);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            await this.OnActivityProcessingCompletedAsync(processor);
        }

        /// <summary>
        /// Handles the next <see cref="V1WorkflowActivityCompletedIntegrationEvent"/>
        /// </summary>
        /// <param name="processor">The <see cref="IWorkflowActivityProcessor"/> that has produced an <see cref="V1WorkflowActivityCompletedIntegrationEvent"/></param>
        /// <param name="e">The <see cref="V1WorkflowActivityCompletedIntegrationEvent"/> to process</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnTransitionCompletedAsync(ITransitionProcessor processor, V1WorkflowActivityCompletedIntegrationEvent e)
        {
            if (!this.Context.Workflow.Definition.TryGetState(processor.Transition.NextState, out StateDefinition nextState))
                throw new NullReferenceException($"Failed to find a state with name '{processor.Transition.NextState}' in workflow '{this.Context.Workflow.Definition.Id} {this.Context.Workflow.Definition.Version}'");
            await this.Context.Workflow.TransitionToAsync(nextState, this.CancellationToken);
            var metadata = new Dictionary<string, string>() 
            { 
                { V1WorkflowActivityMetadata.State, nextState.Name }
            };
            if (processor.Activity.Metadata.TryGetValue(V1WorkflowActivityMetadata.CompensationSource, out var compensationSource))
                metadata.Add(V1WorkflowActivityMetadata.CompensationSource, compensationSource);
            var activity = await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.State, e.Output, metadata, null, this.CancellationToken);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            this.CreateActivityProcessor(activity);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        /// <summary>
        /// Handles the completion of the specified <see cref="IEndProcessor"/>
        /// </summary>
        /// <param name="processor">The <see cref="IEndProcessor"/> that has produced the <see cref="V1WorkflowActivityCompletedIntegrationEvent"/></param>
        /// <param name="e">The <see cref="V1WorkflowActivityCompletedIntegrationEvent"/> to handle</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnEndCompletedAsync(IEndProcessor processor, V1WorkflowActivityCompletedIntegrationEvent e)
        {
            if (processor.Activity.Metadata.TryGetValue(V1WorkflowActivityMetadata.CompensationSource, out var compensationSource))
                await this.Context.Workflow.MarkActivityAsCompensatedAsync(compensationSource, this.CancellationToken);
            await this.Context.Workflow.SetOutputAsync(e.Output, this.CancellationToken);
        }

        /// <summary>
        /// Handles an <see cref="Exception"/> that has occured during the processing of a <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <param name="processor">The <see cref="IWorkflowActivityProcessor"/> that has thrown the <see cref="Exception"/> to handle</param>
        /// <param name="ex">The <see cref="Exception"/> to handle</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnActivityProcessingErrorAsync(IWorkflowActivityProcessor processor, Exception ex)
        {
            try
            {
                this.Logger.LogWarning("An error occured while executing the workflow instance: {ex}", ex.ToString());
                await this.Context.Workflow.FaultAsync(ex, this.CancellationToken);
                this.HostApplicationLifetime.StopApplication();
            }
            catch (Exception cex)
            {
                this.Logger.LogError("A critical exception occured while faulting the execution of the workflow instance: {ex}", cex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Handles the completion of the specified <see cref="IWorkflowActivityProcessor"/>
        /// </summary>
        /// <param name="processor">The <see cref="IWorkflowActivityProcessor"/> to handle the completion of</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnActivityProcessingCompletedAsync(IWorkflowActivityProcessor processor)
        {
            this.Processors.TryRemove(processor);
            processor.Dispose();
            foreach (IWorkflowActivityProcessor childProcessor in this.Processors)
            {
                await childProcessor.ProcessAsync(this.CancellationToken);
            }
        }

        /// <summary>
        /// Handles the completion of the specified <see cref="IEndProcessor"/>
        /// </summary>
        /// <param name="processor">The <see cref="IEndProcessor"/> to handle the completion of</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnCompletedAsync(IEndProcessor processor)
        {
            await this.OnActivityProcessingCompletedAsync(processor);
            this.Logger.LogInformation("Workflow executed");
            this.HostApplicationLifetime.StopApplication();
        }

        /// <inheritdoc/>
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
        public override void Dispose()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
        {
            this._ServerSignalStreamSubscription?.Dispose();
            this._OutboundEventStreamSubscription?.Dispose();
            base.Dispose();
        }

    }

}
