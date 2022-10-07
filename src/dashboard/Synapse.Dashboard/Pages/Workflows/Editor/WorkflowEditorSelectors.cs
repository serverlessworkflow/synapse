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
using Synapse.Dashboard.Pages.Workflows.Editor.State;
using System.Reactive.Linq;

namespace Synapse.Dashboard.Pages.Workflows.Editor
{
    public static class WorkflowEditorSelectors
    {
        /// <summary>
        /// Selects the workflow definition
        /// </summary>
        /// <param name="store">The global <see cref="IStore"/></param>
        /// <returns></returns>
        public static IObservable<WorkflowDefinition?> SelectWorkflowDefinition(IStore store)
        {
            return store.GetFeature<WorkflowEditorState>()
                .Select(featureState => featureState.WorkflowDefinition)
                .DistinctUntilChanged();
        }

        /// <summary>
        /// Selects the workflow definition text
        /// </summary>
        /// <param name="store">The global <see cref="IStore"/></param>
        /// <returns></returns>
        public static IObservable<string?> SelectWorkflowDefinitionText(IStore store)
        {
            return store.GetFeature<WorkflowEditorState>()
                .Select(featureState => featureState.WorkflowDefinitionText)
                .DistinctUntilChanged();
        }

        /// <summary>
        /// Selects the workflow definition validation messages
        /// </summary>
        /// <param name="store">The global <see cref="IStore"/></param>
        /// <returns></returns>
        public static IObservable<ICollection<string>?> SelectValidationMessages(IStore store)
        {
            return store.GetFeature<WorkflowEditorState>()
                .Select(featureState => featureState.ValidationMessages)
                .DistinctUntilChanged();
        }

        /// <summary>
        /// Selects the workflow definition diagram visibility state
        /// </summary>
        /// <param name="store">The global <see cref="IStore"/></param>
        /// <returns></returns>
        public static IObservable<bool> SelectIsDiagramVisible(IStore store)
        {
            return store.GetFeature<WorkflowEditorState>()
                .Select(featureState => featureState.IsDiagramVisible)
                .DistinctUntilChanged();
        }
    }
}
