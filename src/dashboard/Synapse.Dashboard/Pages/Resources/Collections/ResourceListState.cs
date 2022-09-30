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

namespace Synapse.Dashboard.Pages.Resources.Collections
{

    /// <summary>
    /// Represents the Flux state used to manage <see cref="V1FunctionDefinitionCollection"/>s
    /// </summary>
    [Feature]
    public record ResourceListState
    {

        /// <summary>
        /// Initializes a new <see cref="ResourceListState"/>
        /// </summary>
        public ResourceListState()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="ResourceListState"/>
        /// </summary>
        /// <param name="functionDefinitionCollections">A <see cref="List{T}"/> containing the currently available <see cref="V1FunctionDefinitionCollection"/>s</param>
        public ResourceListState(List<V1FunctionDefinitionCollection> functionDefinitionCollections, List<V1EventDefinitionCollection> eventDefinitionCollections, List<V1AuthenticationDefinitionCollection> authenticationDefinitionCollections)
        {
            this.FunctionDefinitionCollections = functionDefinitionCollections;
            this.EventDefinitionCollections = eventDefinitionCollections;
            this.AuthenticationDefinitionCollections = authenticationDefinitionCollections;
        }

        /// <summary>
        /// Gets a <see cref="List{T}"/> containing the currently available <see cref="V1FunctionDefinitionCollection"/>s
        /// </summary>
        public List<V1FunctionDefinitionCollection> FunctionDefinitionCollections { get; set; } = null!;

        /// <summary>
        /// Gets a <see cref="List{T}"/> containing the currently available <see cref="V1EventDefinitionCollection"/>s
        /// </summary>
        public List<V1EventDefinitionCollection> EventDefinitionCollections { get; set; } = null!;

        /// <summary>
        /// Gets a <see cref="List{T}"/> containing the currently available <see cref="V1AuthenticationDefinitionCollection"/>s
        /// </summary>
        public List<V1AuthenticationDefinitionCollection> AuthenticationDefinitionCollections { get; set; } = null!;

    }

}
