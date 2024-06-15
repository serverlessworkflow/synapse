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
    /// Exposes Synapse audiences
    /// </summary>
    public static class Audiences
    {

        /// <summary>
        /// Gets the Synapse API audience
        /// </summary>
        public const string Api = "synapse-api";

    }

    /// <summary>
    /// Exposes Synapse claims
    /// </summary>
    public static class Claims
    {

        /// <summary>
        /// Gets the service account claim type
        /// </summary>
        public const string ServiceAccount = "service_account";

    }

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
            /// <summary>
            /// Gets the token to use to connect to the Synapse API
            /// </summary>
            public const string Token = Prefix + "TOKEN";

            /// <summary>
            /// Exposes constants about environment variables related to API authentication
            /// </summary>
            public static class Authentication
            {

                /// <summary>
                /// Gets the prefix for all API related environment variables
                /// </summary>
                public const string Prefix = Api.Prefix + "AUTH_";

                /// <summary>
                /// Gets the name of the environment variables used to specify the YAML file that defines the users to generate a static token for
                /// </summary>
                public const string File = Prefix + "TOKEN_FILE";

                /// <summary>
                /// Exposes constants about environment variables related to the API's JWT Bearer authentication scheme, if any
                /// </summary>
                public static class Jwt
                {

                    /// <summary>
                    /// Gets the prefix for all JWT Bearer related environment variables
                    /// </summary>
                    public const string Prefix = Authentication.Prefix + "JWT_";

                    /// <summary>
                    /// Gets the name of the environment variables used to specify the JWT Bearer authority to use
                    /// </summary>
                    public const string Authority = Prefix + "AUTHORITY";
                    /// <summary>
                    /// Gets the name of the environment variables used to specify the JWT Bearer audience
                    /// </summary>
                    public const string Audience = Prefix + "AUDIENCE";

                }

                /// <summary>
                /// Exposes constants about environment variables related to the API's OIDC authentication scheme, if any
                /// </summary>
                public static class Oidc
                {

                    /// <summary>
                    /// Gets the prefix for all OIDC related environment variables
                    /// </summary>
                    public const string Prefix = Authentication.Prefix + "OIDC_";

                    /// <summary>
                    /// Gets the name of the environment variables used to specify the OIDC authority to use
                    /// </summary>
                    public const string Authority = Prefix + "AUTHORITY";
                    /// <summary>
                    /// Gets the name of the environment variables used to specify the OIDC client id
                    /// </summary>
                    public const string ClientId = Prefix + "CLIENT_ID";
                    /// <summary>
                    /// Gets the name of the environment variables used to specify the OIDC client secret
                    /// </summary>
                    public const string ClientSecret = Prefix + "CLIENT_SECRET";
                    /// <summary>
                    /// Gets the name of the environment variables used to define the comma-separated OIDC scope(s) to use
                    /// </summary>
                    public const string Scope = Prefix + "SCOPE";
                    /// <summary>
                    /// Gets the name of the environment variables used to define the key used by the OIDC authority to sign tokens
                    /// </summary>
                    public const string SigningKey = Prefix + "SIGNING_KEY";

                }

            }

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
        /// Exposes constants about dashboard-related environment variables
        /// </summary>
        public static class Dashboard
        {

            /// <summary>
            /// Gets the prefix for all dashboard related environment variables
            /// </summary>
            public const string Prefix = EnvironmentVariables.Prefix + "DASHBOARD_";

            /// <summary>
            /// Gets the environment variable used to determine whether or not to serve the dashboard
            /// </summary>
            public const string Serve = Prefix + "SERVE";

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
            /// Exposes constants about an operator's runner-related environment variables
            /// </summary>
            public static class Runner
            {

                /// <summary>
                /// Gets the prefix for all operator runner related environment variables
                /// </summary>
                public const string Prefix = Operator.Prefix + "RUNNER_";

                /// <summary>
                /// Gets the environment variable used to configure the API used by runners spawned by the operator
                /// </summary>
                public const string Api = Prefix + "API";

            }

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
            /// Gets the definition of ServiceAccount resources
            /// </summary>
            public static ResourceDefinition ServiceAccount { get; } = new ServiceAccountResourceDefinition();

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
                yield return ServiceAccount;
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
            /// <summary>
            /// Gets the label used by Synapse to indicate the qualified name of the workflow used by a workflow instance
            /// </summary>
            public const string Workflow = Prefix + "workflow";
            /// <summary>
            /// Gets the label used by Synapse to indicate the version of the workflow used by a workflow instance
            /// </summary>
            public const string WorkflowVersion = Prefix + "workflow/version";
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