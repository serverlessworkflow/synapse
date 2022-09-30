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

using ServerlessWorkflow.Sdk.Models;
using Synapse.Integration.Models;

namespace Synapse.Dashboard.Pages.Resources.Collections.Events
{

    /// <summary>
    /// Represents the Flux action used to initialize the <see cref="EventDefinitionCollectionEditorState"/>
    /// </summary>
    public class InitializeState
    {

        /// <summary>
        /// Initializes a new <see cref="InitializeState"/>
        /// </summary>
        /// <param name="ifNotExists">A boolean indicating whether or not to initialize the state only if it does not yet exist</param>
        public InitializeState(bool ifNotExists = true)
        {
            this.IfNotExists = ifNotExists;
        }

        /// <summary>
        /// Initializes a new <see cref="InitializeState"/>
        /// </summary>
        /// <param name="collectionId">The id of the event definition collection to initialize the state with</param>
        public InitializeState(string collectionId)
        {
            this.CollectionId = collectionId;
        }

        /// <summary>
        /// Gets/sets the id of the event definition collection to initialize the state with
        /// </summary>
        public string? CollectionId { get; }

        /// <summary>
        /// Gets/sets a boolean indicating whether or not to initialize the state only if it does not yet exist
        /// </summary>
        public bool IfNotExists { get; }

    }

    /// <summary>
    /// Represents the Flux action used to set the initialized state
    /// </summary>
    public class InitializeStateSuccessful
    {

        /// <summary>
        /// Initializes a new <see cref="InitializeStateSuccessful"/>
        /// </summary>
        /// <param name="initialState">The initial state</param>
        /// <param name="ifNotExists">A boolean indicating whether or not to initialize the state only if it does not yet exist</param>
        public InitializeStateSuccessful(EventDefinitionCollectionEditorState initialState, bool ifNotExists = true)
        {
            this.InitialState = initialState ?? throw new ArgumentNullException(nameof(initialState));
            this.IfNotExists = ifNotExists;
        }

        /// <summary>
        /// Gets the initial state
        /// </summary>
        public EventDefinitionCollectionEditorState InitialState { get; }

        /// <summary>
        /// Gets a boolean indicating whether or not to initialize the state only if it does not yet exist
        /// </summary>
        public bool IfNotExists { get; }

    }

    /// <summary>
    /// Represents the Flux action to handle changes in the form based editor
    /// </summary>
    public class HandleFormBasedEditorChange
    {

        /// <summary>
        /// Initialised a new <see cref="HandleFormBasedEditorChange"/> action with the provided definition
        /// </summary>
        /// <param name="collection">The updated <see cref="V1EventDefinitionCollection"/></param>
        public HandleFormBasedEditorChange(V1EventDefinitionCollection collection)
        {
            this.Collection = collection ?? throw new ArgumentNullException(nameof(collection));
        }

        /// <summary>
        /// Gets the updated <see cref="V1EventDefinitionCollection"/>
        /// </summary>
        public V1EventDefinitionCollection Collection { get; }

    }

    /// <summary>
    /// Represents the Flux action to handle changes in the text based editor
    /// </summary>
    public class HandleTextBasedEditorChange
    {

        /// <summary>
        /// Initializes a new <see cref="HandleTextBasedEditorChange"/> action with the provided definition
        /// </summary>
        /// <param name="serializedCollection">The updated serialized collection</param>
        public HandleTextBasedEditorChange(string serializedCollection)
        {
            this.SerializedCollection = serializedCollection ?? throw new ArgumentNullException(nameof(serializedCollection));
        }

        /// <summary>
        /// Gets the updated serialized collection
        /// </summary>
        public string SerializedCollection { get; }

    }

    /// <summary>
    /// Represents the Flux action to update the edited event definition collection
    /// </summary>
    public class UpdateCollection
    {

        /// <summary>
        /// Initializes a new <see cref="UpdateCollection"/>
        /// </summary>
        /// <param name="collection">The updated <see cref="V1EventDefinitionCollection"/></param>
        public UpdateCollection(V1EventDefinitionCollection collection)
        {
            this.Collection = collection ?? throw new ArgumentNullException(nameof(collection));
        }

        /// <summary>
        /// Gets the updated <see cref="V1EventDefinitionCollection"/>
        /// </summary>
        public V1EventDefinitionCollection Collection { get; }
    }

