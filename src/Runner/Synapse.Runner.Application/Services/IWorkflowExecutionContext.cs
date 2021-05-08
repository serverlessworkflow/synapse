using CloudNative.CloudEvents;
using Newtonsoft.Json.Linq;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Domain.Models;
using Synapse.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Synapse.Runner.Application.Services
{

    /// <summary>
    /// Defines the fundamentals of a service used to manage a <see cref="V1WorkflowInstance"/>'s execution context
    /// </summary>
    public interface IWorkflowExecutionContext
    {

        /// <summary>
        /// Gets the service used to evaluate workflow expressions
        /// </summary>
        IExpressionEvaluator ExpressionEvaluator { get; }

        /// <summary>
        /// Gets the <see cref="V1Workflow"/> that is being executed
        /// </summary>
        V1Workflow Definition { get; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowInstance"/> that is being executed
        /// </summary>
        V1WorkflowInstance Instance { get; }

        /// <summary>
        /// Initializes the <see cref="IWorkflowExecutionContext"/>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Starts the <see cref="Instance"/>'s execution
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task StartWorkflowAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the <see cref="Instance"/>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task ExecuteWorkflowAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Suspends the <see cref="Instance"/>'s execution
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task SuspendWorkflowAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Faults the <see cref="Instance"/>'s execution
        /// </summary>
        /// <param name="ex">The <see cref="Exception"/> that has occured during the <see cref="Instance"/>'s execution</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task FaultWorkflowAsync(Exception ex, CancellationToken cancellationToken = default);

        /// <summary>
        /// Terminates the <see cref="Instance"/>'s execution
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task TerminateWorkflowAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Starts the transition of the <see cref="Instance"/>'s current <see cref="StateDefinition"/> to the specified one
        /// </summary>
        /// <param name="state">The <see cref="StateDefinition"/> to transition to</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task TransitionToAsync(StateDefinition state, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to correlate the specified <see cref="CloudEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="CloudEvent"/> to correlate</param>
        /// <param name="contextAttributes">An <see cref="IEnumerable{T}"/> containing the context attributes used to correlate the specified <see cref="CloudEvent"/></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A boolean indicating whether or not the specified <see cref="CloudEvent"/> could be correlated</returns>
        Task<bool> TryCorrelateAsync(CloudEvent e, IEnumerable<string> contextAttributes, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the <see cref="Instance"/>'s output
        /// </summary>
        /// <param name="output">The <see cref="Instance"/>'s output data</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task SetWorkflowOutputAsync(JToken output, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to get a bootstrap <see cref="CloudEvent"/> matching the specified <see cref="EventDefinition"/>
        /// </summary>
        /// <param name="eventDefinition">The <see cref="EventDefinition"/> that describes the <see cref="CloudEvent"/> to get</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The matching <see cref="CloudEvent"/>, if any</returns>
        Task<CloudEvent> GetBoostrapEventAsync(EventDefinition eventDefinition, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists the <see cref="Instance"/>'s child <see cref="V1WorkflowActivity"/> instances
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task<List<V1WorkflowActivity>> ListChildActivitiesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists the specified <see cref="V1WorkflowActivity"/>'s child <see cref="V1WorkflowActivity"/> instances
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to list the children of</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task<List<V1WorkflowActivity>> ListActiveChildActivitiesAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists the specified <see cref="V1WorkflowActivity"/>'s child <see cref="V1WorkflowActivity"/> instances
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to list the children of</param>
        /// <param name="includeInactive">A boolean indicating whether or not to include inactive instances. Defaults to false.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task<List<V1WorkflowActivity>> ListChildActivitiesAsync(V1WorkflowActivity activity, bool includeInactive = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to create</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The newly created <see cref="V1WorkflowActivity"/></returns>
        Task<V1WorkflowActivity> CreateActivityAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Initializes the specified <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to initialize</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task<V1WorkflowActivity> InitializeActivityAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Processes the specified <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to process</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The updated <see cref="V1WorkflowActivity"/></returns>
        Task<V1WorkflowActivity> ProcessActivityAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Suspends the specified <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to suspend</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The updated <see cref="V1WorkflowActivity"/></returns>
        Task<V1WorkflowActivity> SuspendActivityAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Faults the specified <see cref="V1WorkflowActivity"/>'s execution
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to fault the execution of</param>
        /// <param name="ex">The <see cref="Exception"/> that has occured during the <see cref="V1WorkflowActivity"/>'s execution</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The updated <see cref="V1WorkflowActivity"/></returns>
        Task<V1WorkflowActivity> FaultActivityAsync(V1WorkflowActivity activity, Exception ex, CancellationToken cancellationToken = default);

        /// <summary>
        /// Terminates the specified <see cref="V1WorkflowActivity"/>'s execution
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to terminate the execution of</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The updated <see cref="V1WorkflowActivity"/></returns>
        Task<V1WorkflowActivity> TerminateActivityAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the specified <see cref="V1WorkflowActivity"/>'s execution result
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to set the execution result of</param>
        /// <param name="result">The <see cref="V1WorkflowActivity"/>'s <see cref="V1WorkflowExecutionResult"/></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The updated <see cref="V1WorkflowActivity"/></returns>
        Task<V1WorkflowActivity> SetActivityResultAsync(V1WorkflowActivity activity, V1WorkflowExecutionResult result, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the specified <see cref="V1WorkflowActivity"/>'s data
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to update the data of</param>
        /// <param name="data">The update data</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The updated <see cref="V1WorkflowActivity"/></returns>
        Task<V1WorkflowActivity> UpdateActivityDataAsync(V1WorkflowActivity activity, JToken data, CancellationToken cancellationToken = default);

    }

}
