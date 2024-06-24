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

using Microsoft.Extensions.Hosting;

namespace Synapse.Dashboard.Services;

/// <summary>
/// Represents a service used to manage security tokens
/// </summary>
/// <param name="jsRuntime">An instance of a JavaScript runtime to which calls may be dispatched.</param>
/// <param name="hostEnvironment">The service used to provide information about the hosting environment an application is running in.</param>
public class SecurityTokenManager(IJSRuntime jsRuntime, IWebAssemblyHostEnvironment hostEnvironment)
    : ISecurityTokenManager
{

    private readonly string _tokenStorageKey = "synapse-static-token";
    private readonly string _tokenStorageProvider = hostEnvironment.IsDevelopment() ? "localStorage" : "sessionStorage";

    /// <summary>
    /// Gets the current static bearer token
    /// </summary>
    protected string? Token { get; private set; }

    /// <inheritdoc/>
    public async Task<string?> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(this.Token))
        {
            this.Token = await jsRuntime.InvokeAsync<string>($"window.{_tokenStorageProvider}.getItem", _tokenStorageKey);
        }
        return this.Token;
    }


    /// <inheritdoc/>
    public async Task SetTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        await jsRuntime.InvokeAsync<string>($"window.{_tokenStorageProvider}.setItem", _tokenStorageKey, token);
        this.Token = token;
    }

}
