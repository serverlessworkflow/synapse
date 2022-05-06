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
using Microsoft.AspNetCore.Components.Web;

namespace Synapse.Dashboard
{
    public class WorkflowGraphEventDispatcher
        : IWorkflowGraphEventDispatcher
    {
        public event AddStateEventHandler? AddStateAsync;
        public event EditStateEventHandler? EditStateAsync;
        public event MoveStateEventHandler? MoveStateBackwardAsync;
        public event MoveStateEventHandler? MoveStateForwardAsync;

        public virtual async Task OnAddStateAsync(StateNodeViewModel? sender, MouseEventArgs e)
        {
            if (this.AddStateAsync != null)
            {
                await this.AddStateAsync.Invoke(sender, e);
            }
        }

        public virtual async Task OnEditStateAsync(StateNodeViewModel sender, MouseEventArgs e)
        {
            if (this.EditStateAsync != null)
            {
                await this.EditStateAsync.Invoke(sender, e);
            }
        }

        public virtual async Task OnMoveStateBackwardAsync(StateNodeViewModel sender, MouseEventArgs e)
        {
            if (this.MoveStateBackwardAsync != null)
            {
                await this.MoveStateBackwardAsync.Invoke(sender, e);
            }
        }
        public virtual async Task OnMoveStateForwardAsync(StateNodeViewModel sender, MouseEventArgs e)
        {
            if (this.MoveStateForwardAsync != null)
            {
                await this.MoveStateForwardAsync.Invoke(sender, e);
            }
        }
    }
}
