namespace Synapse.Worker.Services
{

    /// <summary>
    /// Defines the fundamentals of a service used to process <see cref="V1WorkflowActivity"/> instances
    /// </summary>
    public interface IWorkflowActivityProcessor
        : IObservable<IV1WorkflowActivityIntegrationEvent>, IDisposable
    {

        /// <summary>
        /// Gets the <see cref="V1WorkflowActivity"/> to process
        /// </summary>
        V1WorkflowActivity Activity { get; }

        /// <summary>
        /// Processes the <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task ProcessAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Suspends the processing of the <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task SuspendAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Terminates the <see cref="V1WorkflowActivity"/>'s execution
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task TerminateAsync(CancellationToken cancellationToken);

    }

    /// <summary>
    /// Defines the fundamentals of a service used to process <see cref="V1WorkflowActivity"/> instances
    /// </summary>
    /// <typeparam name="TActivity">The type of the <see cref="V1WorkflowActivity"/> to process</typeparam>
    public interface IWorkflowActivityProcessor<TActivity>
        : IWorkflowActivityProcessor
        where TActivity : V1WorkflowActivity
    {

        /// <summary>
        /// Gets the <see cref="V1WorkflowActivity"/> to process
        /// </summary>
        new TActivity Activity { get; }

    }

}
