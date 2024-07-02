// Copyright © 2024-Present The Synapse Authors
//
// Licensed under the Apache License, Version 2.0 (the "License"),
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Cronos;

namespace Synapse.Operator.Services;

/// <summary>
/// Represents the service used to manage the scheduling and execution of a specific <see cref="Workflow"/>
/// </summary>
/// <param name="logger">The service used to perform logging</param>
/// <param name="resources">The service used to manage resources</param>
/// <param name="workflow">The resource of the workflow to manage the scheduling of</param>
/// <param name="workflowDefinition">The definition of the workflow to manage the scheduling of</param>
public class WorkflowScheduler(ILogger<WorkflowScheduler> logger, IResourceRepository resources, IResourceMonitor<Workflow> workflow, WorkflowDefinition workflowDefinition)
    : IDisposable, IAsyncDisposable
{

    static bool _missedOccurrence = true;
    bool _disposed;

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger Logger { get; } = logger;

    /// <summary>
    /// Gets the service used to manage resources
    /// </summary>
    protected IResourceRepository Resources { get; } = resources;

    /// <summary>
    /// Gets the resource of the workflow to manage the scheduling of
    /// </summary>
    protected IResourceMonitor<Workflow> Workflow { get; } = workflow;

    /// <summary>
    /// Gets the definition of the workflow to manage the scheduling of
    /// </summary>
    protected WorkflowDefinition WorkflowDefinition { get; } = workflowDefinition;

    /// <summary>
    /// Gets the <see cref="WorkflowScheduler"/>'s <see cref="System.Threading.CancellationTokenSource"/>
    /// </summary>
    protected CancellationTokenSource CancellationTokenSource { get; } = new();

    /// <summary>
    /// Gets the <see cref="System.Threading.Timer"/>, if any, used to schedule the task
    /// </summary>
    protected Timer? Timer { get; private set; }

    /// <summary>
    /// Schedules the execution of the workflow
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public virtual async Task ScheduleAsync(CancellationToken cancellationToken = default)
    {
        if (this.WorkflowDefinition.Schedule == null) return;
        if (this.WorkflowDefinition.Schedule.On != null)
        {
            var correlation = await this.Resources.GetAsync<Correlation>(this.Workflow.Resource.GetName(), this.Workflow.Resource.GetNamespace(), cancellationToken).ConfigureAwait(false);
            if (correlation != null) return;
            correlation = new Correlation()
            {
                Metadata = new()
                {
                    Namespace = this.Workflow.Resource.GetNamespace(),
                    Name = $"{this.Workflow.Resource.GetName()}-schedule",
                },
                Spec = new()
                {
                    Source = this.Workflow.Resource.GetReference(),
                    Lifetime = CorrelationLifetime.Durable,
                    Events = this.WorkflowDefinition.Schedule.On,
                    Outcome = new()
                    {
                        Start = new()
                        {
                            Workflow = new()
                            {
                                Namespace = this.Workflow.Resource.GetNamespace()!,
                                Name = this.Workflow.Resource.GetName(),
                                Version = this.WorkflowDefinition.Document.Version
                            }
                        }
                    },
                    Expressions = this.WorkflowDefinition.Evaluate ?? new()
                }
            };
            await this.Resources.AddAsync(correlation, false, cancellationToken).ConfigureAwait(false);
            return;
        }
        TimeSpan delay;
        do
        {
            DateTimeOffset? nextOccurrence;
            WorkflowVersionStatus? versionStatus = null;
            this.Workflow.Resource.Status?.Versions.TryGetValue(this.WorkflowDefinition.Document.Version, out versionStatus);
            if (this.WorkflowDefinition.Schedule.After != null)
            {
                nextOccurrence = versionStatus?.LastEndedAt.HasValue == true
                    ? versionStatus.LastEndedAt!.Value.Add(this.WorkflowDefinition.Schedule.After.ToTimeSpan())
                    : DateTimeOffset.Now.Add(this.WorkflowDefinition.Schedule.After.ToTimeSpan());
            }
            else if (this.WorkflowDefinition.Schedule.Cron != null)
            {
                nextOccurrence = versionStatus?.LastStartedAt.HasValue == true
                    ? versionStatus.LastStartedAt!.Value
                    : DateTimeOffset.Now;
                nextOccurrence = CronExpression.Parse(this.WorkflowDefinition.Schedule.Cron).GetNextOccurrence(nextOccurrence.Value);
            }
            else if (this.WorkflowDefinition.Schedule.Every != null)
            {
                nextOccurrence = this.Workflow.Resource.Status?.Versions[this.WorkflowDefinition.Document.Version].LastStartedAt.HasValue == true
                    ? this.Workflow.Resource.Status.Versions[this.WorkflowDefinition.Document.Version].LastStartedAt!.Value.Add(this.WorkflowDefinition.Schedule.Every.ToTimeSpan())
                    : DateTimeOffset.Now.Add(this.WorkflowDefinition.Schedule.Every.ToTimeSpan());
            }
            else throw new NotSupportedException("The specified workflow schedule type is not supported");
            if (!nextOccurrence.HasValue)
            {
                this.Logger.LogWarning("Failed to compute the next occurrence for workflow '{workflowQualifiedName}'", this.Workflow.Resource.GetQualifiedName());
                return;
            }
            delay = nextOccurrence.Value - DateTimeOffset.Now;
            if (delay <= TimeSpan.Zero && _missedOccurrence)
            {
                _missedOccurrence = false;
                delay = TimeSpan.Zero;
                break;
            }
        }
        while (delay <= TimeSpan.Zero);
        this.Timer = new Timer(async _ => await this.OnScheduleTimerElapsedAsync().ConfigureAwait(false), null, delay, Timeout.InfiniteTimeSpan);
    }

    /// <summary>
    /// Handles the completion of the scheduler's timer
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task OnScheduleTimerElapsedAsync()
    {
        await this.Resources.AddAsync<WorkflowInstance>(new()
        {
            Metadata = new()
            {
                Namespace = this.Workflow.Resource.GetNamespace()!,
                Name = $"{this.Workflow.Resource.GetName()}-"
            },
            Spec = new()
            {
                Definition = new()
                {
                    Namespace = this.Workflow.Resource.GetNamespace()!,
                    Name = this.Workflow.Resource.GetName(),
                    Version = this.WorkflowDefinition.Document.Version
                }
            }
        }, false, this.CancellationTokenSource.Token).ConfigureAwait(false);
        await Task.Delay(50).ConfigureAwait(false);
        await this.ScheduleAsync(this.CancellationTokenSource.Token).ConfigureAwait(false);
    }

    /// <summary>
    /// Disposes of the <see cref="WorkflowScheduler"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="WorkflowScheduler"/> is being disposed of</param>
    /// <returns>A new awaitable <see cref="ValueTask"/></returns>
    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (this._disposed || !disposing) return;
        if (this.Timer != null) await this.Timer.DisposeAsync().ConfigureAwait(false);
        this._disposed = true;
        await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await this.DisposeAsync(true).ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of the <see cref="WorkflowScheduler"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="WorkflowScheduler"/> is being disposed of</param>
    protected virtual void Dispose(bool disposing)
    {
        if (this._disposed || !disposing) return;
        this.Timer?.Dispose();
        this._disposed = true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

}
