// Copyright © 2024-Present The Synapse Authors
//
// Licensed under the Apache License, Version 2.0 (the "License"),
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Json.Schema;

namespace Synapse.Dashboard.Components;

/// <summary>
/// Represents the base class for all components used to manage <see cref="IResource"/>s
/// </summary>
/// <typeparam name="TComponent">The type of component inheriting the <see cref="ResourceManagementComponent{TComponent, TStore, TState, TResource}"/></typeparam>
/// <typeparam name="TStore">The type of <see cref="ResourceManagementComponentStoreBase{TResource}"/> to use</typeparam>
/// <typeparam name="TState">The type of the component's state</typeparam>
/// <typeparam name="TResource">The type of <see cref="IResource"/> to manage</typeparam>
public abstract class ResourceManagementComponent<TComponent, TStore, TState, TResource>
    : StatefulComponent<TComponent, TStore, TState>
    where TComponent : ResourceManagementComponent<TComponent, TStore, TState, TResource>
    where TStore : ResourceManagementComponentStoreBase<TState, TResource>
    where TState : ResourceManagementComponentState<TResource>, new()
    where TResource : Resource, new()
{

    /// <summary>
    /// Gets/sets the service to build a bridge with the monaco interop extension
    /// </summary>
    [Inject]
    protected MonacoInterop? MonacoInterop { get; set; }

    /// <summary>
    /// Gets/sets the service used for JS interop
    /// </summary>
    [Inject]
    protected JSInterop jsInterop { get; set; } = default!;

    /// <summary>
    /// Gets the service used to serialize/deserialize objects to/from JSON
    /// </summary>
    [Inject]
    protected IJsonSerializer Serializer { get; set; } = null!;

    /// <summary>
    /// Gets/sets the list of displayed <see cref="Resource"/>s
    /// </summary>
    protected EquatableList<TResource>? Resources { get; set; }

    /// <summary>
    /// Gets/sets the list of selected <see cref="Resource"/>s
    /// </summary>
    protected EquatableList<string> SelectedResourceNames { get; set; } = [];

    /// <summary>
    /// Gets/sets the <see cref="Offcanvas"/> used to show the <see cref="Resource"/>'s details
    /// </summary>
    protected Offcanvas? DetailsOffCanvas { get; set; }

    /// <summary>
    /// Gets/sets the <see cref="Offcanvas"/> used to edit the <see cref="Resource"/>
    /// </summary>
    protected Offcanvas? EditorOffCanvas { get; set; }

    /// <summary>
    /// Gets/sets the <see cref="ConfirmDialog"/> used to confirm the <see cref="Resource"/>'s deletion
    /// </summary>
    protected ConfirmDialog? Dialog { get; set; }

    /// <summary>
    /// Gets/sets the <see cref="Resource"/>'s <see cref="ResourceDefinition"/>
    /// </summary>
    protected ResourceDefinition? Definition { get; set; }

    /// <summary>
    /// Gets/sets the search term to filter the resources with
    /// </summary>
    protected string? SearchTerm { get; set; }

    /// <summary>
    /// Gets/sets a boolean value that indicates whether data is currently being gathered
    /// </summary>
    protected bool Loading { get; set; } = false;

    /// <summary>
    /// Gets/sets the checkbox used to (un)select all resources
    /// </summary>
    protected ElementReference? CheckboxAll { get; set; } = null;

    string activeResourcesName = null!;
    /// <summary>
    /// Gets/sets the name of the active resource
    /// </summary>
    [Parameter] public string? Name { get; set; }

    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync().ConfigureAwait(false);
        Observable.CombineLatest(
            this.Store.Resources,
            this.Store.SelectedResourceNames,
            (resources, selectedResourceNames) => (resources, selectedResourceNames)
        ).SubscribeAsync(async (values) => {
            var (resources, selectedResourceNames) = values;
            this.OnStateChanged(_ =>
            {
                this.Resources = resources;
                this.SelectedResourceNames = selectedResourceNames;
            });
            if (this.CheckboxAll.HasValue)
            {

                if (selectedResourceNames.Count == 0)
                {
                    await this.jsInterop.SetCheckboxStateAsync(this.CheckboxAll.Value, CheckboxState.Unchecked);
                }
                else if (selectedResourceNames.Count == (resources?.Count ?? 0))
                {
                    await this.jsInterop.SetCheckboxStateAsync(this.CheckboxAll.Value, CheckboxState.Checked);
                }
                else
                {
                    await this.jsInterop.SetCheckboxStateAsync(this.CheckboxAll.Value, CheckboxState.Indeterminate);
                }
            }
        }, cancellationToken: this.CancellationTokenSource.Token);
        this.Store.ActiveResourceName.Subscribe(value => this.OnStateChanged(_ => this.activeResourcesName = value), token: this.CancellationTokenSource.Token);
        this.Store.SearchTerm.Subscribe(value => this.OnStateChanged(_ => this.SearchTerm = value), token: this.CancellationTokenSource.Token);
        this.Store.Loading.Subscribe(value => this.OnStateChanged(_ => this.Loading = value), token: this.CancellationTokenSource.Token);
        this.Store.Definition.SubscribeAsync(async definition =>
        {
            if (this.Definition != definition)
            {
                this.Definition = definition;
                if (this.Definition != null && this.MonacoInterop != null)
                {
                    await this.MonacoInterop.AddValidationSchemaAsync(this.Serializer.SerializeToText(this.Definition.Spec.Versions.First().Schema.OpenAPIV3Schema ?? new JsonSchemaBuilder().Build()), $"https://synapse.io/schemas/{typeof(TResource).Name.ToLower()}.json", $"{typeof(TResource).Name.ToLower()}*").ConfigureAwait(false);
                }
            }
        }, cancellationToken: this.CancellationTokenSource.Token);
        this.Store.ActiveResource.SubscribeAsync(async resource =>
        {
            if (resource != null)
            {
                await this.OnShowResourceDetailsAsync(resource);
            }
        }, cancellationToken: CancellationTokenSource.Token);
    }


    /// <inheritdoc/>
    protected override async Task OnParametersSetAsync()
    {
        if (!string.IsNullOrEmpty(Name) && Name != activeResourcesName)
        {
            Store.SetActiveResourceName(Name);
        }
        await base.OnParametersSetAsync();
    }

    /// <summary>
    /// Handles search input value changes
    /// </summary>
    /// <param name="e">the <see cref="ChangeEventArgs"/> to handle</param>
    protected void OnSearchInput(ChangeEventArgs e)
    {
        this.Store.SetSearchTerm(e.Value?.ToString());
    }

    /// <summary>
    /// Handles the deletion of the targeted <see cref="Resource"/>
    /// </summary>
    /// <param name="resource">The <see cref="Resource"/> to delete</param>
    protected async Task OnDeleteResourceAsync(TResource resource)
    {
        if (this.Dialog == null) return;
        var confirmation = await this.Dialog.ShowAsync(
            title: $"Are you sure you want to delete '{resource.Metadata.Name}'?",
            message1: $"The {typeof(TResource).Name.ToLower()} will be permanently deleted. Are you sure you want to proceed ?",
            confirmDialogOptions: new ConfirmDialogOptions()
            {
                YesButtonColor = ButtonColor.Danger,
                YesButtonText = "Delete",
                NoButtonText = "Abort",
                IsVerticallyCentered = true
            }
        );
        if (!confirmation) return;
        await this.Store.DeleteResourceAsync(resource);
    }

    /// <summary>
    /// Handles the deletion of the selected <see cref="Resource"/>s
    /// </summary>
    protected async Task OnDeleteSelectedResourcesAsync()
    {
        if (this.Dialog == null) return;
        if (this.SelectedResourceNames.Count == 0) return;
        var confirmation = await this.Dialog.ShowAsync(
            title: $"Are you sure you want to delete {SelectedResourceNames.Count} resource{(SelectedResourceNames.Count > 1 ? "s" : "")}?",
            message1: $"The resource{(SelectedResourceNames.Count > 1 ? "s" : "")} will be permanently deleted. Are you sure you want to proceed ?",
            confirmDialogOptions: new ConfirmDialogOptions()
            {
                YesButtonColor = ButtonColor.Danger,
                YesButtonText = "Delete",
                NoButtonText = "Abort",
                IsVerticallyCentered = true
            }
        );
        if (!confirmation) return;
        await this.Store.DeleteSelectedResourcesAsync();
    }

    /// <summary>
    /// Opens the targeted <see cref="Resource"/>'s details
    /// </summary>
    /// <param name="resource">The <see cref="Resource"/> to show the details for</param>
    protected Task OnShowResourceDetailsAsync(TResource resource)
    {
        if (this.DetailsOffCanvas == null) return Task.CompletedTask;
        var parameters = new Dictionary<string, object>
        {
            { nameof(ResourceEditor<TResource>.Resource), resource }
        };
        return this.DetailsOffCanvas.ShowAsync<ResourceDetails<TResource>>(title: typeof(TResource).Name + " details", parameters: parameters);
    }

    /// <summary>
    /// Opens the targeted <see cref="Resource"/>'s edition
    /// </summary>
    /// <param name="resource">The <see cref="Resource"/> to edit</param>
    protected virtual Task OnShowResourceEditorAsync(TResource? resource = null)
    {
        if (this.EditorOffCanvas == null) return Task.CompletedTask;
        var parameters = new Dictionary<string, object>
        {
            { nameof(ResourceEditor<TResource>.Resource), resource! }
        };
        string actionType = resource == null ? "creation" : "edition";
        return this.EditorOffCanvas.ShowAsync<ResourceEditor<TResource>>(title: typeof(TResource).Name + " " + actionType, parameters: parameters);
    }

}


/// <summary>
/// Represents the base class for all components used to manage <see cref="IResource"/>s
/// </summary>
/// <typeparam name="TComponent">The type of component inheriting the <see cref="ResourceManagementComponent{TComponent, TStore, TResource}"/></typeparam>
/// <typeparam name="TStore">The type of <see cref="ResourceManagementComponentStoreBase{TResource}"/> to use</typeparam>
/// <typeparam name="TResource">The type of <see cref="IResource"/> to manage</typeparam>
public abstract class ResourceManagementComponent<TComponent, TStore, TResource>
    : ResourceManagementComponent<TComponent, TStore, ResourceManagementComponentState<TResource>, TResource>
    where TComponent : ResourceManagementComponent<TComponent, TStore, TResource>
    where TStore : ResourceManagementComponentStoreBase<TResource>
    where TResource : Resource, new()
{



}