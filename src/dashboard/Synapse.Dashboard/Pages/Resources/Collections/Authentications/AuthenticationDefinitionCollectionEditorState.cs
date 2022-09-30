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

namespace Synapse.Dashboard.Pages.Resources.Collections.Authentications
{

    /// <summary>
    /// Represents the object used to maintain the state of a function definition collection editor
    /// </summary>
    [Feature]
    public record AuthenticationDefinitionCollectionEditorState
    {

        /// <summary>
        /// Initializes a new <see cref="AuthenticationDefinitionCollectionEditorState"/>
        /// </summary>
        public AuthenticationDefinitionCollectionEditorState()
        {

        }

        /// <summary>
        /// Gets/sets the collection to edit
        /// </summary>
        public Integration.Models.V1AuthenticationDefinitionCollection? Collection { get; set; }

        /// <summary>
        /// Gets/sets the raw JSON/YAML of the collection to edit 
        /// </summary>
        public string? SerializedCollection { get; set; }

        /// <summary>
        /// Gets/sets a boolean indicating whether or not the editor has been initialized
        /// </summary>
        public bool Initialized { get; set; }

        /// <summary>
        /// Gets/sets a boolean indicating whether or not the editor is saving the collection
        /// </summary>
        public bool Saving { get; set; }

        /// <summary>
        /// Gets/sets a boolean indicating whether or not the editor is being updated
        /// </summary>
        public bool Updating { get; set; }

        /// <summary>
        /// Gets a dictionary containing the name mappings of the editor's expanders states
        /// </summary>
        public Dictionary<string, bool>? ExpanderStates { get; set; }

    }

}
