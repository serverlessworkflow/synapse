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
using Synapse.Apis.Management;
using Synapse.Integration.Models;

namespace Synapse.Dashboard
{

    [Feature]
    public class V1WorkflowInstanceCollectionState
        : List<V1WorkflowInstance>
    {
        public V1WorkflowInstanceCollectionState()
        {}
        public V1WorkflowInstanceCollectionState(IEnumerable<V1WorkflowInstance> workflows)
            : base(workflows)
        {}
    }

    [Reducer]
    public static class V1WorkflowInstanceCollectionReducers
    {

        public static V1WorkflowInstanceCollectionState OnGetWorkflowsFromApiSucceeded(V1WorkflowInstanceCollectionState state, SetV1WorkflowInstanceCollection action)
        {
            return new(action.WorkflowInstances);
        }

        public static V1WorkflowInstanceCollectionState OnAddV1WorkflowInstance(V1WorkflowInstanceCollectionState state, AddV1WorkflowInstance action)
        {
            state.Add(action.WorkflowInstance);
            return state;
        }

        public static V1WorkflowInstanceCollectionState OnMarkV1WorkflowInstanceAsStarting(V1WorkflowInstanceCollectionState state, MarkV1WorkflowInstanceAsStarting action)
        {
            var instance = state.FirstOrDefault(i => i.Id == action.Id);
            if(instance != null)
            {
                instance.LastModified = action.StartingAt;
                instance.Status = V1WorkflowInstanceStatus.Starting;
            }
            return state;
        }

        public static V1WorkflowInstanceCollectionState OnMarkV1WorkflowInstanceAsStarted(V1WorkflowInstanceCollectionState state, MarkV1WorkflowInstanceAsStarted action)
        {
            var instance = state.FirstOrDefault(i => i.Id == action.Id);
            if (instance != null)
            {
                instance.LastModified = action.StartedAt;
                instance.StartedAt = action.StartedAt;
                instance.Status = V1WorkflowInstanceStatus.Running;
            }
            return state;
        }

        public static V1WorkflowInstanceCollectionState OnMarkV1WorkflowInstanceAsSuspending(V1WorkflowInstanceCollectionState state, MarkV1WorkflowInstanceAsSuspending action)
        {
            var instance = state.FirstOrDefault(i => i.Id == action.Id);
            if (instance != null)
            {
                instance.LastModified = action.SuspendingAt;
                instance.Status = V1WorkflowInstanceStatus.Suspending;
            }
            return state;
        }

        public static V1WorkflowInstanceCollectionState OnMarkV1WorkflowInstanceAsSuspended(V1WorkflowInstanceCollectionState state, MarkV1WorkflowInstanceAsSuspended action)
        {
            var instance = state.FirstOrDefault(i => i.Id == action.Id);
            if (instance != null)
            {
                instance.LastModified = action.SuspendedAt;
                instance.Status = V1WorkflowInstanceStatus.Suspended;
            }
            return state;
        }

        public static V1WorkflowInstanceCollectionState OnMarkV1WorkflowInstanceAsResuming(V1WorkflowInstanceCollectionState state, MarkV1WorkflowInstanceAsResuming action)
        {
            var instance = state.FirstOrDefault(i => i.Id == action.Id);
            if (instance != null)
            {
                instance.LastModified = action.ResumingAt;
                instance.Status = V1WorkflowInstanceStatus.Resuming;
            }
            return state;
        }


        public static V1WorkflowInstanceCollectionState OnMarkV1WorkflowInstanceAsCancelling(V1WorkflowInstanceCollectionState state, MarkV1WorkflowInstanceAsCancelling action)
        {
            var instance = state.FirstOrDefault(i => i.Id == action.Id);
            if (instance != null)
            {
                instance.LastModified = action.CancellingAt;
                instance.Status = V1WorkflowInstanceStatus.Cancelling;
            }
            return state;
        }

        public static V1WorkflowInstanceCollectionState OnMarkV1WorkflowInstanceAsCancelled(V1WorkflowInstanceCollectionState state, MarkV1WorkflowInstanceAsCancelled action)
        {
            var instance = state.FirstOrDefault(i => i.Id == action.Id);
            if (instance != null)
            {
                instance.LastModified = action.CancelledAt;
                instance.Status = V1WorkflowInstanceStatus.Cancelled;
            }
            return state;
        }

