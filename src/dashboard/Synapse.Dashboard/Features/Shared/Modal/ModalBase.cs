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
    public abstract class ModalBase
        : ComponentBase
    {

        [Parameter]
        public virtual ModalSize Size { get; set; } = ModalSize.Large;

        [Parameter]
        public virtual bool ShowCloseIcon { get; set; } = true;

        [Parameter]
        public virtual EventCallback<bool> OnActiveChange { get; set; }

        public virtual bool Active { get; set; } = false;

        protected virtual string modalSizeClass => this.Size switch
        {
            ModalSize.Small => "modal-sm",
            ModalSize.Default => "",
            ModalSize.Large => "modal-lg",
            ModalSize.ExtraLarge => "modal-xl",
            _ => throw new NotSupportedException($"The specified {nameof(ModalSize)} '{this.Size}' is not supported")
        };

        public virtual async Task ToggleAsync()
        {
            this.Active = !this.Active;
            await this.OnActiveChange.InvokeAsync(this.Active);
        }

        public virtual async Task ShowAsync()
        {
            this.Active = true;
            await this.OnActiveChange.InvokeAsync(this.Active);
        }

        public virtual async Task HideAsync()
        {
            this.Active = false;
            await this.OnActiveChange.InvokeAsync(this.Active);
        }
    }
}
