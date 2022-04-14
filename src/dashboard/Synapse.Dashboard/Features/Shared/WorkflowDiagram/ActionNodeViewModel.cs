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

using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Dashboard
{

    /// <summary>
    /// Represents a <see cref="ActionDefinition"/> <see cref="NodeViewModel"/>
    /// </summary>
    public class ActionNodeViewModel
        : LabeledNodeViewModel
    {

        /// <summary>
        /// Initializes a new <see cref="ActionNodeViewModel"/>
        /// </summary>
        /// <param name="action">The <see cref="ActionDefinition"/> the <see cref="ActionNodeViewModel"/> represents</param>
        public ActionNodeViewModel(ActionDefinition action, string? cssClass = "action-node")
            : base(action.Name ?? action.Function?.RefName ?? action.Subflow?.WorkflowId ?? action.RetryRef, cssClass)
        {
            this.Action = action;
        }

        /// <summary>
        /// Gets the <see cref="ActionDefinition"/> the <see cref="ActionNodeViewModel"/> represents
        /// </summary>
        public ActionDefinition Action { get; }

    }

}
