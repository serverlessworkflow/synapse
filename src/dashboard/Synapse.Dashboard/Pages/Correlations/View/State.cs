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

using Neuroglia.Data.Flux;
using Synapse.Integration.Models;

namespace Synapse.Dashboard.Pages.Correlations.View;

/// <summary>
/// Represents the Flux state used by the correlation view page
/// </summary>
[Feature]
public record CorrelationViewState
{

    /// <summary>
    /// Gets a boolean indicating whether or not the modal used to publish <see cref="CloudEvent"/> is opened
    /// </summary>
    public bool PublishCloudEventModalOpened { get; init; }

    /// <summary>
    /// Gets the current <see cref="V1Correlation"/>, if any
    /// </summary>
    public V1Correlation? Correlation { get; init; }

    /// <summary>
    /// Gets the current <see cref="V1Event"/>, if any
    /// </summary>
    public V1Event? Event { get; init; }

}

