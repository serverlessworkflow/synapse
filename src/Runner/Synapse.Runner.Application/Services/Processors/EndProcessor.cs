using Microsoft.Extensions.Logging;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Domain.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Synapse.Runner.Application.Services.Processors
{

    /// <summary>
    /// Represents a <see cref="IWorkflowActivityProcessor"/> used to process <see cref="EndDefinition"/>s
    /// </summary>
    public class EndProcessor
        : WorkflowActivityProcessor, IEndProcessor
    {

        /// <summary>
        /// Initializes a new <see cref="EndProcessor"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="executionContext">The current <see cref="IWorkflowExecutionContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="end">The <see cref="EndDefinition"/> to process</param>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to process</param>
        public EndProcessor(ILoggerFactory loggerFactory, IWorkflowExecutionContext executionContext, IWorkflowActivityProcessorFactory activityProcessorFactory, EndDefinition end, V1WorkflowActivity activity)
            : base(loggerFactory, executionContext, activityProcessorFactory, activity)
        {
            this.End = end;
        }

        /// <summary>
        /// Gets the <see cref="EndDefinition"/> to process
        /// </summary>
        public EndDefinition End { get; }

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
            await this.OnResultAsync(V1WorkflowExecutionResult.End(this.Activity.Data), cancellationToken);
            await this.OnCompletedAsync(cancellationToken);
        }

    }

}
