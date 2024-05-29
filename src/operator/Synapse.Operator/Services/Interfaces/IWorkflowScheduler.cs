namespace Synapse.Operator.Services;

/// <summary>
/// Defines the fundamentals of a service used to manage the scheduling or a workflow process
/// </summary>
public interface IWorkflowScheduler
{

    /// <summary>
    /// Schedules the workflow process
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task ScheduleAsync(CancellationToken cancellationToken = default);

}
