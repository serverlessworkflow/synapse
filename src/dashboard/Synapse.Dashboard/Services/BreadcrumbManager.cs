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

using System.ComponentModel;

namespace Synapse.Dashboard.Services;

/// <summary>
/// Represents the default implementation of the <see cref="IBreadcrumbManager"/> interface
/// </summary>
/// <param name="navigationManager">The service used for managing navigation with the current application</param>
public class BreadcrumbManager(NavigationManager navigationManager)
    : IBreadcrumbManager
{

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    List<Components.BreadcrumbItem> _items = [];

    /// <summary>
    /// Gets the service used for managing navigation with the current application
    /// </summary>
    protected NavigationManager NavigationManager { get; } = navigationManager;

    /// <inheritdoc/>
    public IReadOnlyCollection<Components.BreadcrumbItem> Items => this._items;

    /// <inheritdoc/>
    public virtual Components.BreadcrumbItem Add(Components.BreadcrumbItem breadcrumbItem)
    {
        ArgumentNullException.ThrowIfNull(breadcrumbItem);
        this._items.Add(breadcrumbItem);
        this.NotifyChange();
        return breadcrumbItem;
    }

    /// <inheritdoc/>
    public virtual Components.BreadcrumbItem Add(string label, string link, string? icon = null) => this.Add(new(label, link, icon));

    /// <inheritdoc/>
    public virtual void Remove(Components.BreadcrumbItem breadcrumbItem)
    {
        ArgumentNullException.ThrowIfNull(breadcrumbItem);
        this._items.Remove(breadcrumbItem);
        this.NotifyChange();
    }

    /// <inheritdoc/>
    public virtual void Clear()
    {
        this._items.Clear();
        this.NotifyChange();
    }

    /// <inheritdoc/>
    public virtual void Use(params Components.BreadcrumbItem[] breadcrumbs)
    {
        this._items = [.. breadcrumbs];
        this.NotifyChange();
    }

    /// <inheritdoc/>
    public virtual void NavigateTo(Components.BreadcrumbItem breadcrumbItem)
    {
        ArgumentNullException.ThrowIfNull(breadcrumbItem);
        var itemIndex = this._items.IndexOf(breadcrumbItem);
        var breadcrumbItems = this._items.Take(itemIndex + 1).ToArray();
        this.Use(breadcrumbItems);
        if (breadcrumbItem.Link != null)
        {
            this.NavigationManager.NavigateTo(breadcrumbItem.Link);
        }
    }

    /// <summary>
    /// Notifies listeners that the breadcrumbs have changed
    /// </summary>
    protected void NotifyChange() => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Items)));

}