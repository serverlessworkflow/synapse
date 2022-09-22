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
using ServerlessWorkflow.Sdk.Models;
using Synapse.Dashboard.Pages.Workflows.Editor.Actions;

namespace Synapse.Dashboard.Pages.Workflows.Editor.State
{

    /// <summary>
    /// The <see cref="State{TState}"/> of the workflow editor
    /// </summary>
    [Feature]
    public record WorkflowEditorState
    {

        /// <summary>
        /// The workflow definition
        /// </summary>
        public WorkflowDefinition? WorkflowDefinition { get; set; }

        /// <summary>
        /// The workflow definition text representation
        /// </summary>
        public string? WorkflowDefinitionText { get; set; }

        /// <summary>
        /// Gets a dictionary containing the name mappings of the editor's expanders states
        /// </summary>
        public Dictionary<string, bool> ExpanderStates { get; set; } = new()
        {
            { "general", true },
            { "states", true },
            { "events", false },
            { "functions", false },
            { "secrets", false },
            { "authentication", false },
            { "annotations", false },
            { "metadata", false }
        };

        /// <summary>
        /// Gets/sets a boolean indicating whether or not the state has been initialized
        /// </summary>
        /// <remarks>Used in conjunction with the <see cref="InitializeState.IfNotExists"/> property</remarks>
        public bool Initialized { get; set; }

        /// <summary>
        /// Defines if the workflow definition is being updated
        /// </summary>
        public bool Updating { get; set; }

        /// <summary>
        /// Defines if the workflow definition is being saved
        /// </summary>
        public bool Saving { get; set; }

    }

}
