// Copyright © 2024-Present Neuroglia SRL. All rights reserved.
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

using Synapse.Resources;
using Neuroglia.Data.Infrastructure.ResourceOriented;
using System.Diagnostics;

namespace Synapse;

/// <summary>
/// Exposes defaults about Synapse
/// </summary>
public static class SynapseDefaults
{

    /// <summary>
    /// Gets the default group for Synapse resources
    /// </summary>
    public const string ResourceGroup = "cloud-flows.io";

    /// <summary>
    /// Exposes Synapse default resources
    /// </summary>
    public static class Resources
    {

        /// <summary>
        /// Exposes Synapse resource definitions
        /// </summary>
        public static class Definitions
        {

            /// <summary>
            /// Gets the definition of Workflow resources
            /// </summary>
            public static ResourceDefinition Workflow { get; } = new WorkflowResourceDefinition();

            /// <summary>
            /// Gets the definition of WorkflowInstance resources
            /// </summary>
            public static ResourceDefinition WorkflowInstance { get; } = new WorkflowInstanceResourceDefinition();

            /// <summary>
            /// Gets a new <see cref="IEnumerable{T}"/> containing Synapse default resource definitions
            /// </summary>
            /// <returns></returns>
            public static IEnumerable<ResourceDefinition> AsEnumerable()
            {
                yield return Workflow;
                yield return WorkflowInstance;
            }

        }

    }

    /// <summary>
    /// Exposes constants about Synapse application telemetry
    /// </summary>
    public static class Telemetry
    {

        /// <summary>
        /// Exposes the Synapse application's <see cref="System.Diagnostics.ActivitySource"/>
        /// </summary>
        public static ActivitySource ActivitySource { get; set; } = null!;

    }

}