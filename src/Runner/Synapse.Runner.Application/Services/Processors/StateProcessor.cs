using Microsoft.Extensions.Logging;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Domain.Models;

namespace Synapse.Runner.Application.Services.Processors
{
    /// <summary>
    /// Represents the base class for all <see cref="IWorkflowActivityProcessor"/>s that process <see cref="StateDefinition"/>s
    /// </summary>
    /// <typeparam name="TState">The type of <see cref="StateDefinition"/> to process</typeparam>
    public abstract class StateProcessor<TState>
        : WorkflowActivityProcessor, IStateProcessor<TState>
        where TState : StateDefinition
    {

        /// <summary>
        /// Initializes a new <see cref="StateProcessor{TState}"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="executionContext">The current <see cref="IWorkflowExecutionContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to process</param>
        /// <param name="state">The <see cref="StateDefinition"/> to process</param>
        protected StateProcessor(ILoggerFactory loggerFactory, IWorkflowExecutionContext executionContext, IWorkflowActivityProcessorFactory activityProcessorFactory, V1WorkflowActivity activity, TState state) 
            : base(loggerFactory, executionContext, activityProcessorFactory, activity)
        {
            this.State = state;
        }

        /// <summary>
        /// Gets the <see cref="StateDefinition"/> to process
        /// </summary>
        public TState State { get; }

        StateDefinition IStateProcessor.State => this.State;

    }

}
