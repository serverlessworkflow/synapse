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

namespace Synapse.Dashboard.Pages.Resources.Collections.Events
{

    /// <summary>
    /// Defines Flux reducers that apply to <see cref="EventDefinitionCollectionEditorState"/>-related Flux actions
    /// </summary>
    [Reducer]
    public static class EventDefinitionCollectionEditorReducers
    {

        /// <summary>
        /// Reduces the <see cref="EventDefinitionCollectionEditorState"/> for the specified <see cref="InitializeStateSuccessful"/>
        /// </summary>
        /// <param name="state">The <see cref="EventDefinitionCollectionEditorState"/> to reduce</param>
        /// <param name="action">The Flux action to reduce</param>
        /// <returns>The reduced <see cref="EventDefinitionCollectionEditorState"/></returns>
        public static EventDefinitionCollectionEditorState On(EventDefinitionCollectionEditorState state, InitializeStateSuccessful action)
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
        public static EventDefinitionCollectionEditorState On(EventDefinitionCollectionEditorState state, StartUpdating action)
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
        public static EventDefinitionCollectionEditorState On(EventDefinitionCollectionEditorState state, StopUpdating action)
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
        public static EventDefinitionCollectionEditorState On(EventDefinitionCollectionEditorState state, UpdateCollection action)
        {
            return state with
            {
                Collection = action.Collection
            };
        }

        /// <summary>
        /// Notifies about the definition being saved
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The action to reduce</param>
        /// <returns>The reduced state</returns>
        public static EventDefinitionCollectionEditorState On(EventDefinitionCollectionEditorState state, SaveCollection action)
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
        public static EventDefinitionCollectionEditorState On(EventDefinitionCollectionEditorState state, CollectionSaved action)
        {
            return state with
            {
                Collection = action.Collection,
                Saving = false
            };
        }

        /// <summary>
        /// Notifies about the failure of the definition save
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The action to reduce</param>
        /// <returns>The reduced state</returns>
        public static EventDefinitionCollectionEditorState On(EventDefinitionCollectionEditorState state, CollectionSaveFailed action)
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
        public static EventDefinitionCollectionEditorState On(EventDefinitionCollectionEditorState state, UpdateSerializedCollection action)
        {
            return state with
            {
                SerializedCollection = action.SerializedCollection
            };
        }

        /// <summary>
        /// Toggles the state of the specified expander
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The action to reduce</param>
        /// <returns>The reduced state</returns>
        public static EventDefinitionCollectionEditorState On(EventDefinitionCollectionEditorState state, ToggleExpand action)
        {
            state.ExpanderStates![action.Name] = action.IsExpanded;
            return state;
        }

    }

}
