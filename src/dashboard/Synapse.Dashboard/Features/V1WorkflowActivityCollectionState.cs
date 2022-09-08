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
using Synapse.Integration.Models;
using System.Collections.ObjectModel;

namespace Synapse.Dashboard
{

    [Feature]
    public class V1WorkflowActivityCollectionState
        : List<V1WorkflowActivity>
    {
        public V1WorkflowActivityCollectionState()
        {}

        public V1WorkflowActivityCollectionState(IEnumerable<V1WorkflowActivity> activities)
            : base(activities)
        {}
    }

    public class SetV1WorkflowActivityCollection
    {

        public SetV1WorkflowActivityCollection(IEnumerable<V1WorkflowActivity> workflowActivities)
        {
            this.WorkflowActivities = workflowActivities;
        }

        public IEnumerable<V1WorkflowActivity> WorkflowActivities { get; }

    }

    public class AddV1WorkflowActivity
    {

        public AddV1WorkflowActivity(V1WorkflowActivity workflowActivity)
        {
            this.WorkflowActivity = workflowActivity;
        }

        public V1WorkflowActivity WorkflowActivity { get; }

    }

    public class MarkV1WorkflowActivityAsStarted
    {

        public MarkV1WorkflowActivityAsStarted(string id, DateTime startedAt)
        {
            this.Id = id;
            this.StartedAt = startedAt;
        }

        public string Id { get; }

        public DateTime StartedAt { get; }

    }

    public class MarkV1WorkflowActivityAsSuspended
    {

        public MarkV1WorkflowActivityAsSuspended(string id, DateTime suspendedAt)
        {
            this.Id = id;
            this.SuspendedAt = suspendedAt;
        }

        public string Id { get; }

        public DateTime SuspendedAt { get; }

    }

    public class MarkV1WorkflowActivityAsFaulted
    {

        public MarkV1WorkflowActivityAsFaulted(string id, DateTime faultedAt, Error error)
        {
            this.Id = id;
            this.FaultedAt = faultedAt;
            this.Error = error;
        }

        public string Id { get; }

        public DateTime FaultedAt { get; }

        public Error Error { get; }

    }

    public class MarkV1WorkflowActivityAsCancelled
    {

        public MarkV1WorkflowActivityAsCancelled(string id, DateTime cancelledAt)
        {
            this.Id = id;
            this.CancelledAt = cancelledAt;
        }

        public string Id { get; }

        public DateTime CancelledAt { get; }

    }

    public class MarkV1WorkflowActivityAsExecuted
    {

        public MarkV1WorkflowActivityAsExecuted(string id, DateTime executedAt, V1WorkflowActivityStatus status, Error? error, Neuroglia.Serialization.Dynamic? output)
        {
            this.Id = id;
            this.ExecutedAt = executedAt;
            this.Status = status;
            this.Error = error;
            this.Output = output;
        }

        public string Id { get; }

        public DateTime ExecutedAt { get; }

        public V1WorkflowActivityStatus Status { get; }

        public Error? Error { get; }

        public Neuroglia.Serialization.Dynamic? Output { get; }

    }

    public class MarkV1WorkflowActivityAsCompensating
    {

        public MarkV1WorkflowActivityAsCompensating(string id, DateTime compensatingAt)
        {
            this.Id = id;
            this.CompensatingAt = compensatingAt;
        }

        public string Id { get; }

        public DateTime CompensatingAt { get; }

    }

    public class MarkV1WorkflowActivityAsCompensated
    {

        public MarkV1WorkflowActivityAsCompensated(string id, DateTime compensatedAt)
        {
            this.Id = id;
            this.CompensatedAt = compensatedAt;
        }

        public string Id { get; }

        public DateTime CompensatedAt { get; }

    }

    public class RemoveV1WorkflowActivity
    {

        public RemoveV1WorkflowActivity(string id)
        {
            this.Id = id;
        }

        public string Id { get; }

    }

    public class ListV1WorkflowInstanceActivities
    {

        public ListV1WorkflowInstanceActivities(string definitionId)
        {
            this.DefinitionId = definitionId;
        }

        public string DefinitionId { get; }

    }

    [Reducer]
    public static class V1WorkflowActivityCollectionReducers
    {

        public static V1WorkflowActivityCollectionState OnGetWorkflowsFromApiSucceeded(V1WorkflowActivityCollectionState state, SetV1WorkflowInstanceCollection action)
        {
            var activities = action.WorkflowInstances.SelectMany(instance => instance.Activities ?? new Collection<V1WorkflowActivity>());
            var activityIds = activities.Select(activity => activity.Id).ToList();
            activities = activities.Concat(state.Where(activity => !activityIds.Contains(activity.Id)));
            return new(activities);
        }

