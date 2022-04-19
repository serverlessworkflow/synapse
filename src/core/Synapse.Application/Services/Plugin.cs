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

using System.Reflection;
using System.Runtime.Loader;

namespace Synapse.Application.Services
{
    /// <summary>
    /// Represents the default implementation of the <see cref="IPlugin"/> interface
    /// </summary>
    public class Plugin
        : IPlugin
    {

        /// <summary>
        /// Initializes a new <see cref="Plugin"/>
        /// </summary>
        /// <param name="assemblyLoadContext">The <see cref="IPlugin"/>'s <see cref="System.Runtime.Loader.AssemblyLoadContext"/></param>
        /// <param name="assembly">The <see cref="IPlugin"/>'s <see cref="System.Reflection.Assembly"/></param>
        public Plugin(AssemblyLoadContext assemblyLoadContext, Assembly assembly)
        {
            this.AssemblyLoadContext = assemblyLoadContext ?? throw new ArgumentNullException(nameof(assemblyLoadContext));
            this.Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        }

        /// <inheritdoc/>
        public AssemblyLoadContext AssemblyLoadContext { get; }

        /// <inheritdoc/>
        public Assembly Assembly { get; }

    }

}
