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

using ServerlessWorkflow.Sdk;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Dashboard
{
    /// <summary>
    /// Represents an <see cref="EventDefinition"/> reference <see cref="NodeViewModel"/>
    /// </summary>
    public class EventNodeViewModel
        : LabeledNodeViewModel
    {

        /// <summary>
        /// Initializes a new <see cref="EventNodeViewModel"/>
        /// </summary>
        /// <param name="kind">The kind of the <see cref="EventDefinition"/> the <see cref="EventNodeViewModel"/> represents</param>
        /// <param name="refName">The name of the <see cref="EventDefinition"/> the <see cref="EventNodeViewModel"/> represents</param>
        public EventNodeViewModel(EventKind kind, string refName)
            :base(refName, "event-node")
        {
            this.Kind = kind;
            this.RefName = refName;
            this.ComponentType = typeof(EventNodeTemplate);
        }

        /// <summary>
        /// Gets the kind of the <see cref="EventDefinition"/> the <see cref="EventNodeViewModel"/> represents
        /// </summary>
        public EventKind Kind { get; }

        /// <summary>
        /// Gets the name of the <see cref="EventDefinition"/> the <see cref="EventNodeViewModel"/> represents
        /// </summary>
        public string RefName { get; }

    }

}
