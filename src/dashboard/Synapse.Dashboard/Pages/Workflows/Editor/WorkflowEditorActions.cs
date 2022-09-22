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
using Synapse.Dashboard.Pages.Workflows.Editor.State;

/// <summary>
/// Stores the actions triggered in the <see cref="Editor"/>
/// </summary>
namespace Synapse.Dashboard.Pages.Workflows.Editor.Actions
{

    /// <summary>
    /// Triggers state initialization
    /// </summary>
    public class InitializeState 
    {
        public InitializeState(bool ifNotExists = true)
        {
            IfNotExists = ifNotExists;
        }

        /// <summary>
        /// Gets/sets a boolean indicating whether or not to initialize the state only if it does not yet exist
        /// </summary>
        public bool IfNotExists { get; }

    }

    /// <summary>
    /// Returns the initial state
    /// </summary>
    public class InitializeStateSuccessful 
    {
        public InitializeStateSuccessful(WorkflowEditorState initialState, bool ifNotExists = true)
        {
            this.InitialState = initialState ?? throw new ArgumentNullException(nameof(initialState));
            this.IfNotExists = ifNotExists;
        }

        /// <summary>
        /// The initial state
        /// </summary>
        public WorkflowEditorState InitialState { get; }

        /// <summary>
        /// Gets/sets a boolean indicating whether or not to initialize the state only if it does not yet exist
        /// </summary>
        public bool IfNotExists { get; }
    }

    /// <summary>
    /// The action to update the workflow definition
    /// </summary>
    public class UpdateDefinition
    {
        /// <summary>
        /// Initialised a new <see cref="UpdateDefinition"/> action with the provided definition
        /// </summary>
        /// <param name="definition"></param>
        public UpdateDefinition(WorkflowDefinition workflowDefinition)
        {
            this.WorkflowDefinition = workflowDefinition ?? throw new ArgumentNullException(nameof(workflowDefinition));
        }

        /// <summary>
        /// The updated workflow definition
        /// </summary>
        public WorkflowDefinition WorkflowDefinition { get; }
    }

    /// <summary>
    /// The action to update the workflow definition text
    /// </summary>
    public class UpdateDefinitionText
    {
        /// <summary>
        /// Initialised a new <see cref="UpdateDefinitionText"/> action with the provided definition text
        /// </summary>
        /// <param name="workflowDefinitionText"></param>
        public UpdateDefinitionText(string workflowDefinitionText)
        {
            this.WorkflowDefinitionText = workflowDefinitionText ?? throw new ArgumentNullException(nameof(workflowDefinitionText));
        }

        /// <summary>
        /// The text of the updated workflow definition
        /// </summary>
        public string WorkflowDefinitionText { get; }
    }

    /// <summary>
    /// The action dispatched when the editor starts to update
    /// </summary>
    public class StartUpdating { }

    /// <summary>
    /// The action dispatched when the editor finished updating
    /// </summary>
    public class StopUpdating { }

    /// <summary>
    /// Saves the specified <see cref="WorkflowDefinition"/> using the Synapse API
    /// </summary>
    public class SaveWorkflowDefinition
    {

        public SaveWorkflowDefinition(WorkflowDefinition workflowDefinition)
        {
            this.WorkflowDefinition = workflowDefinition;
        }

        public WorkflowDefinition WorkflowDefinition { get; }

    }

    /// <summary>
    /// Notifies the UI about the completion of the specified <see cref="WorkflowDefinition"/>'s save
    /// </summary>
    public class WorkflowDefinitionSaved
    {

        public WorkflowDefinitionSaved(WorkflowDefinition workflowDefinition)
        {
            this.WorkflowDefinition = workflowDefinition;
        }

        public WorkflowDefinition WorkflowDefinition { get; }

    }

    /// <summary>
    /// Notifies the UI about the failure of a <see cref="WorkflowDefinition"/>'s save
    /// </summary>
    public class WorkflowDefinitionSaveFailed
    {

        public WorkflowDefinitionSaveFailed(string error)
        {
            this.Error = error;
        }

