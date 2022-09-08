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
    /// Represents a <see cref="FunctionReference"/> <see cref="ActionNodeModel"/>
    /// </summary>
    public class FunctionRefNodeViewModel
        : ActionNodeViewModel
    {

        /// <summary>
        /// Initializes a new <see cref="FunctionRefNodeViewModel"/>
        /// </summary>
        /// <param name="action">The <see cref="ActionDefinition"/> the <see cref="FunctionRefNodeViewModel"/> represents</param>
        /// <param name="function">The <see cref="FunctionReference"/> the <see cref="FunctionRefNodeViewModel"/> represents</param>
        public FunctionRefNodeViewModel(ActionDefinition action, FunctionReference function) 
            : base(action, function.RefName, "function-node")
        {
            this.Function = function;
        }

        /// <summary>
        /// Gets the <see cref="FunctionReference"/> the <see cref="FunctionRefNodeViewModel"/> represents
        /// </summary>
        public FunctionReference Function { get; }

    }

}
