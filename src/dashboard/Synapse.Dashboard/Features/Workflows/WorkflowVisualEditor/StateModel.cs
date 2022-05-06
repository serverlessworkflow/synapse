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
    public class StateModel<T>
        : IStateModel
        where T : StateDefinition, new()
    {
        public string? FromStateName { get; set; }
        public T State { get; set; }

        public bool Update { get; set; }

        public string Name
        {
            get
            {
                return this.State.Name;
            }
            set
            {
                this.State.Name = value;
            }
        }

        public StateModel()
            : this(null, null)
        { }

        public StateModel(T? state = null, string? FromStateName = null, bool update = false)
        {
            this.State = state ?? new();
            this.FromStateName = FromStateName;
            this.Update = update;
        }
    }
}
