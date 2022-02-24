﻿/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */
using Synapse.Integration.Events;

namespace Synapse.Runtime.Services
{

    /// <summary>
    /// Defines the fundamentals of a runtime specific API facade for Synapse
    /// </summary>
    public interface IWorkflowFacade
    {

        /// <summary>
        /// Gets the current workflow's definition
        /// </summary>
        WorkflowDefinition Definition { get; }

        /// <summary>
        /// Gets the current workflow instance
        /// </summary>
        V1WorkflowInstanceDto Instance { get; }

        /// <summary>
        /// Starts the current workflow
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Transitions the current workflow to the specified state
        /// </summary>
        /// <param name="state">The state to transition the current workflow to</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task TransitionToAsync(StateDefinition state, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new activity
        /// </summary>
        /// <param name="type">The type of activity to create</param>
        /// <param name="input">The activity's input data</param>
        /// <param name="metadata">The activity's metadata</param>
        /// <param name="parent">The activity's parent, if any</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The newly created activity</returns>
        Task<V1WorkflowActivityDto> CreateActivityAsync(V1WorkflowActivityType type, object? input, IDictionary<string, string>? metadata, V1WorkflowActivityDto? parent, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all the activities (including non-operative ones) of the current workflow
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="List{T}"/> containing all the activities (including non-operative ones) of the current workflow instance</returns>
        Task<List<V1WorkflowActivityDto>> GetActivitiesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the current workflow's operative activities
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="List{T}"/> containing the current workflow's operative activities</returns>
        Task<List<V1WorkflowActivityDto>> GetOperativeActivitiesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all the activities (including non-operative ones) of the specified workflow activity
        /// </summary>
        /// <param name="activity">The activity to get the children of</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="List{T}"/> containing all the children (including non-operative ones) of the specified activity</returns>
        Task<List<V1WorkflowActivityDto>> GetActivitiesAsync(V1WorkflowActivityDto activity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the operative child activities of the specified activity
        /// </summary>
        /// <param name="activity">The activity to get the operative child activities of</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="List{T}"/> containing the current specified activity's operative children</returns>
        Task<List<V1WorkflowActivityDto>> GetOperativeActivitiesAsync(V1WorkflowActivityDto activity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Initializes the specified activity
        /// </summary>
        /// <param name="activity">The activity to initialize</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task InitializeActivityAsync(V1WorkflowActivityDto activity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Starts or resumes the specified activity
        /// </summary>
        /// <param name="activity">The activity to starts or resumes</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task StartOrResumeActivityAsync(V1WorkflowActivityDto activity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Suspends the execution of the specified activity
        /// </summary>
        /// <param name="activity">The activity to suspend the execution of</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task SuspendActivityAsync(V1WorkflowActivityDto activity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cancels the execution of the specified activity
        /// </summary>
        /// <param name="activity">The activity to cancel the execution of</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task CancelActivityAsync(V1WorkflowActivityDto activity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Faults the execution of the specified activity
        /// </summary>
        /// <param name="activity">The activity to fault</param>
        /// <param name="ex">The <see cref="Exception"/> that cause the activity to fault</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task FaultActivityAsync(V1WorkflowActivityDto activity, Exception ex, CancellationToken cancellationToken = default);

        /// <summary>
        /// Faults the execution of the specified activity
        /// </summary>
        /// <param name="activity">The activity to fault</param>
        /// <param name="error">The error that cause the activity to fault</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task FaultActivityAsync(V1WorkflowActivityDto activity, ErrorDto error, CancellationToken cancellationToken = default);

        /// <summary>
        /// Faults the execution of the current workflow instance
        /// </summary>
        /// <param name="ex">The <see cref="Exception"/> that cause the current workflow to fault</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task FaultAsync(Exception ex, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the output of the current workflow instance
        /// </summary>
        /// <param name="output">The current workflow instance's output</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task SetOutputAsync(object? output, CancellationToken cancellationToken = default);

        /// <summary>
        /// Handles the specified activity event
        /// </summary>
        /// <param name="activity">The activity that has produced the event</param>
        /// <param name="e">The event to handle</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task On(V1WorkflowActivityDto activity, IV1WorkflowActivityIntegrationEvent e, CancellationToken cancellationToken = default);

    }

}