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
    public const string ResourceGroup = "synapse.io";

    /// <summary>
    /// Exposes Synapse environment variables
    /// </summary>
    public static class EnvironmentVariables
    {

        /// <summary>
        /// Gets the prefix for all Synapse environment variables
        /// </summary>
        public const string Prefix = "SYNAPSE_";

        /// <summary>
        /// Gets the environment variable used to configure Synapse to skip certificate validation by default, for example when performing HTTP requests
        /// </summary>
        public const string SkipCertificateValidation = Prefix + "SKIP_CERTIFICATE_VALIDATION";

        /// <summary>
        /// Exposes constants about API-related environment variables
        /// </summary>
        public static class Api
        {

            /// <summary>
            /// Gets the prefix for all API related environment variables
            /// </summary>
            public const string Prefix = EnvironmentVariables.Prefix + "API_";

            /// <summary>
            /// Gets the environment variable used to configure the base uri of the Synapse API to use
            /// </summary>
            public const string Uri = Prefix + "URI";

        }

        /// <summary>
        /// Exposes constants about correlator-related environment variables
        /// </summary>
        public static class Correlator
        {

            /// <summary>
            /// Gets the prefix for all correlator related environment variables
            /// </summary>
            public const string Prefix = EnvironmentVariables.Prefix + "CORRELATOR_";

            /// <summary>
            /// Gets the environment variable used to configure the correlator's namespace
            /// </summary>
            public const string Namespace = Prefix + "NAMESPACE";
            /// <summary>
            /// Gets the environment variable used to configure the correlator's name
            /// </summary>
            public const string Name = Prefix + "NAME";

        }

        /// <summary>
        /// Exposes constants about operator-related environment variables
        /// </summary>
        public static class Operator
        {

            /// <summary>
            /// Gets the prefix for all operator related environment variables
            /// </summary>
            public const string Prefix = EnvironmentVariables.Prefix + "OPERATOR_";

            /// <summary>
            /// Gets the environment variable used to configure the operator's namespace
            /// </summary>
            public const string Namespace = Prefix + "NAMESPACE";
            /// <summary>
            /// Gets the environment variable used to configure the operator's name
            /// </summary>
            public const string Name = Prefix + "NAME";
            /// <summary>
            /// Gets the environment variable used to configure the API used by runners spawned by the operator
            /// </summary>
            public const string Api = Prefix + "API";

        }

        /// <summary>
        /// Exposes constants about workflow-related environment variables
        /// </summary>
        public static class Workflow
        {

            /// <summary>
            /// Gets the prefix for all api related environment variables
            /// </summary>
            public const string Prefix = EnvironmentVariables.Prefix + "WORKFLOW_";

            /// <summary>
            /// Gets the environment variable that holds the qualified name of workflow instance to run
            /// </summary>
            public const string Instance = Prefix + "INSTANCE";

        }

    }

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
            /// Gets the definition of Correlation resources
            /// </summary>
            public static ResourceDefinition Correlation { get; } = new CorrelationResourceDefinition();

            /// <summary>
            /// Gets the definition of Correlator resources
            /// </summary>
            public static ResourceDefinition Correlator { get; } = new CorrelatorResourceDefinition();

            /// <summary>
            /// Gets the definition of Operator resources
            /// </summary>
            public static ResourceDefinition Operator { get; } = new OperatorResourceDefinition();

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
                yield return Correlation;
                yield return Correlator;
                yield return Operator;
                yield return Workflow;
                yield return WorkflowInstance;
            }

        }

        /// <summary>
        /// Exposes the resource labels used by Synapse
        /// </summary>
        public static class Labels
        {

            /// <summary>
            /// Gets the prefix of all Synapse labels
            /// </summary>
            public const string Prefix = "synapse.io/";

            /// <summary>
            /// Gets the label used by Synapse correlators to claim correlations
            /// </summary>
            public const string Correlator = Prefix + "correlator";
            /// <summary>
            /// Gets the label used by Synapse operators to claim workflows or workflow instances
            /// </summary>
            public const string Operator = Prefix + "operator";

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