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
using Synapse.Integration.Commands.Correlations;
using Synapse.Integration.Models;

namespace Synapse.Dashboard.Pages.Correlations.Create;

/// <summary>
/// Represents the Flux state that holds the information required by the '/correlations/new' view
/// </summary>
[Feature]
public record CreateCorrelationState
{

    /// <summary>
    /// Gets a boolean indicating whether or not the <see cref="CreateCorrelationState"/> has been initialized
    /// </summary>
    public bool Initialized { get; init; }

    /// <summary>
    /// Gets a boolean indicating whether or not the <see cref="V1Correlation"/> to create is being saved
    /// </summary>
    public bool Saving { get; init; }

    /// <summary>
    /// Gets an object that describes the command used to create a new <see cref="V1Correlation"/>
    /// </summary>
    public V1CreateCorrelationCommand? Command { get; init; }

}
