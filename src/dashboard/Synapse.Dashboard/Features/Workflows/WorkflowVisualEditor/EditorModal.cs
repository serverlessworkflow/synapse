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
using Microsoft.AspNetCore.Components;

namespace Synapse.Dashboard
{
    public abstract class EditorModal<T>
        : ModalBase
        where T : new()
    {
        [Parameter]
        public T Model { get; set; } = default(T)!;
        protected T model = default(T)!;

        [Parameter]
        public virtual EventCallback<T> OnSaveModel { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            this.model = this.Model;
            await base.OnParametersSetAsync();
        }

        protected virtual async Task OnValidSubmitAsync()
        {
            if (this.OnSaveModel.HasDelegate)
            {
                await this.OnSaveModel.InvokeAsync(this.model);
            }
            await this.HideAsync();
        }
    }
}
