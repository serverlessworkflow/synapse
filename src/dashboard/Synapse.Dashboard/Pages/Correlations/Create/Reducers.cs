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

using Neuroglia.Data.Flux;
using Synapse.Integration.Models;

namespace Synapse.Dashboard.Pages.Correlations.Create;

/// <summary>
/// Defines Flux reducers for <see cref="CreateCorrelationState"/>-related actions
/// </summary>
[Reducer]
public static class Reducers
{

    /// <summary>
    /// Handles the specified <see cref="InitializeState"/>
    /// </summary>
    /// <param name="state">The state to reduce</param>
    /// <param name="action">The <see cref="InitializeState"/> action to reduce</param>
    /// <returns>The reduced state</returns>
    public static CreateCorrelationState On(CreateCorrelationState state, InitializeState action)
    {
        if (state.Initialized && !action.Reset) return state with { Saving = false };
        return state with
        {
            Initialized = true,
            Saving = false,
            Command = new()
            {
                ActivationType = V1CorrelationActivationType.Explicit,
                Conditions = new List<V1CorrelationCondition>()
                {
                    new()
                    {
                        Filters = new List<V1EventFilter>()
                        {
                            new()
                            {
                                Attributes  = new(),
                                CorrelationMappings = new()
                            }
                        } 
                    } 
                }, 
                Outcome = new() 
                { 
                    Type = V1CorrelationOutcomeType.Start 
                } 
            } 
        };
    }

    /// <summary>
    /// Handles the specified <see cref="AddConditionToCorrelation"/>
    /// </summary>
    /// <param name="state">The state to reduce</param>
    /// <param name="action">The <see cref="AddConditionToCorrelation"/> action to reduce</param>
    /// <returns>The reduced state</returns>
    public static CreateCorrelationState On(CreateCorrelationState state, AddConditionToCorrelation action)
    {
        if (state.Command == null) return state;
        var command = state.Command.Clone()!;
        if (command.Conditions == null) command.Conditions = new List<V1CorrelationCondition>();
        command.Conditions.Add(new() { Filters = new List<V1EventFilter>() { new() } });
        return state with { Command = command };
    }

    /// <summary>
    /// Handles the specified <see cref="RemoveConditionFromCorrelation"/>
    /// </summary>
    /// <param name="state">The state to reduce</param>
    /// <param name="action">The <see cref="RemoveConditionFromCorrelation"/> action to reduce</param>
    /// <returns>The reduced state</returns>
    public static CreateCorrelationState On(CreateCorrelationState state, RemoveConditionFromCorrelation action)
    {
        if (state.Command == null) return state;
        var conditionIndex = state.Command.Conditions.ToList().IndexOf(action.Condition);
        if (conditionIndex < 0) return state;
        var command = state.Command.Clone()!;
        if (command.Conditions == null) return state;
        var condition = command.Conditions.ElementAt(conditionIndex);
        command.Conditions.Remove(condition);
        return state with { Command = command };
    }

    /// <summary>
    /// Handles the specified <see cref="AddFilterToCorrelationCondition"/>
    /// </summary>
    /// <param name="state">The state to reduce</param>
    /// <param name="action">The <see cref="AddFilterToCorrelationCondition"/> action to reduce</param>
    /// <returns>The reduced state</returns>
    public static CreateCorrelationState On(CreateCorrelationState state, AddFilterToCorrelationCondition action)
    {
        if (state.Command == null) return state;
        var conditionIndex = state.Command.Conditions.ToList().IndexOf(action.Condition);
        if(conditionIndex < 0) return state;
        var command = state.Command.Clone()!;
        var condition = command.Conditions.ElementAt(conditionIndex);
        if (condition.Filters == null) condition.Filters = new List<V1EventFilter>();
        condition.Filters.Add(new() { Attributes = new(), CorrelationMappings = new() });
        return state with { Command = command };
    }

    /// <summary>
    /// Handles the specified <see cref="RemoveFilterFromCorrelationCondition"/>
    /// </summary>
    /// <param name="state">The state to reduce</param>
    /// <param name="action">The <see cref="RemoveFilterFromCorrelationCondition"/> action to reduce</param>
    /// <returns>The reduced state</returns>
    public static CreateCorrelationState On(CreateCorrelationState state, RemoveFilterFromCorrelationCondition action)
    {
        if (state.Command == null) return state;
        var conditionIndex = state.Command.Conditions.ToList().IndexOf(action.Condition);
        if (conditionIndex < 0) return state;
        var filterIndex = state.Command.Conditions.ElementAt(conditionIndex).Filters.ToList().IndexOf(action.Filter);
        if (filterIndex < 0) return state;
        var command = state.Command.Clone()!;
        var condition = command.Conditions.ElementAt(conditionIndex);
        if (condition.Filters == null) return state;
        var filter = condition.Filters.ElementAt(filterIndex);
        condition.Filters.Remove(filter);
        return state with { Command = command };
    }

    /// <summary>
    /// Handles the specified <see cref="PatchCorrelation"/>
    /// </summary>
    /// <param name="state">The state to reduce</param>
    /// <param name="action">The <see cref="PatchCorrelation"/> action to reduce</param>
    /// <returns>The reduced state</returns>
    public static CreateCorrelationState On(CreateCorrelationState state, PatchCorrelation action)
    {
        if (state.Command == null) return state;
        var command = state.Command.Clone()!;
        action.Patch(command);
        return state with { Command = command };
    }

