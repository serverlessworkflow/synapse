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

namespace Synapse
{
    /// <summary>
    /// Defines extensions for <see cref="IWorkflowRuntimeProxyManager"/>s
    /// </summary>
    public static class IWorkflowRuntimeProxyManagerExtensions
    {

        /// <summary>
        /// Attempts to get the <see cref="IWorkflowRuntimeProxy"/> for the specified <see cref="V1WorkflowInstance"/>
        /// </summary>
        /// <param name="proxyManager"></param>
        /// <param name="workflowInstanceId">The id of the <see cref="V1WorkflowInstance"/> to get the <see cref="IWorkflowRuntimeProxy"/> for</param>
        /// <param name="proxy">The <see cref="IWorkflowRuntimeProxy"/> for the specified <see cref="V1WorkflowInstance"/>, if any</param>
        /// <returns>A boolean indicating whether or not a proxy for the specified <see cref="V1WorkflowInstance"/> could be found</returns>
        public static bool TryGetProxy(this IWorkflowRuntimeProxyManager proxyManager, string workflowInstanceId, out IWorkflowRuntimeProxy proxy)
        {
            proxy = null!;
            try
            {
                proxy = proxyManager.GetProxy(workflowInstanceId);
                return proxy != null;
            }
            catch
            {
                return false;
            }
        }

    }

}