    /// <summary>
    /// Represents the Flux action used to update the edited event definition collection
    /// </summary>
    public class UpdateSerializedCollection
    {
        /// <summary>
        /// Initializes a new <see cref="UpdateSerializedCollection"/>
        /// </summary>
        /// <param name="serializedCollection">The updated serialized collection</param>
        public UpdateSerializedCollection(string serializedCollection)
        {
            this.SerializedCollection = serializedCollection ?? throw new ArgumentNullException(nameof(serializedCollection));
        }

        /// <summary>
        /// Gets the updated serialized collection
        /// </summary>
        public string SerializedCollection { get; }

    }

    /// <summary>
    /// Represents the Flux action dispatched when the editor starts to update
    /// </summary>
    public class StartUpdating { }

    /// <summary>
    /// Represents the Flux action dispatched when the editor finished updating
    /// </summary>
    public class StopUpdating { }

    /// <summary>
    /// Represents the Flux action used to save the specified <see cref="V1EventDefinitionCollection"/> using the Synapse API
    /// </summary>
    public class SaveCollection
    {

        /// <summary>
        /// Initializes a new <see cref="SaveCollection"/>
        /// </summary>
        /// <param name="collection">The <see cref="V1EventDefinitionCollection"/> to save</param>
        public SaveCollection(V1EventDefinitionCollection collection)
        {
            this.Collection = collection;
        }

        /// <summary>
        /// Gets the <see cref="V1EventDefinitionCollection"/> to save
        /// </summary>
        public V1EventDefinitionCollection Collection { get; }

    }

    /// <summary>
    /// Represents the Flux action used to notify the UI about the completion of the specified <see cref="V1EventDefinitionCollection"/>'s save
    /// </summary>
    public class CollectionSaved
    {

        /// <summary>
        /// Initializes a new <see cref="CollectionSaved"/>
        /// </summary>
        /// <param name="collection">The <see cref="V1EventDefinitionCollection"/> that has been saved</param>
        public CollectionSaved(V1EventDefinitionCollection collection)
        {
            this.Collection = collection;
        }

        /// <summary>
        /// Gets the <see cref="V1EventDefinitionCollection"/> that has been saved
        /// </summary>
        public V1EventDefinitionCollection Collection { get; }

    }

    /// <summary>
    /// Notifies the UI about the failure of a <see cref="V1EventDefinitionCollection"/>'s save
    /// </summary>
    public class CollectionSaveFailed
    {

        /// <summary>
        /// Initializes a new <see cref="CollectionSaveFailed"/>
        /// </summary>
        /// <param name="error">The error that has occured while saving the specified <see cref="V1EventDefinitionCollection"/></param>
        public CollectionSaveFailed(string error)
        {
            this.Error = error;
        }

        /// <summary>
        /// Gets the error that has occured while saving the specified <see cref="V1EventDefinitionCollection"/>
        /// </summary>
        public string? Error { get; }

    }

    /// <summary>
    /// Represents the Flux action used to change the language of the text editor
    /// </summary>
    public class ChangeEditorLanguage
    {

        /// <summary>
        /// Initializes a new <see cref="ChangeEditorLanguage"/>
        /// </summary>
        /// <param name="language">The language to use</param>
        /// <param name="serializedCollection">The serialized collection</param>
        public ChangeEditorLanguage(string language, string serializedCollection)
        {
            this.Language = language ?? throw new ArgumentNullException(nameof(language));
            this.SerializedCollection = serializedCollection ?? throw new ArgumentNullException(nameof(serializedCollection));
        }

        /// <summary>
        /// Gets the language to use
        /// </summary>
        public string Language { get; }

        /// <summary>
        /// Gets the serialized collection
        /// </summary>
        public string SerializedCollection { get; }
    }

    /// <summary>
    /// Represents the Flux action used to toggle the state of the specified expander
    /// </summary>
    public class ToggleExpand
    {

        /// <summary>
        /// Initializes a new <see cref="ToggleExpand"/>
        /// </summary>
        /// <param name="name">The name of the state to toggle</param>
        /// <param name="isExpanded">A boolean indicating whether or not the state is expanded</param>
        public ToggleExpand(string name, bool isExpanded)
        {
            this.Name = name;
            this.IsExpanded = isExpanded;
        }

        /// <summary>
        /// Gets the name of the state to toggle
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a boolean indicating whether or not the state is expanded
        /// </summary>
        public bool IsExpanded { get; }

    }

}
