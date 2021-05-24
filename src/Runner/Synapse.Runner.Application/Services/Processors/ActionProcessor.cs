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
    /// Represents a <see cref="IWorkflowActivityProcessor"/> used to process <see cref="ActionDefinition"/>s
    /// </summary>
    public abstract class ActionProcessor
        : WorkflowActivityProcessor
    {

        /// <summary>
        /// Initializes a new <see cref="ActionProcessor"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="executionContext">The current <see cref="IWorkflowExecutionContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="options">The service used to access the current <see cref="ApplicationOptions"/></param>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to process</param>
        /// <param name="action">The <see cref="ActionDefinition"/> to process</param>
        protected ActionProcessor(ILoggerFactory loggerFactory, IWorkflowExecutionContext executionContext, IWorkflowActivityProcessorFactory activityProcessorFactory, IOptions<ApplicationOptions> options, V1WorkflowActivity activity, ActionDefinition action) 
            : base(loggerFactory, executionContext, activityProcessorFactory, options, activity)
        {
            this.Action = action;
        }

        /// <summary>
        /// Gets the <see cref="ActionDefinition"/> to process
        /// </summary>
        public ActionDefinition Action { get; }

        /// <inheritdoc/>
        protected override async Task OnResultAsync(V1WorkflowExecutionResult result, CancellationToken cancellationToken)
        {
            if (result.Output != null)
                result = new V1WorkflowExecutionResult(result.Type, this.ExecutionContext.ExpressionEvaluator.FilterResults(this.Action, result.Output));
            await base.OnResultAsync(result, cancellationToken);
        }

    }

}
