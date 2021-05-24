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
    /// Represents the <see cref="IWorkflowActivityProcessor"/> used to process <see cref="InjectStateDefinition"/>s
    /// </summary>
    public class InjectStateProcessor
        : StateProcessor<InjectStateDefinition>
    {

        /// <summary>
        /// Initializes a new <see cref="InjectStateProcessor"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="executionContext">The current <see cref="IWorkflowExecutionContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="options">The service used to access the current <see cref="ApplicationOptions"/></param>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to process</param>
        /// <param name="state">The <see cref="InjectStateDefinition"/> to process</param>
        public InjectStateProcessor(ILoggerFactory loggerFactory, IWorkflowExecutionContext executionContext, IWorkflowActivityProcessorFactory activityProcessorFactory, IOptions<ApplicationOptions> options, V1WorkflowActivity activity, InjectStateDefinition state)
            : base(loggerFactory, executionContext, activityProcessorFactory, options, activity, state)
        {

        }

        /// <inheritdoc/>
        protected override Task InitializeAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            await this.OnResultAsync(V1WorkflowExecutionResult.Next(this.State.Data), cancellationToken);
            await this.OnCompletedAsync(cancellationToken);
        }

    }

}
