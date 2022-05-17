/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System.Collections.Concurrent;

namespace Synapse.Dashboard.Services
{
    /// <summary>
    /// Represents the default implementation of the <see cref="IStyleManager"/> interface
    /// </summary>
    public class StyleManager
        : IStyleManager
    {

        private const string GetComputedStylePropertyJSMethodName = "getComputedStyleProperty";

        /// <summary>
        /// Initializes a new <see cref="StyleManager"/>
        /// </summary>
        /// <param name="jSRuntime">The service used to interact with JS</param>
        public StyleManager(IJSRuntime jSRuntime)
        {
            this.JSRuntime = jSRuntime;
        }

        /// <summary>
        /// Gets the service used to interact with JS
        /// </summary>
        protected IJSRuntime JSRuntime { get; }

        /// <summary>
        /// Gets the key/value mappings of cached variables
        /// </summary>
        protected ConcurrentDictionary<string, string> Variables { get; } = new();

        /// <inheritdoc/>
        public virtual async Task<string> GetVariableValueAsync(string variableName, CancellationToken cancellationToken = default)
        {
            if(!this.Variables.TryGetValue(variableName, out var value))
            {
                value = await this.JSRuntime.InvokeAsync<string>(GetComputedStylePropertyJSMethodName, variableName);
                this.Variables.TryAdd(variableName, value);
            }
            return value;
        }

    }

}
