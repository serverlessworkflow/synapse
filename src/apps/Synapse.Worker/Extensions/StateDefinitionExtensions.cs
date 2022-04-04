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

namespace Synapse.Worker
{
    /// <summary>
    /// Defines extensions for <see cref="StateDefinition"/>s
    /// </summary>
    public static class StateDefinitionExtensions
    {

        /// <summary>
        /// Gets the <see cref="ActionDefinition"/> described by the specified metadata
        /// </summary>
        /// <param name="state">The <see cref="StateDefinition"/> the <see cref="ActionDefinition"/> to get belongs to</param>
        /// <param name="metadata">An <see cref="IDictionary{TKey, TValue}"/> that describes the <see cref="ActionDefinition"/> to get</param>
        /// <returns>The <see cref="ActionDefinition"/> described by the specified metadata</returns>
        public static ActionDefinition GetAction(this StateDefinition state, IDictionary<string, string> metadata)
        {
            ActionDefinition action;
            if (!metadata.TryGetValue(V1WorkflowActivityMetadata.Action, out var actionName))
                throw new ArgumentException($"The metadata is missing the required metadata field '{V1WorkflowActivityMetadata.Action}'");
            switch (state)
            {
                case OperationStateDefinition operationState:
                    if (!operationState.TryGetAction(actionName, out action))
                        throw new NullReferenceException($"Failed to find an action with the specified name '{actionName}' in the state with name '{state.Name}'");
                    break;
                case ForEachStateDefinition forEachState:
                    if (!forEachState.TryGetAction(actionName, out action))
                        throw new NullReferenceException($"Failed to find an action with the specified name '{actionName}' in the state with name '{state.Name}'");
                    break;
                case CallbackStateDefinition callbackState:
                    action = callbackState.Action!;
                    break;
                case ParallelStateDefinition parallelState:
                    if (!metadata.TryGetValue(V1WorkflowActivityMetadata.Branch, out var branchName))
                        throw new ArgumentException($"The metadata is missing the required metadata field '{V1WorkflowActivityMetadata.Branch}'");
                    if (!parallelState.TryGetBranch(branchName, out var branch))
                        throw new NullReferenceException($"Failed to find a branch with the specified name '{branchName}' in the state with name '{state.Name}'");
                    if (!branch.TryGetAction(actionName, out action))
                        throw new NullReferenceException($"Failed to find an action with the specified name '{action.Name}' in the branch with name '{branch.Name}'");
                    break;
                case EventStateDefinition eventState:
                    if (!metadata.TryGetValue(V1WorkflowActivityMetadata.Trigger, out var triggerId))
                        throw new ArgumentException($"The metadata is missing the required metadata field '{V1WorkflowActivityMetadata.Trigger}'");
                    if (!int.TryParse(triggerId, out var triggerIndex))
                        throw new ArgumentException($"The metadata has an unexpected value '{triggerId}' set for the metadata field '{V1WorkflowActivityMetadata.Trigger}'");
                    if (!eventState.TryGetTrigger(triggerIndex, out var trigger))
                        throw new NullReferenceException($"Failed to find an event state trigger at the specified index '{triggerIndex}' in the state with name'{state.Name}'");
                    if (!trigger.TryGetAction(actionName, out action))
                        throw new NullReferenceException($"Failed to find an action with the specified name '{action.Name}' in the trigger at the specified index '{triggerIndex}'");
                    break;
                default:
                    throw new NotSupportedException($"The specified {nameof(StateDefinition)} type '{state.GetType().Name}' is not supported in this context");
            }
            return action;
        }

        /// <summary>
        /// Attempts to get the <see cref="ActionDefinition"/> described by the specified metadata
        /// </summary>
        /// <param name="state">The <see cref="StateDefinition"/> the <see cref="ActionDefinition"/> to get belongs to</param>
        /// <param name="metadata">The metadata that describes the <see cref="ActionDefinition"/> to get</param>
        /// <param name="action">The <see cref="ActionDefinition"/> described by the specified metadata</param>
        /// <returns>A boolean indicating whether or not the action described bv the specified metadata could be resolved</returns>
        public static bool TryGetAction(this StateDefinition state, IDictionary<string, string> metadata, out ActionDefinition action)
        {
            action = null!;
            try
            {
                action = state.GetAction(metadata);
                return action != null;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

    }

}