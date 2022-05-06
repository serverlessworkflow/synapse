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
using Microsoft.AspNetCore.Components.Forms;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Dashboard
{
    public abstract class StateEditorModal<T>
        : EditorModal<T>, IDisposable
        where T : class, IStateModel, new()
    {
        protected EditContext? editContext;
        protected ValidationMessageStore? validationMessages;

        [CascadingParameter]
        public ICollection<StateDefinition> States { get; set; } = null!;

        protected override async Task OnParametersSetAsync()
        {
            bool refresh = false;
            if (this.model != this.Model )
            {
                refresh = true;
            }
            await base.OnParametersSetAsync();
            if (!this.model.Update && string.IsNullOrWhiteSpace(this.model.FromStateName) && this.States.Any())
            {
                this.model.FromStateName = this.States.First(state => state.IsEnd).Name;
            }
            if (refresh) {
                if (this.editContext != null)
                {
                    this.editContext.OnValidationRequested -= this.OnValidationRequested;
                }
                this.editContext = new(this.model);
                this.editContext.OnValidationRequested += this.OnValidationRequested;
                this.validationMessages = new(editContext);
            }
        }
        protected virtual void OnValidationRequested(object? sender, ValidationRequestedEventArgs args)
        {
            this.validationMessages?.Clear();
            if (string.IsNullOrWhiteSpace(this.model.Name))
            {
                this.validationMessages?.Add(() => this.model.Name, "A state name is required.");
            }
            if (!this.model.Update && this.States.Select(state => state.Name).Contains(this.model.Name))
            {
                this.validationMessages?.Add(() => this.model.Name, "Another state with the same name already exists.");
            }
        }

        protected override async Task OnValidSubmitAsync()
        {
            await base.OnValidSubmitAsync();
        }

        public void Dispose()
        {
            if (editContext is not null)
            {
                editContext.OnValidationRequested -= OnValidationRequested;
            }
        }
    }
}
