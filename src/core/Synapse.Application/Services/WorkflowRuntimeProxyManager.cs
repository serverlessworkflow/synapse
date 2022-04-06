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

using System.Collections.Concurrent;

namespace Synapse.Application.Services
{
    /// <summary>
    /// Represents the default implementation of the <see cref="IWorkflowRuntimeProxyManager"/> interface
    /// </summary>
    public class WorkflowRuntimeProxyManager
        : IWorkflowRuntimeProxyManager
    {

        /// <summary>
        /// Gets a <see cref="ConcurrentDictionary{TKey, TValue}"/> containing the workflow id mappings of all active <see cref="IWorkflowRuntimeProxy"/> instances
        /// </summary>
        protected ConcurrentDictionary<string, IWorkflowRuntimeProxy> RuntimeProxies { get; } = new();

        /// <inheritdoc/>
        public virtual IWorkflowRuntimeProxy Register(IWorkflowRuntimeProxy runtimeProxy)
        {
            if(runtimeProxy == null)
                throw new ArgumentNullException(nameof(runtimeProxy));
            this.RuntimeProxies.AddOrUpdate(runtimeProxy.Id, runtimeProxy, (key, current) => 
            {
                current.Dispose();
                return runtimeProxy;
            });
            runtimeProxy.Disposed += this.OnRuntimeProxyDisposed;
            return runtimeProxy;
        }

        /// <inheritdoc/>
        public virtual IWorkflowRuntimeProxy GetProxy(string id)
        {
            if(string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            return this.RuntimeProxies[id];
        }

        /// <inheritdoc/>
        public virtual void Unregister(IWorkflowRuntimeProxy runtimeProxy)
        {
            if (runtimeProxy == null)
                throw new ArgumentNullException(nameof(runtimeProxy));
            runtimeProxy.Dispose();
            runtimeProxy.Disposed -= this.OnRuntimeProxyDisposed;
        }

        /// <summary>
        /// Handles the disposal of the specified <see cref="IWorkflowRuntimeProxy"/>
        /// </summary>
        /// <param name="sender">The disposed <see cref="IWorkflowRuntimeProxy"/></param>
        /// <param name="e">The disposal <see cref="EventArgs"/></param>
        protected virtual void OnRuntimeProxyDisposed(object? sender, EventArgs e)
        {
            var runtimeProxy = (IWorkflowRuntimeProxy)sender!;
            this.RuntimeProxies.TryRemove(runtimeProxy.Id, out _);
        }

    }

}