        public static V1WorkflowInstanceCollectionState OnMarkV1WorkflowInstanceAsFaulted(V1WorkflowInstanceCollectionState state, MarkV1WorkflowInstanceAsFaulted action)
        {
            var instance = state.FirstOrDefault(i => i.Id == action.Id);
            if (instance != null)
            {
                instance.LastModified = action.FaultedAt;
                instance.ExecutedAt = action.FaultedAt;
                instance.Status = V1WorkflowInstanceStatus.Faulted;
                instance.Error = action.Error;
            }
            return state;
        }

        public static V1WorkflowInstanceCollectionState OnMarkV1WorkflowInstanceAsCompleted(V1WorkflowInstanceCollectionState state, MarkV1WorkflowAsCompleted action)
        {
            var instance = state.FirstOrDefault(i => i.Id == action.Id);
            if (instance != null)
            {
                instance.LastModified = action.CompletedAt;
                instance.ExecutedAt = action.CompletedAt;
                instance.Status = V1WorkflowInstanceStatus.Completed;
                instance.Output = action.Output;
            }
            return state;
        }

        public static V1WorkflowInstanceCollectionState OnRemoveV1WorkflowInstance(V1WorkflowInstanceCollectionState state, RemoveV1WorkflowInstance action)
        {
            var instance = state.FirstOrDefault(i => i.Id == action.Id);
            if (instance != null)
                state.Remove(instance);
            return state;
        }

    }

    [Effect]
    public static class V1WorkflowInstanceCollectionEffects
    {

        public static async Task OnListV1WorkflowInstancesByDefinition(ListV1WorkflowInstancesByDefinition action, IEffectContext context)
        {
            var api = context.Services.GetRequiredService<ISynapseManagementApi>();
            var workflowInstances = await api.GetWorkflowInstancesAsync($"$filter={nameof(V1WorkflowInstance.WorkflowId)} eq '{action.DefinitionId}'");
            context.Dispatcher.Dispatch(new SetV1WorkflowInstanceCollection(workflowInstances));
        }

        public static async Task OnSuspendV1WorkflowInstance(SuspendV1WorkflowInstance action, IEffectContext context)
        {
            var api = context.Services.GetRequiredService<ISynapseManagementApi>();
            await api.SuspendWorkflowInstanceAsync(action.InstanceId);
        }

        public static async Task OnResumeV1WorkflowInstance(ResumeV1WorkflowInstance action, IEffectContext context)
        {
            var api = context.Services.GetRequiredService<ISynapseManagementApi>();
            await api.ResumeWorkflowInstanceAsync(action.InstanceId);
        }
        
        public static async Task OnCancelV1WorkflowInstance(CancelV1WorkflowInstance action, IEffectContext context)
        {
            var api = context.Services.GetRequiredService<ISynapseManagementApi>();
            await api.CancelWorkflowInstanceAsync(action.InstanceId);
        }

        public static async Task OnDeleteV1WorkflowInstance(DeleteV1WorkflowInstance action, IEffectContext context)
        {
            var api = context.Services.GetRequiredService<ISynapseManagementApi>();
            await api.DeleteWorkflowInstanceAsync(action.InstanceId);
        }

    }

    public class SetV1WorkflowInstanceCollection
    {

        public SetV1WorkflowInstanceCollection(IEnumerable<V1WorkflowInstance> workflowInstances)
        {
            this.WorkflowInstances = workflowInstances;
        }

        public IEnumerable<V1WorkflowInstance> WorkflowInstances { get; }

    }

    public class AddV1WorkflowInstance
    {

        public AddV1WorkflowInstance(V1WorkflowInstance workflowInstance)
        {
            this.WorkflowInstance = workflowInstance;
        }

        public V1WorkflowInstance WorkflowInstance { get; }

    }

    public class MarkV1WorkflowInstanceAsStarting
    {

