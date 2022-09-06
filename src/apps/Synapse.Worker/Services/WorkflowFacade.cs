/*
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

using CloudNative.CloudEvents;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using Synapse.Apis.Runtime;
using Synapse.Integration.Events;
using Synapse.Integration.Events.WorkflowActivities;

namespace Synapse.Worker.Services
{

    /// <summary>
    /// Represents the default implementation of the <see cref="IWorkflowFacade"/> interface
    /// </summary>
    public class WorkflowFacade
        : IWorkflowFacade
    {

        /// <summary>
        /// Initializes a new <see cref="WorkflowFacade"/>
        /// </summary>
        /// <param name="definition">The current workflow's <see cref="WorkflowDefinition"/></param>
        /// <param name="instance">The current <see cref="V1WorkflowInstance"/></param>
        /// <param name="synapseRuntimeApi">The service used to interact with the Synapse Runtime API</param>
        public WorkflowFacade(WorkflowDefinition definition, V1WorkflowInstance instance, ISynapseRuntimeApi synapseRuntimeApi)
        {
            this.Definition = definition;
            this.Instance = instance;
            this.SynapseRuntimeApi = synapseRuntimeApi;
        }

        /// <inheritdoc/>
        public WorkflowDefinition Definition { get; }

        /// <inheritdoc/>
        public V1WorkflowInstance Instance { get; private set; }

        /// <summary>
        /// Gets the service used to interact with the Synapse Runtime API
        /// </summary>
        protected ISynapseRuntimeApi SynapseRuntimeApi { get; }

        /// <inheritdoc/>
        public virtual async Task StartAsync(CancellationToken cancellationToken)
        {
            this.Instance = await this.SynapseRuntimeApi.StartAsync(this.Instance.Id, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<CloudEvent?> ConsumeOrBeginCorrelateEventAsync(EventDefinition eventDefinition, CancellationToken cancellationToken = default)
        {
            if(eventDefinition == null)
                throw new ArgumentNullException(nameof(eventDefinition));
            var e = await this.SynapseRuntimeApi.ConsumeOrBeginCorrelateEventAsync(new() { WorkflowInstanceId = this.Instance.Id, EventDefinition = eventDefinition }, cancellationToken);
            if (e == null)
                return null;
            else
                return e.ToCloudEvent();
        }

        /// <inheritdoc/>
        public virtual async Task SetCorrelationMappingAsync(string key, string value, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value));
            await this.SynapseRuntimeApi.SetCorrelationMappingAsync(new() { Id = this.Instance.Id, Key = key, Value = value }, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<bool> TryCorrelateAsync(V1Event e, IEnumerable<string>? mappingKeys, CancellationToken cancellationToken = default)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            if (mappingKeys == null)
                mappingKeys = Array.Empty<string>();
            return await this.SynapseRuntimeApi.TryCorrelateAsync(new() { WorkflowInstanceId = this.Instance.Id, Event = e, MappingKeys = mappingKeys }, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowActivity>> GetActivitiesAsync(CancellationToken cancellationToken = default)
        {
            return await this.SynapseRuntimeApi.GetActivitiesAsync(this.Instance.Id, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowActivity>> GetOperativeActivitiesAsync(CancellationToken cancellationToken)
        {
            return await this.SynapseRuntimeApi.GetOperativeActivitiesAsync(this.Instance.Id, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowActivity>> GetActivitiesAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default)
        {
            return await this.SynapseRuntimeApi.GetActivitiesAsync(this.Instance.Id, activity.Id, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowActivity>> GetOperativeActivitiesAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default)
        {
            return await this.SynapseRuntimeApi.GetOperativeActivitiesAsync(this.Instance.Id, activity.Id, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> CreateActivityAsync(V1WorkflowActivityType type, object? input, IDictionary<string, string>? metadata, V1WorkflowActivity? parent, CancellationToken cancellationToken)
        {
            var inputParam = input as Dynamic;
            if (inputParam == null && input != null)
                inputParam = Dynamic.FromObject(input);
            var metadataParam = metadata == null ? null : new NameValueCollection<string>(metadata);
            return await this.SynapseRuntimeApi.CreateActivityAsync(new() { WorkflowInstanceId = this.Instance.Id, Type = type, Input = inputParam, Metadata = metadataParam, ParentId = parent?.Id }, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task InitializeActivityAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            //await this.SynapseRuntimeApi.InitializeActivityAsync(activity.Id, cancellationToken);
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual async Task StartOrResumeActivityAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            await this.SynapseRuntimeApi.StartActivityAsync(activity.Id, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task SuspendActivityAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            await this.SynapseRuntimeApi.SuspendActivityAsync(activity.Id, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task SkipActivityAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            await this.SynapseRuntimeApi.SkipActivityAsync(activity.Id, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task SetActivityMetadataAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            await this.SynapseRuntimeApi.SetActivityMetadataAsync(new() { Id = activity.Id, Metadata = activity.Metadata }, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task FaultActivityAsync(V1WorkflowActivity activity, Exception ex, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            if (ex == null)
                throw new ArgumentNullException(nameof(ex));
            await this.FaultActivityAsync(activity, new Integration.Models.Error() { Code = ex.GetType().Name.Replace("exception", string.Empty, StringComparison.OrdinalIgnoreCase), Message = ex.Message }, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task FaultActivityAsync(V1WorkflowActivity activity, Integration.Models.Error error, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            if (error == null)
                throw new ArgumentNullException(nameof(error));
            await this.SynapseRuntimeApi.FaultActivityAsync(new() { Id = activity.Id, Error = error }, cancellationToken);;
        }

        /// <inheritdoc/>
        public virtual async Task CompensateActivityAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            await this.SynapseRuntimeApi.CompensateActivityAsync(new() { Id = activity.Id }, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task MarkActivityAsCompensatedAsync(string activityId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(activityId))
                throw new ArgumentNullException(nameof(activityId));
            await this.SynapseRuntimeApi.MarkActivityAsCompensatedAsync(new() { Id = activityId }, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task CancelActivityAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            await this.SynapseRuntimeApi.CancelActivityAsync(activity.Id, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task SetActivityOutputAsync(V1WorkflowActivity activity, object? output, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            var outputParam = output as Dynamic;
            if (outputParam == null && output != null)
                outputParam = Dynamic.FromObject(output);
            await this.SynapseRuntimeApi.SetActivityOutputAsync(new() { Id = activity.Id, Output = outputParam }, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<object> GetActivityStateDataAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default)
        {
            var result = await this.SynapseRuntimeApi.GetActivityStateDataAsync(activity.Id, cancellationToken);
            return result.ToObject()!;
        }

        /// <inheritdoc/>
        public virtual async Task<string> StartSubflowAsync(string workflowId, object? input, CancellationToken cancellationToken = default)
        {
            var inputData = input as Dynamic;
            if (inputData == null && input != null)
                inputData = Dynamic.FromObject(input);
            var workflowInstance = await this.SynapseRuntimeApi.StartSubflowAsync(new() { WorkflowId = workflowId, InputData = inputData, ActivationType = V1WorkflowInstanceActivationType.Subflow, ParentId = this.Instance.Id, AutoStart = true }, cancellationToken);
            return workflowInstance.Id;
        }

        /// <inheritdoc/>
        public virtual async Task SuspendAsync(CancellationToken cancellationToken = default)
        {
            this.Instance = await this.SynapseRuntimeApi.SuspendAsync(this.Instance.Id, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task CancelAsync(CancellationToken cancellationToken = default)
        {
            this.Instance = await this.SynapseRuntimeApi.CancelAsync(this.Instance.Id, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task TransitionToAsync(StateDefinition state, CancellationToken cancellationToken = default)
        {
            //await this.Synapse.TransitionWorkflowInstanceToAsync(this.Instance.Id, state.Name, cancellationToken); //todo
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual async Task FaultAsync(Exception ex, CancellationToken cancellationToken)
        {
            this.Instance = await this.SynapseRuntimeApi.FaultAsync(new() { Id = this.Instance.Id, Error = new() { Code = ex.GetType().Name.Replace("exception", string.Empty, StringComparison.InvariantCultureIgnoreCase), Message = ex.Message } }, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task SetOutputAsync(object? output, CancellationToken cancellationToken = default)
        {
            var outputData = output as Dynamic;
            if (outputData == null && output != null)
                outputData = Dynamic.FromObject(output);
            this.Instance = await this.SynapseRuntimeApi.SetOutputAsync(new() { Id = this.Instance.Id, Output = outputData }, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task On(V1WorkflowActivity activity, IV1WorkflowActivityIntegrationEvent e, CancellationToken cancellationToken = default)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            switch (e)
            {
                case V1WorkflowActivityStartedIntegrationEvent:
                case V1WorkflowActivityResumedIntegrationEvent:
                    await this.StartOrResumeActivityAsync(activity, cancellationToken);
                    break;
                case V1WorkflowActivitySuspendedIntegrationEvent:
                    await this.SuspendActivityAsync(activity, cancellationToken);
                    break;
                case V1WorkflowActivitySkippedIntegrationEvent:
                    await this.SkipActivityAsync(activity, cancellationToken);
                    break;
                case V1WorkflowActivityFaultedIntegrationEvent faultedEvent:
                    await this.FaultActivityAsync(activity, faultedEvent.Error, cancellationToken);
                    break;
                case V1WorkflowActivityCancelledIntegrationEvent:
                    await this.CancelActivityAsync(activity, cancellationToken);
                    break;
                case V1WorkflowActivityCompletedIntegrationEvent completedEvent:
                    await this.SetActivityOutputAsync(activity, completedEvent.Output, cancellationToken);
                    break;
                default:
                    throw new NotSupportedException($"The specified workflow activity integration event type '{e.GetType().Name}' is not supported in this context");
            }
        }

    }

}
