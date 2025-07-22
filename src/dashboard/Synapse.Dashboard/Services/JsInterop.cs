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

namespace Synapse.Dashboard.Services;

/// <summary>
/// The service used to build a bridge with JS interop
/// </summary>
/// <param name="jsRuntime">The service used to interop with JS</param>
/// <param name="storage">The service used to build a bridge with the localStorage</param>
/// <param name="monacoEditorHelper">The serviceused to facilitate the Monaco editor configuration</param>
public class JSInterop(IJSRuntime jsRuntime, ILocalStorage storage, IMonacoEditorHelper monacoEditorHelper)
    : IAsyncDisposable
{

    /// <summary>
    /// A reference to the js interop module
    /// </summary>
    readonly Lazy<Task<IJSObjectReference>> moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/js-interop.js?v=2").AsTask());

    /// <summary>
    /// Sets a checkbox tri-state
    /// </summary>
    /// <param name="checkbox">The <see cref="ElementReference"/> of the checkbox</param>
    /// <param name="state">The <see cref="CheckboxState"/> to set</param>
    /// <returns>A <see cref="ValueTask"/></returns>
    public async ValueTask SetCheckboxStateAsync(ElementReference checkbox, CheckboxState state)
    {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync("setCheckboxState", checkbox, state);
    }

    /// <summary>
    /// Scrolls down the provided element
    /// </summary>
    /// <param name="element">The <see cref="ElementReference"/> to scorll</param>
    /// <param name="height">The height to scroll to, down to the end if not provided</param>
    /// <returns></returns>
    public async ValueTask ScrollDownAsync(ElementReference element, int? height = null)
    {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync("scrollDown", element, height);
    }

    /// <summary>
    /// Sets the theme
    /// </summary>
    /// <param name="theme">The theme to set</param>
    /// <returns></returns>
    public async ValueTask SetThemeAsync(string theme)
    {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync("setTheme", theme);
        await storage.SetItemAsync("preferredTheme", theme);
        await monacoEditorHelper.ChangePreferredThemeAsync(theme);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
        GC.SuppressFinalize(this);
    }

}
