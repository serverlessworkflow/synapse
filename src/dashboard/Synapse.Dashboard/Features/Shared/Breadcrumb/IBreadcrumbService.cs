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

using System.ComponentModel;

namespace Synapse.Dashboard
{
    /// <summary>
    /// The service used to manage the breadcrumb
    /// </summary>
    public interface IBreadcrumbService
    {
        /// <summary>
        /// Notifies when the list has changed
        /// </summary>
        event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// The list of displayed <see cref="IBreadcrumbItem"/>
        /// </summary>
        List<IBreadcrumbItem> Items { get; init; }

        /// <summary>
        /// Adds the specified <see cref="IBreadcrumbItem"/> to the list
        /// </summary>
        /// <param name="breadcrumbItem"></param>
        /// <returns></returns>
        Task AddItem(IBreadcrumbItem breadcrumbItem);

        /// <summary>
        /// Creates a new <see cref="IBreadcrumbItem"/> with the specified label and icon for the active route and adds it to the list
        /// </summary>
        /// <param name="label"></param>
        /// <param name="icon"></param>
        /// <returns>The created  <see cref="IBreadcrumbItem"/></returns>
        Task<IBreadcrumbItem> AddCurrentUri(string label, string? icon = null);

        /// <summary>
        /// Adds the specified <see cref="IBreadcrumbItem"/> to the list
        /// </summary>
        /// <param name="breadcrumbItem"></param>
        /// <returns></returns>
        Task RemoveItem(IBreadcrumbItem breadcrumbItem);

        /// <summary>
        /// Clears the <see cref="IBreadcrumbItem"/>'s list
        /// </summary>
        /// <returns></returns>
        Task Clear();

        /// <summary>
        /// Replaces the current <see cref="IBreadcrumbItem"/>'s list with the provided one
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        Task Use(IEnumerable<IBreadcrumbItem> list);

        /// <summary>
        /// Navigate to the provided item and set the breadcrumb state accordingly
        /// </summary>
        /// <param name="breadcrumbItem"></param>
        /// <returns></returns>
        Task NavigateTo(IBreadcrumbItem breadcrumbItem);
    }
}
