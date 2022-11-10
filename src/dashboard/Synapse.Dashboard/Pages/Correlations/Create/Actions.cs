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

using Synapse.Integration.Commands.Correlations;
using Synapse.Integration.Models;

namespace Synapse.Dashboard.Pages.Correlations.Create;

/// <summary>
/// Represents the Flux action used to initialize the <see cref="CreateCorrelationState"/>
/// </summary>
public class InitializeState
{

    /// <summary>
    /// Initializes a new <see cref="InitializeState"/>
    /// </summary>
    /// <param name="reset">A boolean indicating whether or not to reset the state if it has already been initialized</param>
    public InitializeState(bool reset = false)
    {
        this.Reset = reset;
    }

    /// <summary>
    /// Gets a boolean indicating whether or not to reset the state if it has already been initialized
    /// </summary>
    public bool Reset { get; }

}

/// <summary>
/// Represents the Flux action used to create a new <see cref="V1Correlation"/>
/// </summary>
public class CreateCorrelation
{

    /// <summary>
    /// Initializes a new <see cref="CreateCorrelation"/>
    /// </summary>
    /// <param name="command">An object that describes the command to execute</param>
    public CreateCorrelation(V1CreateCorrelationCommand command)
    {
        this.Command = command;
    }

    /// <summary>
    /// Gets an object that describes the command to execute
    /// </summary>
    public V1CreateCorrelationCommand Command { get; }

}

/// <summary>
/// Represents a Flux action used to handle the differed result of a <see cref="CreateCorrelation"/> action
/// </summary>
public class HandleCreateCorrelationResult
{

    /// <summary>
    /// Initializes a new <see cref="HandleCreateCorrelationResult"/>
    /// </summary>
    /// <param name="result">The newly created <see cref="V1Correlation"/>, if any</param>
    public HandleCreateCorrelationResult(V1Correlation result)
    {
        this.Result = result;
    }

    /// <summary>
    /// Gets the newly created <see cref="V1Correlation"/>, if any
    /// </summary>
    public V1Correlation Result { get; }

}

/// <summary>
/// Represents the Flux action used to patch the <see cref="V1Correlation"/> to create
/// </summary>
public class PatchCorrelation
{

    /// <summary>
    /// Initializes a new <see cref="PatchCorrelation"/>
    /// </summary>
    /// <param name="patch">The <see cref="Action{T}"/> used to patch the <see cref="V1CreateCorrelationCommand"/></param>
    public PatchCorrelation(Action<V1CreateCorrelationCommand> patch)
    {
        this.Patch = patch;
    }

    /// <summary>
    /// Gets the <see cref="Action{T}"/> used to patch the <see cref="V1CreateCorrelationCommand"/>
    /// </summary>
    public Action<V1CreateCorrelationCommand> Patch { get; }

}

/// <summary>
/// Represents the Flux action to add a new <see cref="V1CorrelationCondition"/> to the <see cref="V1Correlation"/> to create
/// </summary>
public class AddConditionToCorrelation
{



}

/// <summary>
/// Represents the Flux action to remove a <see cref="V1CorrelationCondition"/> from the <see cref="V1Correlation"/> to create
/// </summary>
public class RemoveConditionFromCorrelation
{

    /// <summary>
    /// Initializes a new <see cref="RemoveConditionFromCorrelation"/>
    /// </summary>
    /// <param name="condition">The <see cref="V1CorrelationCondition"/> to remove</param>
    public RemoveConditionFromCorrelation(V1CorrelationCondition condition)
    {
        this.Condition = condition;
    }

    /// <summary>
    /// Gets the <see cref="V1CorrelationCondition"/> to remove
    /// </summary>
    public V1CorrelationCondition Condition { get; }

}

/// <summary>
/// Represents the Flux action to add a new <see cref="V1EventFilter"/> to a <see cref="V1CorrelationCondition"/> of the <see cref="V1Correlation"/> to create
/// </summary>
public class AddFilterToCorrelationCondition
{

