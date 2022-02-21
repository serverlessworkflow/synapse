using Microsoft.Extensions.Logging;
using Synapse.Application.Services;

namespace Synapse.Runtime.ProcessManager.Services
{

    /// <summary>
    /// Represents the Kubernetes implementation if the <see cref="IWorkflowRuntimeHost"/>
    /// </summary>
    public class ProcessManagerWorkflowRuntimeHost
        : WorkflowRuntimeHostBase
    {

        /// <inheritdoc/>
        public ProcessManagerWorkflowRuntimeHost(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }

        public override Task StartWorkflowAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task ResumeWorkflowAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task SuspendWorkflowAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task TerminateWorkflowAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

    }

}