        public string? Error { get; }

    }

    /// <summary>
    /// The action to handle changes in the form based editor
    /// </summary>
    public class HandleFormBasedEditorChange
    {
        /// <summary>
        /// Initialised a new <see cref="HandleFormBasedEditorChange"/> action with the provided definition
        /// </summary>
        /// <param name="definition"></param>
        public HandleFormBasedEditorChange(WorkflowDefinition workflowDefinition)
        {
            this.WorkflowDefinition = workflowDefinition ?? throw new ArgumentNullException(nameof(workflowDefinition));
        }

        /// <summary>
        /// The updated workflow definition
        /// </summary>
        public WorkflowDefinition WorkflowDefinition { get; }
    }

    /// <summary>
    /// The action to handle changes in the text based editor
    /// </summary>
    public class HandleTextBasedEditorChange
    {
        /// <summary>
        /// Initialised a new <see cref="HandleTextBasedEditorChange"/> action with the provided definition
        /// </summary>
        /// <param name="definition"></param>
        public HandleTextBasedEditorChange(string workflowDefinitionText)
        {
            this.WorkflowDefinitionText = workflowDefinitionText ?? throw new ArgumentNullException(nameof(workflowDefinitionText));
        }

        /// <summary>
        /// The updated workflow definition text
        /// </summary>
        public string WorkflowDefinitionText { get; }
    }

    /// <summary>
    /// The action dispatched when the text editor language changes
    /// </summary>
    public class ChangeTextLanguage
    {
        /// <summary>
        /// Initialised a new <see cref="ChangeTextLanguage"/> action with the provided definition
        /// </summary>
        /// <param name="definition"></param>
        public ChangeTextLanguage(string language, string workflowDefinitionText)
        {
            this.Language = language ?? throw new ArgumentNullException(nameof(language));
            this.WorkflowDefinitionText = workflowDefinitionText ?? throw new ArgumentNullException(nameof(workflowDefinitionText));
        }

        /// <summary>
        /// The new language of the text editor
        /// </summary>
        public string Language { get; }

        /// <summary>
        /// The updated workflow definition text
        /// </summary>
        public string WorkflowDefinitionText { get; }
    }

    /// <summary>
    /// Toggles the state of the specified expander
    /// </summary>
    public class ToggleExpand
    {

        public ToggleExpand(string name, bool isExpanded)
        {
            this.Name = name;
            this.IsExpanded = isExpanded;
        }

        public string Name { get; }

        public bool IsExpanded { get; }

    }

    /// <summary>
    /// Toggles the workflow diagram visibility
    /// </summary>
    public class ToggleDiagramVisibility { }

    /// <summary>
    /// The action dispatched to valide the workflow definition
    /// </summary>
    public class ValidateWorkflowDefinition 
    {
        public ValidateWorkflowDefinition(WorkflowDefinition workflowDefinition, bool saveAfterValidation)
        {
            this.WorkflowDefinition = workflowDefinition;
            this.SaveAfterValidation = saveAfterValidation;
        }

        public WorkflowDefinition WorkflowDefinition { get; }
        public bool SaveAfterValidation { get; }
    }

    /// <summary>
    /// The action dispatched to set the validation messages
    /// </summary>
    public class SetValidationMessages 
    {
        public SetValidationMessages(List<string> validationMessages)
        {
            ValidationMessages = validationMessages;
        }

        public List<string> ValidationMessages { get; }

    }

    /// <summary>
    /// The action dispatched a workflow definition has been validated without messages/errors
    /// </summary>
    public class WorkflowDefinitionValidated
    {
        public WorkflowDefinitionValidated(WorkflowDefinition workflowDefinition, bool saveAfterValidation)
        {
            this.WorkflowDefinition = workflowDefinition;
            this.SaveAfterValidation = saveAfterValidation;
        }

        public WorkflowDefinition WorkflowDefinition { get; }
        public bool SaveAfterValidation { get; }
    }

    /// <summary>
    /// The action dispatched to clear the validation messages
    /// </summary>
    public class ClearValidationMessages { }

}
