using ServerlessWorkflow.Sdk.Models;
using Synapse.Domain.Models;
using Synapse.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Synapse
{

    /// <summary>
    /// Defines extensions for <see cref="V1WorkflowInstance"/>s
    /// </summary>
    public static class V1WorkflowInstanceRepositoryExtensions
    {

        /// <summary>
        /// Starts the specified <see cref="V1WorkflowInstance"/>
        /// </summary>
        /// <param name="repository">The <see cref="IRepository{TEntity}"/> used to persist <see cref="V1WorkflowInstance"/>s</param>
        /// <param name="workflowInstance">The <see cref="V1WorkflowInstance"/> to start</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The started <see cref="V1WorkflowInstance"/></returns>
        public static async Task<V1WorkflowInstance> StartAsync(this IRepository<V1WorkflowInstance> repository, V1WorkflowInstance workflowInstance, CancellationToken cancellationToken = default)
        {
            if (workflowInstance == null)
                throw new ArgumentNullException(nameof(workflowInstance));
            workflowInstance.Start();
            workflowInstance = await repository.UpdateAsync(workflowInstance, cancellationToken);
            return workflowInstance;
        }

        /// <summary>
        /// Executes the specified <see cref="V1WorkflowInstance"/>
        /// </summary>
        /// <param name="repository">The <see cref="IRepository{TEntity}"/> used to persist <see cref="V1WorkflowInstance"/>s</param>
        /// <param name="workflowInstance">The <see cref="V1WorkflowInstance"/> to execute</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The executing <see cref="V1WorkflowInstance"/></returns>
        public static async Task<V1WorkflowInstance> ExecuteAsync(this IRepository<V1WorkflowInstance> repository, V1WorkflowInstance workflowInstance, CancellationToken cancellationToken = default)
        {
            if (workflowInstance == null)
                throw new ArgumentNullException(nameof(workflowInstance));
            workflowInstance.Execute();
            workflowInstance = await repository.UpdateAsync(workflowInstance, cancellationToken);
            return workflowInstance;
        }

        /// <summary>
        /// Suspends the specified <see cref="V1WorkflowInstance"/>'s execution
        /// </summary>
        /// <param name="repository">The <see cref="IRepository{TEntity}"/> used to persist <see cref="V1WorkflowInstance"/>s</param>
        /// <param name="workflowInstance">The <see cref="V1WorkflowInstance"/> to suspend the execution of</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The executing <see cref="V1WorkflowInstance"/></returns>
        public static async Task<V1WorkflowInstance> SuspendAsync(this IRepository<V1WorkflowInstance> repository, V1WorkflowInstance workflowInstance, CancellationToken cancellationToken = default)
        {
            if (workflowInstance == null)
                throw new ArgumentNullException(nameof(workflowInstance));
            workflowInstance.Suspend();
            workflowInstance = await repository.UpdateAsync(workflowInstance, cancellationToken);
            return workflowInstance;
        }

        /// <summary>
        /// Faults the specified <see cref="V1WorkflowInstance"/>'s execution
        /// </summary>
        /// <param name="repository">The <see cref="IRepository{TEntity}"/> used to persist <see cref="V1WorkflowInstance"/>s</param>
        /// <param name="workflowInstance">The <see cref="V1WorkflowInstance"/> to fault the execution of</param>
        /// <param name="ex">The <see cref="Exception"/> that has occured during the <see cref="V1WorkflowInstance"/>'s execution</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The executing <see cref="V1WorkflowInstance"/></returns>
        public static async Task<V1WorkflowInstance> FaultAsync(this IRepository<V1WorkflowInstance> repository, V1WorkflowInstance workflowInstance, Exception ex, CancellationToken cancellationToken = default)
        {
            if (workflowInstance == null)
                throw new ArgumentNullException(nameof(workflowInstance));
            workflowInstance.Fault(ex);
            workflowInstance = await repository.UpdateAsync(workflowInstance, cancellationToken);
            return workflowInstance;
        }

        /// <summary>
        /// Terminates the specified <see cref="V1WorkflowInstance"/>'s execution
        /// </summary>
        /// <param name="repository">The <see cref="IRepository{TEntity}"/> used to persist <see cref="V1WorkflowInstance"/>s</param>
        /// <param name="workflowInstance">The <see cref="V1WorkflowInstance"/> to terminate the execution of</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The executing <see cref="V1WorkflowInstance"/></returns>
        public static async Task<V1WorkflowInstance> TerminateAsync(this IRepository<V1WorkflowInstance> repository, V1WorkflowInstance workflowInstance, CancellationToken cancellationToken = default)
        {
            if (workflowInstance == null)
                throw new ArgumentNullException(nameof(workflowInstance));
            workflowInstance.Terminate();
            workflowInstance = await repository.UpdateAsync(workflowInstance, cancellationToken);
            return workflowInstance;
        }

        /// <summary>
        /// Operates a transition to the specified <see cref="StateDefinition"/> on a running <see cref="V1WorkflowInstance"/> 
        /// </summary>
        /// <param name="repository">The <see cref="IRepository{TEntity}"/> used to persist <see cref="V1WorkflowInstance"/>s</param>
        /// <param name="workflowInstance">The <see cref="V1WorkflowInstance"/> to operate a transition on</param>
        /// <param name="state">The <see cref="StateDefinition"/> to transition to</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The transitioned <see cref="V1WorkflowInstance"/></returns>
        public static async Task<V1WorkflowInstance> TransitionToAsync(this IRepository<V1WorkflowInstance> repository, V1WorkflowInstance workflowInstance, StateDefinition state, CancellationToken cancellationToken = default)
        {
            if (workflowInstance == null)
                throw new ArgumentNullException(nameof(workflowInstance));
            workflowInstance.TransitionTo(state);
            workflowInstance = await repository.UpdateAsync(workflowInstance, cancellationToken);
            return workflowInstance;
        }

    }

}