    /// <summary>
    /// Handles the specified <see cref="AddOrUpdateConditionFilterAttribute"/>
    /// </summary>
    /// <param name="state">The state to reduce</param>
    /// <param name="action">The <see cref="AddOrUpdateConditionFilterAttribute"/> action to reduce</param>
    /// <returns>The reduced state</returns>
    public static CreateCorrelationState On(CreateCorrelationState state, AddOrUpdateConditionFilterAttribute action)
    {
        if (state.Command == null) return state;
        var conditionIndex = state.Command.Conditions.ToList().IndexOf(action.Condition);
        if (conditionIndex < 0) return state;
        var filterIndex = state.Command.Conditions.ElementAt(conditionIndex).Filters.ToList().IndexOf(action.Filter);
        if (filterIndex < 0) return state;
        var command = state.Command.Clone()!;
        var condition = command.Conditions.ElementAt(conditionIndex);
        if (condition.Filters == null) return state;
        var filter = condition.Filters.ElementAt(filterIndex);
        if (filter.Attributes == null) filter.Attributes = new();
        filter.Attributes[action.AttributeName.ToLowerInvariant()] = action.AttributeValue;
        return state with { Command = command };
    }

    /// <summary>
    /// Handles the specified <see cref="RemoveAttributeFromConditionFilter"/>
    /// </summary>
    /// <param name="state">The state to reduce</param>
    /// <param name="action">The <see cref="RemoveAttributeFromConditionFilter"/> action to reduce</param>
    /// <returns>The reduced state</returns>
    public static CreateCorrelationState On(CreateCorrelationState state, RemoveAttributeFromConditionFilter action)
    {
        if (state.Command == null) return state;
        var conditionIndex = state.Command.Conditions.ToList().IndexOf(action.Condition);
        if (conditionIndex < 0) return state;
        var filterIndex = state.Command.Conditions.ElementAt(conditionIndex).Filters.ToList().IndexOf(action.Filter);
        if (filterIndex < 0) return state;
        var command = state.Command.Clone()!;
        var condition = command.Conditions.ElementAt(conditionIndex);
        if (condition.Filters == null) return state;
        var filter = condition.Filters.ElementAt(filterIndex);
        if (!filter.Attributes.Remove(action.AttributeName)) return state;
        return state with { Command = command };
    }

    /// <summary>
    /// Handles the specified <see cref="AddOrUpdateConditionFilterCorrelationMapping"/>
    /// </summary>
    /// <param name="state">The state to reduce</param>
    /// <param name="action">The <see cref="AddOrUpdateConditionFilterCorrelationMapping"/> action to reduce</param>
    /// <returns>The reduced state</returns>
    public static CreateCorrelationState On(CreateCorrelationState state, AddOrUpdateConditionFilterCorrelationMapping action)
    {
        if (state.Command == null) return state;
        var conditionIndex = state.Command.Conditions.ToList().IndexOf(action.Condition);
        if (conditionIndex < 0) return state;
        var filterIndex = state.Command.Conditions.ElementAt(conditionIndex).Filters.ToList().IndexOf(action.Filter);
        if (filterIndex < 0) return state;
        var command = state.Command.Clone()!;
        var condition = command.Conditions.ElementAt(conditionIndex);
        if (condition.Filters == null) return state;
        var filter = condition.Filters.ElementAt(filterIndex);
        if (filter.CorrelationMappings == null) filter.CorrelationMappings = new();
        filter.CorrelationMappings[action.AttributeName.ToLowerInvariant()] = action.AttributeValue;
        return state with { Command = command };
    }

    /// <summary>
    /// Handles the specified <see cref="RemoveCorrelationMappingFromConditionFilter"/>
    /// </summary>
    /// <param name="state">The state to reduce</param>
    /// <param name="action">The <see cref="RemoveCorrelationMappingFromConditionFilter"/> action to reduce</param>
    /// <returns>The reduced state</returns>
    public static CreateCorrelationState On(CreateCorrelationState state, RemoveCorrelationMappingFromConditionFilter action)
    {
        if (state.Command == null) return state;
        var conditionIndex = state.Command.Conditions.ToList().IndexOf(action.Condition);
        if (conditionIndex < 0) return state;
        var filterIndex = state.Command.Conditions.ElementAt(conditionIndex).Filters.ToList().IndexOf(action.Filter);
        if (filterIndex < 0) return state;
        var command = state.Command.Clone()!;
        var condition = command.Conditions.ElementAt(conditionIndex);
        if (condition.Filters == null) return state;
        var filter = condition.Filters.ElementAt(filterIndex);
        if (!filter.CorrelationMappings.Remove(action.AttributeName)) return state;
        return state with { Command = command };
    }

    /// <summary>
    /// Handles the specified <see cref="CreateCorrelation"/>
    /// </summary>
    /// <param name="state">The state to reduce</param>
    /// <param name="action">The <see cref="CreateCorrelation"/> action to reduce</param>
    /// <returns>The reduced state</returns>
    public static CreateCorrelationState On(CreateCorrelationState state, CreateCorrelation action)
    {
        return state with { Saving = true };
    }

    /// <summary>
    /// Handles the specified <see cref="HandleCreateCorrelationResult"/>
    /// </summary>
    /// <param name="state">The state to reduce</param>
    /// <param name="action">The <see cref="HandleCreateCorrelationResult"/> action to reduce</param>
    /// <returns>The reduced state</returns>
    public static CreateCorrelationState On(CreateCorrelationState state, HandleCreateCorrelationResult action)
    {
        return state with { Saving = false };
    }

}
