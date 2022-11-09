/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0(the "License");
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
 */

using Newtonsoft.Json.Schema;
using Synapse.Integration.Models;

namespace Synapse.Dashboard.Pages.Correlations.View;

/// <summary>
/// Represents the Flux action used to retrieve a <see cref="V1Correlation"/> by id
/// </summary>
public class GetCorrelationById
{

    /// <summary>
    /// Initializes a new <see cref="GetCorrelationById"/>
    /// </summary>
    /// <param name="id">The id of the <see cref="V1Correlation"/> to get</param>
    public GetCorrelationById(string id)
    {
        this.Id = id;
    }

    /// <summary>
    /// Gets the id of the <see cref="V1Correlation"/> to get
    /// </summary>
    public string Id { get; }

}

/// <summary>
/// Represents the Flux action used to handle the differed result of a <see cref="GetCorrelationById"/> action
/// </summary>
public class HandleGetCorrelationByIdResult
{

    /// <summary>
    /// Initializes a new <see cref="HandleGetCorrelationByIdResult"/>
    /// </summary>
    /// <param name="result">The differed result of a <see cref="GetCorrelationById"/> action</param>
    public HandleGetCorrelationByIdResult(V1Correlation? result)
    {
        this.Result = result;
    }

    /// <summary>
    /// Gets the differed result of a <see cref="GetCorrelationById"/> action
    /// </summary>
    public V1Correlation? Result { get; }

}

/// <summary>
/// Represents the Flux action used to display the modal used to publish a new <see cref="CloudEvent"/>
/// </summary>
public class ShowPublishCloudEventModal
{

    /// <summary>
    /// Initializes a new <see cref="ShowPublishCloudEventModal"/>
    /// </summary>
    /// <param name="source">The <see cref="V1Event"/> to produce</param>
    public ShowPublishCloudEventModal(V1Event e)
    {
        this.Event = e;
    }

    /// <summary>
    /// Initializes a new <see cref="ShowPublishCloudEventModal"/>
    /// </summary>
    /// <param name="condition">The <see cref="V1CorrelationCondition"/> to publish a new <see cref="CloudEvent"/> for</param>
    public ShowPublishCloudEventModal(V1CorrelationCondition condition)
    {
        var e = V1Event.Create();
        var filter = condition.Filters.FirstOrDefault();
        if(filter != null)
        {
            foreach (var attribute in filter.Attributes)
            {
                e.SetAttribute(attribute.Key, attribute.Value);
            }
            foreach (var attribute in filter.CorrelationMappings)
            {
                e.SetAttribute(attribute.Key, attribute.Value);
            }
        }
        this.Event = e;
    }

    /// <summary>
    /// Gets the <see cref="V1Event"/> to produce
    /// </summary>
    public V1Event Event { get; }

}

/// <summary>
/// Represents the Flux action used to hide the publish cloud event modal
/// </summary>
public class HidePublishCloudEventModal
{

    /// <summary>
    /// Initializes a new <see cref="HidePublishCloudEventModal"/>
    /// </summary>
    /// <param name="reset">A boolean indicating whether or not to reset the modal's fields</param>
    public HidePublishCloudEventModal(bool reset = false)
    {
        this.Reset = reset;
    }

    /// <summary>
    /// Gets a boolean indicating whether or not to reset the modal's fields
    /// </summary>
    public bool Reset { get; }

}

/// <summary>
/// Represents the Flux action used to delete a <see cref="V1Correlation"/>'s <see cref="V1CorrelationContext"/>
/// </summary>
public class DeleteCorrelationContext
{

    /// <summary>
    /// Initializes a new <see cref="DeleteCorrelationContext"/>
    /// </summary>
    /// <param name="correlationId">The id of the <see cref="V1Correlation"/> that owns the <see cref="V1CorrelationContext"/> to delete</param>
    /// <param name="contextId">The id of the <see cref="V1CorrelationContext"/> to delete</param>
    public DeleteCorrelationContext(string correlationId, string contextId)
    {
        this.CorrelationId = correlationId;
        this.ContextId = contextId;
    }

    /// <summary>
    /// Gets the id of the <see cref="V1Correlation"/> that owns the <see cref="V1CorrelationContext"/> to delete
    /// </summary>
    public string CorrelationId { get; }

    /// <summary>
    /// Gets the id of the <see cref="V1CorrelationContext"/> to delete
    /// </summary>
    public string ContextId { get; }

}

/// <summary>
/// Represents the Flux action used to handle the differed result of a <see cref="DeleteCorrelationContext"/> action
/// </summary>
public class HandleDeleteCorrelationContextResult
{

}