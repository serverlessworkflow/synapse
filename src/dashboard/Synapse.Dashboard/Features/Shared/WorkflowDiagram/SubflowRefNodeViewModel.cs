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

namespace Synapse.Dashboard
{
    /// <summary>
    /// Represents a <see cref="SubflowReference"/> <see cref="NodeViewModel"/>
    /// </summary>
    public class SubflowRefNodeViewModel
        : ActionNodeViewModel
    {

        /// <summary>
        /// Initializes a new <see cref="SubflowRefNodeViewModel"/>
        /// </summary>
        /// <param name="subflow">The <see cref="SubflowReference"/> the <see cref="SubflowRefNodeViewModel"/> represents</param>
        public SubflowRefNodeViewModel(ActionDefinition action, SubflowReference subflow)
            : base(action, $"{subflow.WorkflowId}:{subflow.Version ?? "latest"}", "subflow-node")
        {
            this.Subflow = subflow;
        }

        /// <summary>
        /// Gets the <see cref="SubflowReference"/> the <see cref="SubflowRefNodeViewModel"/> represents
        /// </summary>
        public SubflowReference Subflow { get; }

    }

}
