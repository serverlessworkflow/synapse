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
    public delegate Task AddStateEventHandler(StateNodeViewModel? sender, MouseEventArgs e);
    public delegate Task EditStateEventHandler(StateNodeViewModel sender, MouseEventArgs e);
    public delegate Task MoveStateEventHandler(StateNodeViewModel sender, MouseEventArgs e);

    public interface IWorkflowGraphEventDispatcher
    {
        event AddStateEventHandler? AddStateAsync;
        event EditStateEventHandler? EditStateAsync;
        event MoveStateEventHandler? MoveStateBackwardAsync;
        event MoveStateEventHandler? MoveStateForwardAsync;

        Task OnAddStateAsync(StateNodeViewModel? sender, MouseEventArgs e);
        Task OnEditStateAsync(StateNodeViewModel sender, MouseEventArgs e);
        Task OnMoveStateBackwardAsync(StateNodeViewModel sender, MouseEventArgs e);
        Task OnMoveStateForwardAsync(StateNodeViewModel sender, MouseEventArgs e);
    }
}
