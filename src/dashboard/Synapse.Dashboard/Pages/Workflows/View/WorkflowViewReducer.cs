/*
 * Copyright © 2022-Present The Synapse Authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using Neuroglia.Data.Flux;
using Synapse.Dashboard.Pages.Workflows.View.Actions;
using Synapse.Dashboard.Pages.Workflows.View.State;
using Synapse.Integration.Models;

namespace Synapse.Dashboard.Pages.Workflows.View
{
    [Reducer]
    public static class WorkflowViewReducer
    {
        /// <summary>
        /// Initialize the state
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="InitializeStateSuccessful"/> action to reduce</param>
        /// <returns>The reduced state</returns>
        public static WorkflowViewState On(WorkflowViewState state, InitializeStateSuccessful action)
        {
            return action.InitialState;
        }

        /// <summary>
        /// Sets the workflow
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="SetWorkflow"/> action to reduce</param>
        /// <returns></returns>
        public static WorkflowViewState On(WorkflowViewState state, SetWorkflow action)
        {
            return state with
            {
                Workflow = action.Workflow,
                ActiveInstance = null
            };
        }

        /// <summary>
        /// Sets the active instance
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="SetActiveInstance"/> action to reduce</param>
        /// <returns></returns>
        public static WorkflowViewState On(WorkflowViewState state, SetActiveInstance action)
        {
            return state with
            {
                ActiveInstance = action.Instance
            };
        }

        /// <summary>
        /// Sets the workflow instances
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="SetWorkflowInstances"/> action to reduce</param>
        /// <returns></returns>
        public static WorkflowViewState On(WorkflowViewState state, SetWorkflowInstances action)
        {
            return state with
            {
                Instances = action.WorkflowInstances.ToDictionary(instance => instance.Id),
                Activities = action.WorkflowInstances.SelectMany(instance => instance.Activities).ToDictionary(activity => activity.Id)
            };
        }

        /// <summary>
        /// Adds a workflow instance to the list
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="AddV1WorkflowInstance"/> action to reduce</param>
        /// <returns></returns>
        public static WorkflowViewState On(WorkflowViewState state, AddV1WorkflowInstance action)
        {
            if (action.WorkflowInstance.WorkflowId != state.Workflow?.Id)
            {
                return state;
            }
            var instances = state.Instances != null ? new Dictionary<string, V1WorkflowInstance>(state.Instances) : new Dictionary<string, V1WorkflowInstance>();
            if (instances.ContainsKey(action.WorkflowInstance.Id))
                return state;
            instances.Add(action.WorkflowInstance.Id, action.WorkflowInstance);
            return state with
            { 
                Instances = instances
            };
        }

        /// <summary>
        /// Removes an instance
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="RemoveV1WorkflowInstance"/> action to reduce</param>
        /// <returns></returns>
        public static WorkflowViewState On(WorkflowViewState state, RemoveV1WorkflowInstance action)
        {
            if (state.Instances == null)
                return state;
            if (!state.Instances.ContainsKey(action.Id))
                return state;
            var instances = new Dictionary<string, V1WorkflowInstance>(state.Instances);
            instances.Remove(action.Id);
            return state with
            {
                Instances = instances
            };
        }

        /// <summary>
        /// Marks an instance as starting
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="MarkV1WorkflowInstanceAsStarting"/> action to reduce</param>
        /// <returns></returns>
        public static WorkflowViewState On(WorkflowViewState state, MarkV1WorkflowInstanceAsStarting action)
        {
            if (state.Instances == null)
                return state;
            if (!state.Instances.ContainsKey(action.Id))
                return state;
            var instance = state.Instances[action.Id];
            instance.LastModified = action.StartingAt;
            instance.Status = V1WorkflowInstanceStatus.Starting;
            var instances = new Dictionary<string, V1WorkflowInstance>(state.Instances);
            return state with
            {
                Instances = instances
            };
        }

        /// <summary>
        /// Marks an instance as started
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="MarkV1WorkflowInstanceAsStarted"/> action to reduce</param>
        /// <returns></returns>
        public static WorkflowViewState On(WorkflowViewState state, MarkV1WorkflowInstanceAsStarted action)
        {
            if (state.Instances == null)
                return state;
            if (!state.Instances.ContainsKey(action.Id))
                return state;
            var instance = state.Instances[action.Id];
            instance.LastModified = action.StartedAt;
            instance.StartedAt = action.StartedAt;
            instance.Status = V1WorkflowInstanceStatus.Running;
            var instances = new Dictionary<string, V1WorkflowInstance>(state.Instances);
            return state with
            {
                Instances = instances
            };
        }

        /// <summary>
        /// Marks an instance as suspending
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="MarkV1WorkflowInstanceAsSuspending"/> action to reduce</param>
        /// <returns></returns>
        public static WorkflowViewState On(WorkflowViewState state, MarkV1WorkflowInstanceAsSuspending action)
        {
            if (state.Instances == null)
                return state;
            if (!state.Instances.ContainsKey(action.Id))
                return state;
            var instance = state.Instances[action.Id];
            instance.LastModified = action.SuspendingAt;
            instance.Status = V1WorkflowInstanceStatus.Suspending;
            var instances = new Dictionary<string, V1WorkflowInstance>(state.Instances);
            return state with
            {
                Instances = instances
            };
        }

        /// <summary>
        /// Marks an instance as suspended
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="MarkV1WorkflowInstanceAsSuspending"/> action to reduce</param>
        /// <returns></returns>
        public static WorkflowViewState On(WorkflowViewState state, MarkV1WorkflowInstanceAsSuspended action)
        {
            if (state.Instances == null)
                return state;
            if (!state.Instances.ContainsKey(action.Id))
                return state;
            var instance = state.Instances[action.Id];
            instance.LastModified = action.SuspendedAt;
            instance.Status = V1WorkflowInstanceStatus.Suspended;
            var instances = new Dictionary<string, V1WorkflowInstance>(state.Instances);
            return state with
            {
                Instances = instances
            };
        }

        /// <summary>
        /// Marks an instance as resuming
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="MarkV1WorkflowInstanceAsResuming"/> action to reduce</param>
        /// <returns></returns>
        public static WorkflowViewState On(WorkflowViewState state, MarkV1WorkflowInstanceAsResuming action)
        {
            if (state.Instances == null)
                return state;
            if (!state.Instances.ContainsKey(action.Id))
                return state;
            var instance = state.Instances[action.Id];
            instance.LastModified = action.ResumingAt;
            instance.Status = V1WorkflowInstanceStatus.Resuming;
            var instances = new Dictionary<string, V1WorkflowInstance>(state.Instances);
            return state with
            {
                Instances = instances
            };
        }

        /// <summary>
        /// Marks an instance as cancelling
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="MarkV1WorkflowInstanceAsCancelling"/> action to reduce</param>
        /// <returns></returns>
        public static WorkflowViewState On(WorkflowViewState state, MarkV1WorkflowInstanceAsCancelling action)
        {
            if (state.Instances == null)
                return state;
            if (!state.Instances.ContainsKey(action.Id))
                return state;
            var instance = state.Instances[action.Id];
            instance.LastModified = action.CancellingAt;
            instance.Status = V1WorkflowInstanceStatus.Cancelling;
            var instances = new Dictionary<string, V1WorkflowInstance>(state.Instances);
            return state with
            {
                Instances = instances
            };
        }

        /// <summary>
        /// Marks an instance as cancelled
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="MarkV1WorkflowInstanceAsCancelled"/> action to reduce</param>
        /// <returns></returns>
        public static WorkflowViewState On(WorkflowViewState state, MarkV1WorkflowInstanceAsCancelled action)
        {
            if (state.Instances == null)
                return state;
            if (!state.Instances.ContainsKey(action.Id))
                return state;
            var instance = state.Instances[action.Id];
            instance.LastModified = action.CancelledAt;
            instance.Status = V1WorkflowInstanceStatus.Cancelled;
            var instances = new Dictionary<string, V1WorkflowInstance>(state.Instances);
            return state with
            {
                Instances = instances
            };
        }

        /// <summary>
        /// Marks an instance as faulted
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="MarkV1WorkflowInstanceAsFaulted"/> action to reduce</param>
        /// <returns></returns>
        public static WorkflowViewState On(WorkflowViewState state, MarkV1WorkflowInstanceAsFaulted action)
        {
            if (state.Instances == null)
                return state;
            if (!state.Instances.ContainsKey(action.Id))
                return state;
            var instance = state.Instances[action.Id];
            instance.LastModified = action.FaultedAt;
            instance.ExecutedAt = action.FaultedAt;
            instance.Status = V1WorkflowInstanceStatus.Faulted;
            instance.Error = action.Error;
            var instances = new Dictionary<string, V1WorkflowInstance>(state.Instances);
            return state with
            {
                Instances = instances
            };
        }

        /// <summary>
        /// Marks an instance as completed
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="MarkV1WorkflowAsCompleted"/> action to reduce</param>
        /// <returns></returns>
        public static WorkflowViewState On(WorkflowViewState state, MarkV1WorkflowAsCompleted action)
        {
            if (state.Instances == null)
                return state;
            if (!state.Instances.ContainsKey(action.Id))
                return state;
            var instance = state.Instances[action.Id];
            instance.LastModified = action.CompletedAt;
            instance.ExecutedAt = action.CompletedAt;
            instance.Status = V1WorkflowInstanceStatus.Completed;
            instance.Output = action.Output;
            var instances = new Dictionary<string, V1WorkflowInstance>(state.Instances);
            return state with
            {
                Instances = instances
            };
        }

        /// <summary>
        /// Adds a workflow activity
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="AddV1WorkflowActivity"/> action to reduce</param>
        /// <returns></returns>
        public static WorkflowViewState On(WorkflowViewState state, AddV1WorkflowActivity action)
        {
            if (state.Instances == null)
                return state;
            if (!state.Instances.ContainsKey(action.WorkflowActivity.WorkflowInstanceId))
                return state;
            var activities = state.Activities != null ? new Dictionary<string, V1WorkflowActivity>(state.Activities) : new Dictionary<string, V1WorkflowActivity>();
            if (activities.ContainsKey(action.WorkflowActivity.Id))
                return state;
            activities.Add(action.WorkflowActivity.Id, action.WorkflowActivity);
            return state with
            {
                Activities = activities
            };
        }

        /// <summary>
        /// Marks an activity as started
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="MarkV1WorkflowActivityAsStarted"/> action to reduce</param>
        /// <returns></returns>
        public static WorkflowViewState On(WorkflowViewState state, MarkV1WorkflowActivityAsStarted action)
        {
            if (state.Activities == null)
                return state;
            if (!state.Activities.ContainsKey(action.Id))
                return state;
            var activity = state.Activities[action.Id];
            activity.LastModified = action.StartedAt;
            activity.StartedAt = action.StartedAt;
            activity.Status = V1WorkflowActivityStatus.Running;
            var activities = new Dictionary<string, V1WorkflowActivity>(state.Activities);
            return state with
            {
                Activities = activities
            };
        }

        /// <summary>
        /// Marks an activity as suspended
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="MarkV1WorkflowActivityAsSuspended"/> action to reduce</param>
        /// <returns></returns>
        public static WorkflowViewState On(WorkflowViewState state, MarkV1WorkflowActivityAsSuspended action)
        {
            if (state.Activities == null)
                return state;
            if (!state.Activities.ContainsKey(action.Id))
                return state;
            var activity = state.Activities[action.Id];
            activity.LastModified = action.SuspendedAt;
            activity.Status = V1WorkflowActivityStatus.Suspended;
            var activities = new Dictionary<string, V1WorkflowActivity>(state.Activities);
            return state with
            {
                Activities = activities
            };
        }

        /// <summary>
        /// Marks an activity as faulted
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="MarkV1WorkflowActivityAsFaulted"/> action to reduce</param>
        /// <returns></returns>
        public static WorkflowViewState On(WorkflowViewState state, MarkV1WorkflowActivityAsFaulted action)
        {
            if (state.Activities == null)
                return state;
            if (!state.Activities.ContainsKey(action.Id))
                return state;
            var activity = state.Activities[action.Id];
            activity.LastModified = action.FaultedAt;
            activity.Error = action.Error;
            activity.Status = V1WorkflowActivityStatus.Faulted;
            var activities = new Dictionary<string, V1WorkflowActivity>(state.Activities);
            return state with
            {
                Activities = activities
            };
        }

        /// <summary>
        /// Marks an activity as cancelled
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="MarkV1WorkflowActivityAsCancelled"/> action to reduce</param>
        /// <returns></returns>
        public static WorkflowViewState On(WorkflowViewState state, MarkV1WorkflowActivityAsCancelled action)
        {
            if (state.Activities == null)
                return state;
            if (!state.Activities.ContainsKey(action.Id))
                return state;
            var activity = state.Activities[action.Id];
            activity.LastModified = action.CancelledAt;
            activity.ExecutedAt = action.CancelledAt;
            activity.Status = V1WorkflowActivityStatus.Cancelled;
            var activities = new Dictionary<string, V1WorkflowActivity>(state.Activities);
            return state with
            {
                Activities = activities
            };
        }

        /// <summary>
        /// Marks an activity as executed
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="MarkV1WorkflowActivityAsExecuted"/> action to reduce</param>
        /// <returns></returns>
        public static WorkflowViewState On(WorkflowViewState state, MarkV1WorkflowActivityAsExecuted action)
        {
            if (state.Activities == null)
                return state;
            if (!state.Activities.ContainsKey(action.Id))
                return state;
            var activity = state.Activities[action.Id];
            activity.LastModified = action.ExecutedAt;
            activity.ExecutedAt = action.ExecutedAt;
            activity.Error = action.Error;
            activity.Output = action.Output;
            activity.Status = action.Status;
            var activities = new Dictionary<string, V1WorkflowActivity>(state.Activities);
            return state with
            {
                Activities = activities
            };
        }

        /// <summary>
        /// Marks an activity as compensating
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="MarkV1WorkflowActivityAsCompensating"/> action to reduce</param>
        /// <returns></returns>
        public static WorkflowViewState On(WorkflowViewState state, MarkV1WorkflowActivityAsCompensating action)
        {
            if (state.Activities == null)
                return state;
            if (!state.Activities.ContainsKey(action.Id))
                return state;
            var activity = state.Activities[action.Id];
            activity.LastModified = action.CompensatingAt;
            var activities = new Dictionary<string, V1WorkflowActivity>(state.Activities);
            return state with
            {
                Activities = activities
            };
        }

        /// <summary>
        /// Marks an activity as compensated
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="MarkV1WorkflowActivityAsCompensated"/> action to reduce</param>
        /// <returns></returns>
        public static WorkflowViewState On(WorkflowViewState state, MarkV1WorkflowActivityAsCompensated action)
        {
            if (state.Activities == null)
                return state;
            if (!state.Activities.ContainsKey(action.Id))
                return state;
            var activity = state.Activities[action.Id];
            activity.LastModified = action.CompensatedAt;
            activity.Status = V1WorkflowActivityStatus.Compensated;
            var activities = new Dictionary<string, V1WorkflowActivity>(state.Activities);
            return state with
            {
                Activities = activities
            };
        }

        /// <summary>
        /// Removes an activity
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="RemoveV1WorkflowActivity"/> action to reduce</param>
        /// <returns></returns>
        public static WorkflowViewState On(WorkflowViewState state, RemoveV1WorkflowActivity action)
        {
            if (state.Activities == null)
                return state;
            if (!state.Activities.ContainsKey(action.Id))
                return state;
            var activities = new Dictionary<string, V1WorkflowActivity>(state.Activities);
            activities.Remove(action.Id);
            return state with
            {
                Activities = activities
            };
        }
    }
}
