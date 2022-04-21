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

using Synapse.Infrastructure.Plugins;
using System.Reflection;
using System.Runtime.Loader;

namespace Synapse.Application.Services
{

    /// <summary>
    /// Represents an <see cref="AssemblyLoadContext"/> used to load <see cref="IPlugin"/> assemblies
    /// </summary>
    public class PluginAssemblyLoadContext
        : AssemblyLoadContext
    {

        /// <summary>
        /// Initializes a new <see cref="PluginAssemblyLoadContext"/>
        /// </summary>
        /// <param name="assemblyDependencyResolver">The service used to resolve assembly dependencies</param>
        public PluginAssemblyLoadContext(AssemblyDependencyResolver assemblyDependencyResolver)
            : base("PluginAssemblyLoadContext", true)
        {
            this.AssemblyDependencyResolver = assemblyDependencyResolver;
        }

        /// <summary>
        /// Initializes a new <see cref="PluginAssemblyLoadContext"/>
        /// </summary>
        /// <param name="path">The path of the plugin <see cref="Assembly"/> to load</param>
        public PluginAssemblyLoadContext(string path)
            : this(new AssemblyDependencyResolver(path))
        {
            var directory = new DirectoryInfo(Path.GetDirectoryName(path)!);
            foreach(var assemblyFile in directory.GetFiles("*.dll"))
            {
                var assemblyName = new AssemblyName(Path.GetFileNameWithoutExtension(assemblyFile.FullName)!);
                if(Default.Assemblies.Any(a => a.GetName().Name == assemblyName.Name))
                    assemblyFile.Delete();
            }
        }

        /// <summary>
        /// Gets the service used to resolve assembly dependencies
        /// </summary>
        protected AssemblyDependencyResolver AssemblyDependencyResolver { get; }

        /// <inheritdoc/>
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var assembly = Default.Assemblies.FirstOrDefault(a => a.FullName == assemblyName.FullName);
            if (assembly != null)
                return assembly;
            var assemblyPath = this.AssemblyDependencyResolver.ResolveAssemblyToPath(assemblyName);
            if (string.IsNullOrWhiteSpace(assemblyPath))
                return null;
            return this.LoadFromAssemblyPath(assemblyPath);
        }

        /// <inheritdoc/>
        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var assemblyPath = this.AssemblyDependencyResolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (string.IsNullOrWhiteSpace(assemblyPath))
                return IntPtr.Zero;
            return this.LoadUnmanagedDllFromPath(assemblyPath);
        }

    }

}