        public static V1WorkflowActivityCollectionState OnAddV1WorkflowInstance(V1WorkflowActivityCollectionState state, AddV1WorkflowInstance action)
        {
            var activities = (action.WorkflowInstance.Activities ?? new Collection<V1WorkflowActivity>()).AsEnumerable();
            var activityIds = activities.Select(activity => activity.Id).ToList();
            activities = activities.Concat(state.Where(activity => !activityIds.Contains(activity.Id)));
            return new(activities);
        }

        public static V1WorkflowActivityCollectionState OnAddV1WorkflowActivity(V1WorkflowActivityCollectionState state, AddV1WorkflowActivity action)
        {
            state.Add(action.WorkflowActivity);
            return state;
        }

        public static V1WorkflowActivityCollectionState OnMarkV1WorkflowActivityAsStarted(V1WorkflowActivityCollectionState state, MarkV1WorkflowActivityAsStarted action)
        {
            var activity = state.FirstOrDefault(a => a.Id == action.Id);
            if (activity != null)
            {
                activity.LastModified = action.StartedAt;
                activity.StartedAt = action.StartedAt;
                activity.Status = V1WorkflowActivityStatus.Running;
            }
            return state;
        }

        public static V1WorkflowActivityCollectionState OnMarkV1WorkflowActivityAsSuspended(V1WorkflowActivityCollectionState state, MarkV1WorkflowActivityAsSuspended action)
        {
            var activity = state.FirstOrDefault(a => a.Id == action.Id);
            if (activity != null)
            {
                activity.LastModified = action.SuspendedAt;
                activity.Status = V1WorkflowActivityStatus.Suspended;
            }
            return state;
        }

        public static V1WorkflowActivityCollectionState OnMarkV1WorkflowActivityAsFaulted(V1WorkflowActivityCollectionState state, MarkV1WorkflowActivityAsFaulted action)
        {
            var activity = state.FirstOrDefault(a => a.Id == action.Id);
            if (activity != null)
            {
                activity.LastModified = action.FaultedAt;
                activity.Error = action.Error;
                activity.Status = V1WorkflowActivityStatus.Faulted;
            }
            return state;
        }

        public static V1WorkflowActivityCollectionState OnMarkV1WorkflowActivityAsCancelled(V1WorkflowActivityCollectionState state, MarkV1WorkflowActivityAsCancelled action)
        {
            var activity = state.FirstOrDefault(a => a.Id == action.Id);
            if (activity != null)
            {
                activity.LastModified = action.CancelledAt;
                activity.ExecutedAt = action.CancelledAt;
                activity.Status = V1WorkflowActivityStatus.Cancelled;
            }
            return state;
        }

        public static V1WorkflowActivityCollectionState OnMarkV1WorkflowActivityAsExecuted(V1WorkflowActivityCollectionState state, MarkV1WorkflowActivityAsExecuted action)
        {
            var activity = state.FirstOrDefault(a => a.Id == action.Id);
            if (activity != null)
            {
                activity.LastModified = action.ExecutedAt;
                activity.ExecutedAt = action.ExecutedAt;
                activity.Error = action.Error;
                activity.Output = action.Output;
                activity.Status = action.Status;
            }
            return state;
        }

        public static V1WorkflowActivityCollectionState OnMarkV1WorkflowActivityAsCompensating(V1WorkflowActivityCollectionState state, MarkV1WorkflowActivityAsCompensating action)
        {
            var activity = state.FirstOrDefault(a => a.Id == action.Id);
            if (activity != null)
            {
                activity.LastModified = action.CompensatingAt;
            }
            return state;
        }

        public static V1WorkflowActivityCollectionState OnMarkV1WorkflowActivityAsCompensated(V1WorkflowActivityCollectionState state, MarkV1WorkflowActivityAsCompensated action)
        {
            var activity = state.FirstOrDefault(a => a.Id == action.Id);
            if (activity != null)
            {
                activity.LastModified = action.CompensatedAt;
                activity.Status = V1WorkflowActivityStatus.Compensated;
            }
            return state;
        }

        public static V1WorkflowActivityCollectionState OnRemoveV1WorkflowActivity(V1WorkflowActivityCollectionState state, RemoveV1WorkflowActivity action)
        {
            var activity = state.FirstOrDefault(a => a.Id == action.Id);
            if (activity != null)
                state.Remove(activity);
            return state;
        }

    }

}
