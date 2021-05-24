using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Domain.Models;
using Synapse.Runner.Application.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Synapse.Runner.Application.Services.Processors
{

    /// <summary>
    /// Represents an <see cref="ActionProcessor"/> used to process <see cref="TransitionDefinition"/>s
    /// </summary>
    public class TransitionProcessor
        : WorkflowActivityProcessor, ITransitionProcessor
    {

        /// <summary>
        /// Initializes a new <see cref="TransitionProcessor"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="executionContext">The current <see cref="IWorkflowExecutionContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="options">The service used to access the current <see cref="ApplicationOptions"/></param>
        /// <param name="state">The <see cref="StateDefinition"/> to process the transition of</param>
        /// <param name="transition">The <see cref="TransitionDefinition"/> to process</param>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to process</param>
        public TransitionProcessor(ILoggerFactory loggerFactory, IWorkflowExecutionContext executionContext, IWorkflowActivityProcessorFactory activityProcessorFactory, IOptions<ApplicationOptions> options, StateDefinition state, TransitionDefinition transition, V1WorkflowActivity activity) 
            : base(loggerFactory, executionContext, activityProcessorFactory, options, activity)
        {
            this.State = state;
            this.Transition = transition;
        }

        /// <summary>
        /// Gets the <see cref="StateDefinition"/> to process the transition of
        /// </summary>
        public StateDefinition State { get; }

        /// <summary>
        /// Gets the <see cref="TransitionDefinition"/> to process
        /// </summary>
        public TransitionDefinition Transition { get; }

        /// <inheritdoc/>
        protected override Task InitializeAsync(CancellationToken cancellationToken)
        {
            //TODO
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            //TODO
            await this.OnResultAsync(V1WorkflowExecutionResult.Next(this.Activity.Data), cancellationToken);
            await this.OnCompletedAsync(cancellationToken);
        }

    }

}
