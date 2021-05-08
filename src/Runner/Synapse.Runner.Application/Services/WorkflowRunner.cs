using CloudNative.CloudEvents;
using ConcurrentCollections;
using k8s.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Rest;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Domain.Models;
using Synapse.Runner.Application.Configuration;
using System;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Synapse.Runner.Application.Services
{

    /// <summary>
    /// Represents the default implementation of the <see cref="IWorkflowRunner"/> interface
    /// </summary>
    public class WorkflowRunner
        : BackgroundService, IWorkflowRunner
    {

        /// <summary>
        /// Initializes a new <see cref="WorkflowRunner"/>
        /// </summary>
        /// <param name="logger">The service used to perform logging</param>
        /// <param name="applicationLifetime">The service used to manage the application's lifetime</param>
        /// <param name="executionContext">The current <see cref="IWorkflowExecutionContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="applicationOptions">The service used to access the current <see cref="Configuration.ApplicationOptions"/></param>
        /// <param name="cloudEventFormatter">The service used to format <see cref="CloudEvent"/>s</param>
        /// <param name="cloudEventStream">The <see cref="Subject{T}"/> used to monitor consumed <see cref="CloudEvent"/>s</param>
        public WorkflowRunner(ILogger<WorkflowRunner> logger, IHostApplicationLifetime applicationLifetime, IWorkflowExecutionContext executionContext, IWorkflowActivityProcessorFactory activityProcessorFactory, 
            IOptions<ApplicationOptions> applicationOptions, ICloudEventFormatter cloudEventFormatter, Subject<CloudEvent> cloudEventStream)
        {
            this.Logger = logger;
            this.ApplicationLifetime = applicationLifetime;
            this.ExecutionContext = executionContext;
            this.ActivityProcessorFactory = activityProcessorFactory;
            this.ApplicationOptions = applicationOptions.Value;
            this.CloudEventFormatter = cloudEventFormatter;
            this.CloudEventStream = cloudEventStream;
        }
        
        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the service used to manage the application's lifetime
        /// </summary>
        protected IHostApplicationLifetime ApplicationLifetime { get; }

        /// <summary>
        /// Gets the current <see cref="Configuration.ApplicationOptions"/>
        /// </summary>
        protected ApplicationOptions ApplicationOptions { get; }

        /// <summary>
        /// Gets the current <see cref="IWorkflowExecutionContext"/>
        /// </summary>
        protected IWorkflowExecutionContext ExecutionContext { get; }

        /// <summary>
        /// Gets the service used to create <see cref="IWorkflowActivityProcessor"/>s
        /// </summary>
        protected IWorkflowActivityProcessorFactory ActivityProcessorFactory { get; }

        /// <summary>
        /// Gets the service used to format <see cref="CloudEvent"/>s
        /// </summary>
        protected ICloudEventFormatter CloudEventFormatter { get; }

        /// <summary>
        /// Gets the <see cref="Subject{T}"/> used to monitor consumed <see cref="CloudEvent"/>s
        /// </summary>
        protected Subject<CloudEvent> CloudEventStream { get; }

        /// <summary>
        /// Gets the <see cref="WorkflowRunner"/>'s <see cref="CancellationTokenSource"/>
        /// </summary>
        protected CancellationTokenSource CancellationTokenSource { get; private set; }

        /// <summary>
        /// Gets a <see cref="ConcurrentHashSet{T}"/> containing all child <see cref="IWorkflowActivityProcessor"/>s
        /// </summary>
        protected ConcurrentHashSet<IWorkflowActivityProcessor> Processors { get; } = new ConcurrentHashSet<IWorkflowActivityProcessor>();

        /// <inheritdoc/>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            this.Logger.LogInformation(SynapseRunnerConstants.Logging.GetHeader(SynapseConstants.EnvironmentVariables.Workflows.Id.Value, SynapseConstants.EnvironmentVariables.Workflows.Version.Value, SynapseConstants.EnvironmentVariables.Workflows.Instance.Value));
            _ = this.RunWorkflowAsync(this.CancellationTokenSource.Token);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Runs the <see cref="V1WorkflowInstance"/>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task RunWorkflowAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await this.ExecutionContext.InitializeAsync(cancellationToken);
                switch (this.ExecutionContext.Instance.Status.Type)
                {
                    case V1WorkflowActivityStatus.Deployed:
                        await this.ExecutionContext.StartWorkflowAsync(cancellationToken);
                        if (!this.ExecutionContext.Definition.Spec.Definition.TryGetStartupState(out StateDefinition startState))
                            throw new InvalidOperationException($"Failed to start the workflow '{this.ExecutionContext.Definition.Spec.Definition.Id}:{this.ExecutionContext.Definition.Spec.Definition.Version}' because it does not define exactly one start state");
                        await this.ExecutionContext.TransitionToAsync(startState, cancellationToken);
                        await this.ExecutionContext.CreateActivityAsync(V1WorkflowActivity.State(this.ExecutionContext.Instance, startState, this.ExecutionContext.ExpressionEvaluator.FilterInput(startState, this.ExecutionContext.Instance.Spec.Input)), cancellationToken);
                        break;
                    case V1WorkflowActivityStatus.Executing:
                        break;
                    case V1WorkflowActivityStatus.Suspended:
                        await this.ExecutionContext.ExecuteWorkflowAsync(this.CancellationTokenSource.Token);
                        break;
                    default:
                        throw new InvalidOperationException($"The workflow instance '{this.ExecutionContext.Instance.Name()}' is in an unexpected state '{this.ExecutionContext.Instance.Status.Type}'");
                }
                foreach (V1WorkflowActivity activity in await this.ExecutionContext.ListChildActivitiesAsync(cancellationToken))
                {
                    IWorkflowActivityProcessor processor = this.CreateProcessorFor(activity);
                    await processor.ProcessAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                if (ex is HttpOperationException httpEx)
                    this.Logger.LogError($"An error occured while starting the workflow runner: the Kubernetes API returned an non-success status code '{{statusCode}}''{Environment.NewLine}Response content: {{responseContent}}{Environment.NewLine}Details: {{ex}}", httpEx.Response.StatusCode, httpEx.Response.Content, ex.ToString());
                else
                    this.Logger.LogError($"An error occured while starting the workflow runner:{Environment.NewLine}{{ex}}", ex.ToString());
                if(this.ExecutionContext?.Instance != null)
                    await this.ExecutionContext.FaultWorkflowAsync(ex, cancellationToken);   
                throw;
            }
        }

        /// <summary>
        /// Creates a new child <see cref="IWorkflowActivityProcessor"/> for the specified <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to create a child <see cref="IWorkflowActivityProcessor"/> for</param>
        protected virtual IWorkflowActivityProcessor CreateProcessorFor(V1WorkflowActivity activity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            CancellationToken cancellationToken = this.CancellationTokenSource.Token;
            IWorkflowActivityProcessor processor = this.ActivityProcessorFactory.Create(activity);
            switch (processor)
            {
                case IStateProcessor stateProcessor:
                    processor.SubscribeAsync
                    (
                        async result => await this.OnStateResultAsync(stateProcessor, result, cancellationToken),
                        async ex => await this.OnStateErrorAsync(stateProcessor, ex, cancellationToken),
                        async () => await this.OnStateCompletedAsync(stateProcessor, cancellationToken)
                    );
                    break;
                case ITransitionProcessor transitionProcessor:
                    processor.SubscribeAsync
                    (
                        async result => await this.OnTransitionResultAsync(transitionProcessor, result, cancellationToken),
                        async ex => await this.OnTransitionErrorAsync(transitionProcessor, ex, cancellationToken),
                        async () => await this.OnTransitionCompletedAsync(transitionProcessor, cancellationToken)
                    );
                    break;
                case IEndProcessor endProcessor:
                    processor.SubscribeAsync
                    (
                        async result => await this.OnResultAsync(result, cancellationToken),
                        async ex => await this.OnEndErrorAsync(endProcessor, ex, cancellationToken),
                        async () => await this.OnEndCompletedAsync(endProcessor, cancellationToken)
                    );
                    break;
            }
            this.Processors.Add(processor);
            return processor;
        }

        /// <summary>
        /// Handles the next <see cref="V1WorkflowExecutionResult"/> returned by a <see cref="IWorkflowActivityProcessor"/>
        /// </summary>
        /// <param name="processor">The <see cref="IWorkflowActivityProcessor"/> that has returned an <see cref="V1WorkflowExecutionResult"/></param>
        /// <param name="result">The <see cref="V1WorkflowExecutionResult"/> to process</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnStateResultAsync(IStateProcessor processor, V1WorkflowExecutionResult result, CancellationToken cancellationToken)
        {
            //TODO
            //if (processor is SwitchStateProcessor switchStateProcessor)
            //{
            //    if (!processor.Pointer.Metadata.TryGetValue(SwitchStateProcessor.MatchingConditionMetadata, out string conditionName))
            //        throw new InvalidOperationException($"Failed to retrieve the required metadata with key '{SwitchStateProcessor.MatchingConditionMetadata}'");
            //    if (!switchStateProcessor.State.TryGetCondition(conditionName, out SwitchConditionDefinition condition))
            //        throw new InvalidOperationException($"Failed to find a condition with name '{conditionName}' in the state '{processor.State.Name}' of workflow '{this.ExecutionContext.Definition.Spec.Definition.Id}:{this.ExecutionContext.Definition.Spec.Definition.Version}'");
            //    switch (condition.Type)
            //    {
            //        case ConditionType.End:
            //            await this.ExecutionContext.CreateActivityAsync(V1WorkflowActivity.End(this.ExecutionContext.Instance, switchStateProcessor.State, conditionName, result.Output), cancellationToken);
            //            break;
            //        case ConditionType.Transition:
            //            await this.ExecutionContext.CreateActivityAsync(V1WorkflowActivity.Transition(this.ExecutionContext.Instance, switchStateProcessor.State, conditionName, result.Output), cancellationToken);
            //            break;
            //        default:
            //            throw new NotSupportedException($"The specified condition type '{condition.Type}' is not supported in this context");
            //    }
            //}
            /*else*/
            try
            {
                if (processor.State.Transition != null)
                {
                    await this.ExecutionContext.CreateActivityAsync(V1WorkflowActivity.Transition(this.ExecutionContext.Instance, processor.State, result.Output), cancellationToken);
                }
                else if (processor.State.End != null)
                {
                    await this.ExecutionContext.CreateActivityAsync(V1WorkflowActivity.End(this.ExecutionContext.Instance, processor.State, result.Output), cancellationToken);
                }
                else
                {
                    throw new InvalidOperationException($"The state '{processor.State.Name}' must declare a transition definition or an end definition for it is part of the main execution logic of the workflow '{this.ExecutionContext.Instance.Spec.Definition.Id}:{this.ExecutionContext.Instance.Spec.Definition.Version}'");
                }
                foreach (V1WorkflowActivity activity in await this.ExecutionContext.ListChildActivitiesAsync(cancellationToken))
                {
                    this.CreateProcessorFor(activity);
                }
            }
            catch(Exception ex)
            {
                this.Logger.LogError(ex.ToString());
            }
           
        }

        /// <summary>
        /// Handles the <see cref="Exception"/> thrown by a <see cref="IWorkflowActivityProcessor"/>
        /// </summary>
        /// <param name="processor">The <see cref="IWorkflowActivityProcessor"/> that has thrown the <see cref="Exception"/></param>
        /// <param name="ex">The thrown <see cref="Exception"/></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnStateErrorAsync(IStateProcessor processor, Exception ex, CancellationToken cancellationToken)
        {
            this.Processors.TryRemove(processor);
            processor.Dispose();
            await this.OnErrorAsync(ex, cancellationToken);
        }

        /// <summary>
        /// Handles the completion of a <see cref="IWorkflowActivityProcessor"/> execution
        /// </summary>
        /// <param name="processor">The <see cref="IWorkflowActivityProcessor"/> that ran to completion</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnStateCompletedAsync(IStateProcessor processor, CancellationToken cancellationToken)
        {
            this.Processors.TryRemove(processor);
            processor.Dispose();
            foreach (IWorkflowActivityProcessor childProcessor in this.Processors)
            {
                await childProcessor.ProcessAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Handles the next <see cref="V1WorkflowExecutionResult"/> returned by a <see cref="IWorkflowActivityProcessor"/>
        /// </summary>
        /// <param name="processor">The <see cref="IWorkflowActivityProcessor"/> that has returned an <see cref="V1WorkflowExecutionResult"/></param>
        /// <param name="result">The <see cref="V1WorkflowExecutionResult"/> to process</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnTransitionResultAsync(ITransitionProcessor processor, V1WorkflowExecutionResult result, CancellationToken cancellationToken)
        {
            if (!this.ExecutionContext.Definition.Spec.Definition.TryGetState(processor.Transition.To, out StateDefinition nextState))
                throw new NullReferenceException($"Failed to find a state with name '{processor.Transition.To}' in workflow '{this.ExecutionContext.Definition.Spec.Definition.Id} {this.ExecutionContext.Definition.Spec.Definition.Version}'");
            await this.ExecutionContext.TransitionToAsync(nextState, cancellationToken);
            V1WorkflowActivity pointer = await this.ExecutionContext.CreateActivityAsync(V1WorkflowActivity.State(this.ExecutionContext.Instance, nextState, result.Output), cancellationToken);
            this.CreateProcessorFor(pointer);
        }

        /// <summary>
        /// Handles the <see cref="Exception"/> thrown by a <see cref="IWorkflowActivityProcessor"/>
        /// </summary>
        /// <param name="processor">The <see cref="IWorkflowActivityProcessor"/> that has thrown the <see cref="Exception"/></param>
        /// <param name="ex">The thrown <see cref="Exception"/></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnTransitionErrorAsync(IWorkflowActivityProcessor processor, Exception ex, CancellationToken cancellationToken)
        {
            this.Processors.TryRemove(processor);
            processor.Dispose();
            await this.OnErrorAsync(ex, cancellationToken);
        }

        /// <summary>
        /// Handles the completion of a <see cref="IWorkflowActivityProcessor"/> execution
        /// </summary>
        /// <param name="processor">The <see cref="IWorkflowActivityProcessor"/> that ran to completion</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnTransitionCompletedAsync(ITransitionProcessor processor, CancellationToken cancellationToken)
        {
            this.Processors.TryRemove(processor);
            processor.Dispose();
            foreach (IWorkflowActivityProcessor childProcessor in this.Processors)
            {
                await childProcessor.ProcessAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Handles the <see cref="Exception"/> thrown by a <see cref="IWorkflowActivityProcessor"/>
        /// </summary>
        /// <param name="processor">The <see cref="IWorkflowActivityProcessor"/> that has thrown the <see cref="Exception"/></param>
        /// <param name="ex">The thrown <see cref="Exception"/></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnEndErrorAsync(IWorkflowActivityProcessor processor, Exception ex, CancellationToken cancellationToken)
        {
            this.Processors.TryRemove(processor);
            processor.Dispose();
            await this.OnErrorAsync(ex, cancellationToken);
        }

        /// <summary>
        /// Handles the completion of a <see cref="IWorkflowActivityProcessor"/> execution
        /// </summary>
        /// <param name="processor">The <see cref="IWorkflowActivityProcessor"/> that ran to completion</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnEndCompletedAsync(IWorkflowActivityProcessor processor, CancellationToken cancellationToken)
        {
            this.Processors.TryRemove(processor);
            processor.Dispose();
            await this.OnCompletedAsync(cancellationToken);
        }

        /// <summary>
        /// Handles the next <see cref="V1WorkflowInstance"/>'s <see cref="V1WorkflowExecutionResult"/>
        /// </summary>
        /// <param name="executionResult">The <see cref="V1WorkflowInstance"/>'s <see cref="V1WorkflowExecutionResult"/></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnResultAsync(V1WorkflowExecutionResult executionResult, CancellationToken cancellationToken)
        {
            if (executionResult.Type == V1WorkflowExecutionResultType.Next
                || executionResult.Type == V1WorkflowExecutionResultType.End)
                await this.ExecutionContext.SetWorkflowOutputAsync(executionResult.Output, cancellationToken);
        }

        /// <summary>
        /// Handles an <see cref="Exception"/> thrown during the  <see cref="WorkflowRunner"/>'s execution
        /// </summary>
        /// <param name="ex">The thrown <see cref="Exception"/></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnErrorAsync(Exception ex, CancellationToken cancellationToken)
        {
            try
            {
                this.Logger.LogWarning($"An error occured while executing the workflow instance{Environment.NewLine}Details: {{ex}}", ex.ToString());
                await this.ExecutionContext.FaultWorkflowAsync(ex, cancellationToken);
                this.ApplicationLifetime.StopApplication();
            }
            catch (Exception cex)
            {
                if (cex is HttpOperationException httpEx)
                    this.Logger.LogError($"A critical exception occured while faulting the execution of the workflow instance: the Kubernetes API returned an non-success status code '{{statusCode}}''{Environment.NewLine}Response content: {{responseContent}}{Environment.NewLine}Details: {{ex}}", httpEx.Response.StatusCode, httpEx.Response.Content, ex.ToString());
                else
                    this.Logger.LogError($"A critical exception occured while faulting the execution of the workflow instance:{ Environment.NewLine}{{ex}}", cex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Handles the <see cref="WorkflowRunner"/>'s completion
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual Task OnCompletedAsync(CancellationToken cancellationToken)
        {
            this.Logger.LogInformation("Workflow executed");
            this.ApplicationLifetime.StopApplication();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
            this.Processors?.ToList().ForEach(p => p.Dispose());
            this.Processors?.Clear();
        }

    }

}