        public MarkV1WorkflowInstanceAsStarting(string id, DateTime startingAt)
        {
            this.Id = id;
            this.StartingAt = startingAt;
        }

        public string Id { get; }

        public DateTime StartingAt { get; }

    }

    public class MarkV1WorkflowInstanceAsStarted
    {

        public MarkV1WorkflowInstanceAsStarted(string id, DateTime startedAt)
        {
            this.Id = id;
            this.StartedAt = startedAt;
        }

        public string Id { get; }

        public DateTime StartedAt { get; }

    }

    public class MarkV1WorkflowInstanceAsSuspending
    {

        public MarkV1WorkflowInstanceAsSuspending(string id, DateTime suspendingAt)
        {
            this.Id = id;
            this.SuspendingAt = suspendingAt;
        }

        public string Id { get; }

        public DateTime SuspendingAt { get; }

    }

    public class MarkV1WorkflowInstanceAsSuspended
    {

        public MarkV1WorkflowInstanceAsSuspended(string id, DateTime suspendedAt)
        {
            this.Id = id;
            this.SuspendedAt = suspendedAt;
        }

        public string Id { get; }

        public DateTime SuspendedAt { get; }

    }

    public class MarkV1WorkflowInstanceAsResuming
    {

        public MarkV1WorkflowInstanceAsResuming(string id, DateTime resumingAt)
        {
            this.Id = id;
            this.ResumingAt = resumingAt;
        }

        public string Id { get; }

        public DateTime ResumingAt { get; }

    }

    public class MarkV1WorkflowInstanceAsCancelling
    {

        public MarkV1WorkflowInstanceAsCancelling(string id, DateTime cancellingAt)
        {
            this.Id = id;
            this.CancellingAt = cancellingAt;
        }

        public string Id { get; }

        public DateTime CancellingAt { get; }

    }

    public class MarkV1WorkflowInstanceAsCancelled
    {

        public MarkV1WorkflowInstanceAsCancelled(string id, DateTime cancelledAt)
        {
            this.Id = id;
            this.CancelledAt = cancelledAt;
        }

        public string Id { get; }

        public DateTime CancelledAt { get; }

    }

    public class MarkV1WorkflowInstanceAsFaulted
    {

        public MarkV1WorkflowInstanceAsFaulted(string id, DateTime faultedAt, Error error)
        {
            this.Id = id;
            this.FaultedAt = faultedAt;
            this.Error = error;
        }

        public string Id { get; }

        public DateTime FaultedAt { get; }

        public Error Error { get; }

    }

    public class MarkV1WorkflowAsCompleted
    {

        public MarkV1WorkflowAsCompleted(string id, DateTime completedAt, Neuroglia.Serialization.Dynamic output)
        {
            this.Id = id;
            this.CompletedAt = completedAt;
            this.Output = output;
        }

        public string Id { get; }

        public DateTime CompletedAt { get; }

        public Neuroglia.Serialization.Dynamic Output { get; }

    }

    public class RemoveV1WorkflowInstance
    {

        public RemoveV1WorkflowInstance(string id)
        {
            this.Id = id;
        }

        public string Id { get; }

    }

    public class ListV1WorkflowInstancesByDefinition
    {

        public ListV1WorkflowInstancesByDefinition(string definitionId)
        {
            this.DefinitionId = definitionId;
        }

        public string DefinitionId { get; }

    }

    public class SuspendV1WorkflowInstance
    {

        public SuspendV1WorkflowInstance(string instanceId)
        {
            this.InstanceId = instanceId;
        }

        public string InstanceId { get; }
    }

    public class ResumeV1WorkflowInstance
    {

        public ResumeV1WorkflowInstance(string instanceId)
        {
            this.InstanceId = instanceId;
        }

        public string InstanceId { get; }
    }

    public class CancelV1WorkflowInstance
    {

        public CancelV1WorkflowInstance(string instanceId)
        {
            this.InstanceId = instanceId;
        }

        public string InstanceId { get; }
    }

    public class DeleteV1WorkflowInstance
    {

        public DeleteV1WorkflowInstance(string instanceId)
        {
            this.InstanceId = instanceId;
        }

        public string InstanceId { get; }

    }

}
