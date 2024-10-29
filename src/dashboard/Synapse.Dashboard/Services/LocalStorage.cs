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
/// The service used to build a bridge with the localStorage
/// </summary>
/// <param name="jsRuntime">The service used to interop with JS</param>
public class LocalStorage(IJSRuntime jsRuntime)
    : ILocalStorage
{

    /// <inheritdoc/>
    public async ValueTask<string?> GetItemAsync(string key)
    {
        return await jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
    }

    /// <inheritdoc/>
    public async ValueTask SetItemAsync(string key, string value)
    {
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
    }

    /// <inheritdoc/>
    public async ValueTask RemoveItemAsync(string key)
    {
        await jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }

    /// <inheritdoc/>
    public async ValueTask ClearAsync()
    {
        await jsRuntime.InvokeVoidAsync("localStorage.clear");
    }

    /// <inheritdoc/>
    public async ValueTask<int> LengthAsync()
    {
        return await jsRuntime.InvokeAsync<int>("eval", "localStorage.length");
    }

    /// <inheritdoc/>
    public async ValueTask<string?> KeyAsync(int index)
    {
        return await jsRuntime.InvokeAsync<string?>("localStorage.key", index);
    }

    /// <inheritdoc/>
    public async ValueTask<IEnumerable<string>> KeysAsync()
    {
        return await jsRuntime.InvokeAsync<IEnumerable<string>>("eval", "Object.keys(localStorage)");
    }
}
