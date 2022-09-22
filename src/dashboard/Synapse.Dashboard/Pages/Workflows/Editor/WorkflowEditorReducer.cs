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
using Synapse.Dashboard.Pages.Workflows.Editor.Actions;
using Synapse.Dashboard.Pages.Workflows.Editor.State;

namespace Synapse.Dashboard.Pages.Workflows.Editor
{

    [Reducer]
    public static class WorkflowEditorReducer
    {

        /// <summary>
        /// Initialize the state
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The action to reduce</param>
        /// <returns>The reduced state</returns>
        public static WorkflowEditorState On(WorkflowEditorState state, InitializeStateSuccessful action)
        {
            if (action.IfNotExists && state.Initialized)
                return state;
            return action.InitialState with { Initialized = true };
        }

        /// <summary>
        /// Changes the updating state when editing the definition form the text editor
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The action to reduce</param>
        /// <returns>The reduced state</returns>
        public static WorkflowEditorState On(WorkflowEditorState state, StartUpdating action)
        {
            return state with
            {
                Updating = true
            };
        }

        /// <summary>
        /// Changes the updating state when failed to handle the value from the text editor
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The action to reduce</param>
        /// <returns>The reduced state</returns>
        public static WorkflowEditorState On(WorkflowEditorState state, StopUpdating action)
        {
            return state with
            {
                Updating = false
            };
        }

        /// <summary>
        /// Changes the definition state
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The action to reduce</param>
        /// <returns>The reduced state</returns>
        public static WorkflowEditorState On(WorkflowEditorState state, UpdateDefinition action)
        {
            return state with
            {
                WorkflowDefinition = action.WorkflowDefinition
            };
        }

        /// <summary>
        /// Notifies about the definition being saved
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The action to reduce</param>
        /// <returns>The reduced state</returns>
        public static WorkflowEditorState On(WorkflowEditorState state, SaveWorkflowDefinition action)
        {
            return state with
            {
                Saving = true
            };
        }

        /// <summary>
        /// Notifies about the completion of the definition save
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The action to reduce</param>
        /// <returns>The reduced state</returns>
        public static WorkflowEditorState On(WorkflowEditorState state, WorkflowDefinitionSaved action)
        {
            return state with
            {
                WorkflowDefinition = action.WorkflowDefinition,
                Saving = false
            };
        }

        /// <summary>
        /// Notifies about the failure of the definition save
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The action to reduce</param>
        /// <returns>The reduced state</returns>
        public static WorkflowEditorState On(WorkflowEditorState state, WorkflowDefinitionSaveFailed action)
        {
            return state with
            {
                Saving = false
            };
        }

        /// <summary>
        /// Changes the JSON definition state
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The action to reduce</param>
        /// <returns>The reduced state</returns>
        public static WorkflowEditorState On(WorkflowEditorState state, UpdateDefinitionText action)
        {
            return state with
            {
                WorkflowDefinitionText = action.WorkflowDefinitionText
            };
        }

        /// <summary>
        /// Toggles the state of the specified expander
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The action to reduce</param>
        /// <returns>The reduced state</returns>
        public static WorkflowEditorState On(WorkflowEditorState state, ToggleExpand action)
        {
            state.ExpanderStates[action.Name] = action.IsExpanded;
            return state;
        }

    }

}