    /// <summary>
    /// Initializes a new <see cref="AddFilterToCorrelationCondition"/>
    /// </summary>
    /// <param name="condition">The <see cref="V1CorrelationCondition"/> to add a new <see cref="V1EventFilter"/> to</param>
    public AddFilterToCorrelationCondition(V1CorrelationCondition condition)
    {
        this.Condition = condition;
    }

    /// <summary>
    /// Gets the <see cref="V1CorrelationCondition"/> to add a new <see cref="V1EventFilter"/> to
    /// </summary>
    public V1CorrelationCondition Condition { get; }

}

/// <summary>
/// Represents the Flux action to remove a <see cref="V1EventFilter"/> from a <see cref="V1CorrelationCondition"/> of the <see cref="V1Correlation"/> to create
/// </summary>
public class RemoveFilterFromCorrelationCondition
{

    /// <summary>
    /// Initializes a new <see cref="RemoveFilterFromCorrelationCondition"/>
    /// </summary>
    /// <param name="condition">The <see cref="V1CorrelationCondition"/> to remove the specified <see cref="V1EventFilter"/> from</param>
    /// <param name="filter">The <see cref="V1EventFilter"/> to remove from the specified <see cref="V1CorrelationCondition"/></param>
    public RemoveFilterFromCorrelationCondition(V1CorrelationCondition condition, V1EventFilter filter)
    {
        this.Condition = condition;
        this.Filter = filter;
    }

    /// <summary>
    /// Gets the <see cref="V1CorrelationCondition"/> to remove the specified <see cref="V1EventFilter"/> from
    /// </summary>
    public V1CorrelationCondition Condition { get; }

    /// <summary>
    /// Gets the <see cref="V1EventFilter"/> to remove from the specified <see cref="V1CorrelationCondition"/>
    /// </summary>
    public V1EventFilter Filter { get; }

}

/// <summary>
/// Represents the Flux action used to add or update the specified <see cref="V1EventFilter"/>'s attribute
/// </summary>
public class AddOrUpdateConditionFilterAttribute
{

    /// <summary>
    /// Initializes a new <see cref="AddOrUpdateConditionFilterAttribute"/>
    /// </summary>
    /// <param name="condition">The <see cref="V1CorrelationCondition"/> the <see cref="V1EventFilter"/> to patch belongs to</param>
    /// <param name="filter">The <see cref="V1EventFilter"/> to patch</param>
    /// <param name="attributeName">The name of the attribute to add or update</param>
    /// <param name="attributeValue">The value of the attribute to add or update</param>
    public AddOrUpdateConditionFilterAttribute(V1CorrelationCondition condition, V1EventFilter filter, string attributeName, string attributeValue)
    {
        this.Condition = condition;
        this.Filter = filter;
        this.AttributeName = attributeName;
        this.AttributeValue = attributeValue;
    }

    /// <summary>
    /// Gets the <see cref="V1CorrelationCondition"/> the <see cref="V1EventFilter"/> to patch belongs to
    /// </summary>
    public V1CorrelationCondition Condition { get; }

    /// <summary>
    /// Gets the <see cref="V1EventFilter"/> to patch
    /// </summary>
    public V1EventFilter Filter { get; }

    /// <summary>
    /// Gets the name of the attribute to add or update
    /// </summary>
    public string AttributeName { get; }

    /// <summary>
    /// Gets the value of the attribute to add or update
    /// </summary>
    public string AttributeValue { get; }

}

/// <summary>
/// Represents the Flux action used to remove an attribute from a <see cref="V1EventFilter"/>
/// </summary>
public class RemoveAttributeFromConditionFilter
{

