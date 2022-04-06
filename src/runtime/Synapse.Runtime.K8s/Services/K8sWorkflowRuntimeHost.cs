using Microsoft.Extensions.Logging;
using Synapse.Application.Services;

namespace Synapse.Runtime.Services
{

    /// <summary>
    /// Represents the Kubernetes implementation if the <see cref="IWorkflowRuntimeHost"/>
    /// </summary>
    public class K8sWorkflowRuntimeHost
        : WorkflowRuntimeHostBase
    {

        /// <inheritdoc/>
        public K8sWorkflowRuntimeHost(ILoggerFactory loggerFactory)
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