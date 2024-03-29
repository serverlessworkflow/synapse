﻿/*
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

namespace Synapse.Dashboard
{
    /// <summary>
    /// Represents a inject state node
    /// </summary>
    public class InjectNodeViewModel
        : WorkflowNodeViewModel
    {

        /// <summary>
        /// Initializes a new <see cref="InjectNodeViewModel"/>
        /// </summary>
        /// <param name="data"></param>
        public InjectNodeViewModel(string data)
            : base("", "inject-node", null, Neuroglia.Blazor.Dagre.Constants.NodeHeight * 1.5, Neuroglia.Blazor.Dagre.Constants.NodeHeight * 1.5)
        {
            this.Data = data;
            this.ComponentType = typeof(InjectNodeTemplate);
        }

        public string Data { get; set; }

    }
    

}