    /// <summary>
    /// Initializes a new <see cref="RemoveAttributeFromConditionFilter"/>
    /// </summary>
    /// <param name="condition">The <see cref="V1CorrelationCondition"/> the <see cref="V1EventFilter"/> to patch belongs to</param>
    /// <param name="filter">The <see cref="V1EventFilter"/> to patch</param>
    /// <param name="attributeName">The name of the attribute to remove</param>
    public RemoveAttributeFromConditionFilter(V1CorrelationCondition condition, V1EventFilter filter, string attributeName)
    {
        this.Condition = condition;
        this.Filter = filter;
        this.AttributeName = attributeName;
    }

    /// <summary>
    /// Gets the <see cref="V1CorrelationCondition"/> the <see cref="V1EventFilter"/> to patch belongs to
    /// </summary>
    public V1CorrelationCondition Condition { get; }

    /// <summary>
    /// Gets the <see cref="V1EventFilter"/> to patch
    /// </summary>
    public V1EventFilter Filter { get; }

    /// <summary>
    /// Gets the name of the attribute to remove
    /// </summary>
    public string AttributeName { get; }

}

/// <summary>
/// Represents the Flux action used to add or update the specified <see cref="V1EventFilter"/>'s correlation mapping
/// </summary>
public class AddOrUpdateConditionFilterCorrelationMapping
{

    /// <summary>
    /// Initializes a new <see cref="AddOrUpdateConditionFilterCorrelationMapping"/>
    /// </summary>
    /// <param name="condition">The <see cref="V1CorrelationCondition"/> the <see cref="V1EventFilter"/> to patch belongs to</param>
    /// <param name="filter">The <see cref="V1EventFilter"/> to patch</param>
    /// <param name="attributeName">The name of the attribute to add or update</param>
    /// <param name="attributeValue">The value of the correlation mapping to add or update</param>
    public AddOrUpdateConditionFilterCorrelationMapping(V1CorrelationCondition condition, V1EventFilter filter, string attributeName, string attributeValue)
    {
        this.Condition = condition;
        this.Filter = filter;
        this.AttributeName = attributeName;
        this.AttributeValue = attributeValue;
    }

    /// <summary>
    /// Gets the <see cref="V1CorrelationCondition"/> the <see cref="V1EventFilter"/> to patch belongs to
    /// </summary>
    public V1CorrelationCondition Condition { get; }

    /// <summary>
    /// Gets the <see cref="V1EventFilter"/> to patch
    /// </summary>
    public V1EventFilter Filter { get; }

    /// <summary>
    /// Gets the name of the attribute to add or update
    /// </summary>
    public string AttributeName { get; }

    /// <summary>
    /// Gets the value of the attribute to add or update
    /// </summary>
    public string AttributeValue { get; }

}

/// <summary>
/// Represents the Flux action used to remove a correlation mapping from a <see cref="V1EventFilter"/>
/// </summary>
public class RemoveCorrelationMappingFromConditionFilter
{

    /// <summary>
    /// Initializes a new <see cref="RemoveCorrelationMappingFromConditionFilter"/>
    /// </summary>
    /// <param name="condition">The <see cref="V1CorrelationCondition"/> the <see cref="V1EventFilter"/> to patch belongs to</param>
    /// <param name="filter">The <see cref="V1EventFilter"/> to patch</param>
    /// <param name="attributeName">The name of the attribute to remove</param>
    public RemoveCorrelationMappingFromConditionFilter(V1CorrelationCondition condition, V1EventFilter filter, string attributeName)
    {
        this.Condition = condition;
        this.Filter = filter;
        this.AttributeName = attributeName;
    }

    /// <summary>
    /// Gets the <see cref="V1CorrelationCondition"/> the <see cref="V1EventFilter"/> to patch belongs to
    /// </summary>
    public V1CorrelationCondition Condition { get; }

    /// <summary>
    /// Gets the <see cref="V1EventFilter"/> to patch
    /// </summary>
    public V1EventFilter Filter { get; }

    /// <summary>
    /// Gets the name of the attribute to remove
    /// </summary>
    public string AttributeName { get; }

}