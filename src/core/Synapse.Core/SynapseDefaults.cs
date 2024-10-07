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

using Neuroglia.Data.Infrastructure.ResourceOriented;
using Synapse.Resources;
using System.Diagnostics;
using System.Reflection;

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
    /// Exposes constants about cloud events produced by Synapse
    /// </summary>
    public static class CloudEvents
    {

        /// <summary>
        /// Gets the type prefix for all cloud events produced by Synapse
        /// </summary>
        public const string TypePrefix = "io.synapse-wfms.events.";

        /// <summary>
        /// Exposes constants about the Synapse cloud event bus
        /// </summary>
        public static class Bus
        {

            /// <summary>
            /// Gets the name of the stream used to observe cloud events published to the Synapse cloud event bus
            /// </summary>
            public const string StreamName = "cloud-events";
            /// <summary>
            /// Gets the name of the field used to store the serialized cloud event
            /// </summary>
            public const string EventFieldName = "event";
            /// <summary>
            /// Gets the key of the list used to store all existing consumer groups
            /// </summary>
            public const string ConsumerGroupListKey = "cloud-events-consumer-groups";
            /// <summary>
            /// Gets the LUA script used to distribute cloud event bus messages amongst consumer groups
            /// </summary>
            public static string MessageDistributionScript = @"
local message = redis.call('RPOP', KEYS[1]);
if message then 
    local consumerGroups = redis.call('SMEMBERS', KEYS[2]); 
    for _, group in ipairs(consumerGroups) do 
        redis.call('LPUSH', group, message); 
    end; 
end; 
return message;
";

        }

        /// <summary>
        /// Exposes constants about workflow-related cloud events
        /// </summary>
        public static class Workflow
        {

            /// <summary>
            /// Gets the type prefix for all workflow-related cloud events produced by Synapse
            /// </summary>
            public const string TypePrefix = CloudEvents.TypePrefix + "workflows.";

            /// <summary>
            /// Exposes constants about the cloud event used to notify that a workflow started
            /// </summary>
            public static class Started
            {

                const string TypeName = "started";
                /// <summary>
                /// Gets the type of the workflow started cloud event version 1
                /// </summary>
                public const string v1 = TypePrefix + TypeName + ".v1";

            }

            /// <summary>
            /// Exposes constants about the cloud event used to notify that a workflow has been suspended
            /// </summary>
            public static class Suspended
            {

                const string TypeName = "suspended";
                /// <summary>
                /// Gets the type of the workflow suspended cloud event version 1
                /// </summary>
                public const string v1 = TypePrefix + TypeName + ".v1";

            }

            /// <summary>
            /// Exposes constants about the cloud event used to notify that a workflow has been resumed
            /// </summary>
            public static class Resumed
            {

                const string TypeName = "resumed";
                /// <summary>
                /// Gets the type of the workflow resumed cloud event version 1
                /// </summary>
                public const string v1 = TypePrefix + TypeName + ".v1";

            }

            /// <summary>
            /// Exposes constants about the cloud event used to notify that a workflow has started correlating events
            /// </summary>
            public static class CorrelationStarted
            {

                const string TypeName = "correlation-started";
                /// <summary>
                /// Gets the type of the workflow correlation started cloud event version 1
                /// </summary>
                public const string v1 = TypePrefix + TypeName + ".v1";

            }

            /// <summary>
            /// Exposes constants about the cloud event used to notify that a workflow has finished correlating events
            /// </summary>
            public static class CorrelationCompleted
            {

                const string TypeName = "correlation-completed";
                /// <summary>
                /// Gets the type of the workflow correlation completed cloud event version 1
                /// </summary>
                public const string v1 = TypePrefix + TypeName + ".v1";

            }

            /// <summary>
            /// Exposes constants about the cloud event used to notify that a workflow has faulted
            /// </summary>
            public static class Faulted
            {

                const string TypeName = "faulted";
                /// <summary>
                /// Gets the type of the workflow faulted cloud event version 1
                /// </summary>
                public const string v1 = TypePrefix + TypeName + ".v1";

            }

            /// <summary>
            /// Exposes constants about the cloud event used to notify that a workflow ran to completion
            /// </summary>
            public static class Completed
            {

                const string TypeName = "completed";
                /// <summary>
                /// Gets the type of the workflow completed cloud event version 1
                /// </summary>
                public const string v1 = TypePrefix + TypeName + ".v1";

            }

            /// <summary>
            /// Exposes constants about the cloud event used to notify that the execution of a workflow has been cancelled
            /// </summary>
            public static class Cancelled
            {

                const string TypeName = "cancelled";
                /// <summary>
                /// Gets the type of the workflow cancelled cloud event version 1
                /// </summary>
                public const string v1 = TypePrefix + TypeName + ".v1";

            }

            /// <summary>
            /// Exposes constants about the cloud event used to notify that the execution of a workflow has ended
            /// </summary>
            public static class Ended
            {

                const string TypeName = "ended";
                /// <summary>
                /// Gets the type of the workflow ended cloud event version 1
                /// </summary>
                public const string v1 = TypePrefix + TypeName + ".v1";

            }

        }

        /// <summary>
        /// Exposes constants about task-related cloud events
        /// </summary>
        public static class Task
        {

            /// <summary>
            /// Gets the type prefix for all task-related cloud events produced by Synapse
            /// </summary>
            public const string TypePrefix = CloudEvents.TypePrefix + "tasks.";

            /// <summary>
            /// Exposes constants about the cloud event used to notify that a task has been created
            /// </summary>
            public static class Created
            {

                const string TypeName = "created";
                /// <summary>
                /// Gets the type of the task created cloud event version 1
                /// </summary>
                public const string v1 = TypePrefix + TypeName + ".v1";

            }

            /// <summary>
            /// Exposes constants about the cloud event used to notify that a task started
            /// </summary>
            public static class Started
            {

                const string TypeName = "started";
                /// <summary>
                /// Gets the type of the task started cloud event version 1
                /// </summary>
                public const string v1 = TypePrefix + TypeName + ".v1";

            }

            /// <summary>
            /// Exposes constants about the cloud event used to notify that a task has been suspended
            /// </summary>
            public static class Suspended
            {

                const string TypeName = "suspended";
                /// <summary>
                /// Gets the type of the task suspended cloud event version 1
                /// </summary>
                public const string v1 = TypePrefix + TypeName + ".v1";

            }

            /// <summary>
            /// Exposes constants about the cloud event used to notify that a task has been resumed
            /// </summary>
            public static class Resumed
            {

                const string TypeName = "resumed";
                /// <summary>
                /// Gets the type of the task resumed cloud event version 1
                /// </summary>
                public const string v1 = TypePrefix + TypeName + ".v1";

            }

            /// <summary>
            /// Exposes constants about the cloud event used to notify that a task is being retried
            /// </summary>
            public static class Retrying
            {

                const string TypeName = "retrying";
                /// <summary>
                /// Gets the type of the retrying task cloud event version 1
                /// </summary>
                public const string v1 = TypePrefix + TypeName + ".v1";

            }

            /// <summary>
            /// Exposes constants about the cloud event used to notify that a task has been skipped
            /// </summary>
            public static class Skipped
            {

                const string TypeName = "skipped";
                /// <summary>
                /// Gets the type of the task skipped cloud event version 1
                /// </summary>
                public const string v1 = TypePrefix + TypeName + ".v1";

            }

            /// <summary>
            /// Exposes constants about the cloud event used to notify that a task has faulted
            /// </summary>
            public static class Faulted
            {

                const string TypeName = "faulted";
                /// <summary>
                /// Gets the type of the task faulted cloud event version 1
                /// </summary>
                public const string v1 = TypePrefix + TypeName + ".v1";

            }

            /// <summary>
            /// Exposes constants about the cloud event used to notify that a task ran to completion
            /// </summary>
            public static class Completed
            {

                const string TypeName = "completed";
                /// <summary>
                /// Gets the type of the task completed cloud event version 1
                /// </summary>
                public const string v1 = TypePrefix + TypeName + ".v1";

            }

            /// <summary>
            /// Exposes constants about the cloud event used to notify that the execution of a task has been cancelled
            /// </summary>
            public static class Cancelled
            {

                const string TypeName = "cancelled";
                /// <summary>
                /// Gets the type of the task cancelled cloud event version 1
                /// </summary>
                public const string v1 = TypePrefix + TypeName + ".v1";

            }

            /// <summary>
            /// Exposes constants about the cloud event used to notify that the execution of a task has ended
            /// </summary>
            public static class Ended
            {

                const string TypeName = "ended";
                /// <summary>
                /// Gets the type of the task ended cloud event version 1
                /// </summary>
                public const string v1 = TypePrefix + TypeName + ".v1";

            }

        }

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

            /// <summary>
            /// Exposes constants about environment variables related to cloud events published by the API
            /// </summary>
            public static class CloudEvents
            {

                /// <summary>
                /// Gets the prefix for all API related environment variables
                /// </summary>
                public const string Prefix = Api.Prefix + "CLOUD_EVENTS_";

                /// <summary>
                /// Gets the absolute uri of the endpoint the API must publish cloud events to
                /// </summary>
                public const string Endpoint = Prefix + "ENDPOINT";

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

            /// <summary>
            /// Exposes constants about the cloud events related environment variables of a correlator
            /// </summary>
            public static class Events
            {

                /// <summary>
                /// Gets the prefix for all correlator related environment variables
                /// </summary>
                public const string Prefix = Correlator.Prefix + "EVENTS_";

                /// <summary>
                /// Gets the name of the consumer group the correlator to configure belongs to, if any
                /// </summary>
                public const string ConsumerGroup = Prefix + "CONSUMER";

            }

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
        /// Exposes constants about database-related environment variables
        /// </summary>
        public static class Database
        {

            /// <summary>
            /// Gets the prefix for all correlator related environment variables
            /// </summary>
            public const string Prefix = EnvironmentVariables.Prefix + "DATABASE_";

            /// <summary>
            /// Exposes constants about environment variables related to database seeding
            /// </summary>
            public static class Seeding
            {

                /// <summary>
                /// Gets the prefix for all API related environment variables
                /// </summary>
                public const string Prefix = Database.Prefix + "SEEDING_";

                /// <summary>
                /// Gets the name of the environment variables used to configure whether or not to reset the database upon starting the API server
                /// </summary>
                public const string Reset = Prefix + "RESET";
                /// <summary>
                /// Gets the name of the environment variables used to configure the directory from which to load the static resources used to seed the database
                /// </summary>
                public const string Directory = Prefix + "DIRECTORY";
                /// <summary>
                /// Gets the name of the environment variables used to configure whether or not to overwrite existing resources
                /// </summary>
                public const string Overwrite = Prefix + "OVERWRITE";
                /// <summary>
                /// Gets the name of the environment variables used to configure the GLOB pattern used to match the static resource files to use to seed the database
                /// </summary>
                public const string FilePattern = Prefix + "FILE_PATTERN";

            }

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

        }

        /// <summary>
        /// Exposes constants about runner-related environment variables
        /// </summary>
        public static class Runner
        {

            /// <summary>
            /// Gets the prefix for all runner related environment variables
            /// </summary>
            public const string Prefix = EnvironmentVariables.Prefix + "RUNNER_";

            /// <summary>
            /// Gets the environment variable used to configure the API used by runners
            /// </summary>
            public const string Api = Prefix + "API";
            /// <summary>
            /// Gets the environment variable used to configure the container platform used by runners
            /// </summary>
            public const string ContainerPlatform = Prefix + "CONTAINER_PLATFORM";
            /// <summary>
            /// Gets the environment variable used to configure whether or not runners should publish lifecycle events
            /// </summary>
            public const string LifecycleEvents = Prefix + "LIFECYCLE_EVENTS";
            /// <summary>
            /// Gets the environment variable used to configure the runner's namespace
            /// </summary>
            public const string Namespace = Prefix + "NAMESPACE";
            /// <summary>
            /// Gets the environment variable used to configure the runner's name
            /// </summary>
            public const string Name = Prefix + "NAME";

        }

        /// <summary>
        /// Exposes constants about runtime-related environment variables
        /// </summary>
        public static class Runtime
        {

            /// <summary>
            /// Gets the prefix for all runtime related environment variables
            /// </summary>
            public const string Prefix = EnvironmentVariables.Prefix + "RUNTIME_";

            /// <summary>
            /// Gets the environment variable used to configure the runtime mode
            /// </summary>
            public const string Mode = Prefix + "MODE";

            /// <summary>
            /// Exposes constants about Docker runtime-related environment variables
            /// </summary>
            public static class Docker
            {

                /// <summary>
                /// Gets the prefix for all Docker runtime related environment variables
                /// </summary>
                public const string Prefix = Runtime.Prefix + "DOCKER_";

                /// <summary>
                /// Gets the environment variable used to specify the YAML file used to configure the Docker runner container
                /// </summary>
                public const string Container = Prefix + "CONTAINER";
                /// <summary>
                /// Gets the environment variable used to configure the network runner containers should be connected to
                /// </summary>
                public const string Network = Prefix + "NETWORK";

                /// <summary>
                /// Exposes constants about environment variables used to configure the API of a Docker runtime
                /// </summary>
                public static class Api
                {

                    /// <summary>
                    /// Gets the prefix for all Docker runtime API related environment variables
                    /// </summary>
                    public const string Prefix = Docker.Prefix + "API_";

                    /// <summary>
                    /// Gets the environment variable used to configure the endpoint of the Docker API to use
                    /// </summary>
                    public const string Endpoint = Prefix + "ENDPOINT";
                    /// <summary>
                    /// Gets the environment variable used to configure the version of the Docker API to use
                    /// </summary>
                    public const string Version = Prefix + "VERSION";

                }

                /// <summary>
                /// Exposes constants about environment variables used to configure the runner images of a Docker runtime
                /// </summary>
                public static class Image
                {

                    /// <summary>
                    /// Gets the prefix for all Docker runtime image related environment variables
                    /// </summary>
                    public const string Prefix = Docker.Prefix + "IMAGE_";

                    /// <summary>
                    /// Gets the environment variable used to configure the image registry to use when pulling runner images
                    /// </summary>
                    public const string Registry = Prefix + "REGISTRY";
                    /// <summary>
                    /// Gets the environment variable used to configure the policy to use when pulling runner images
                    /// </summary>
                    public const string PullPolicy = Prefix + "PULL_POLICY";

                }

                /// <summary>
                /// Exposes constants about environment variables used to configure the secrets used by a Docker runtime
                /// </summary>
                public static class Secrets
                {

                    /// <summary>
                    /// Gets the prefix for all Docker runtime secrets related environment variables
                    /// </summary>
                    public const string Prefix = Docker.Prefix + "SECRETS_";

                    /// <summary>
                    /// Gets the environment variable used to configure the directory that contains the secrets to mount onto runner containers
                    /// </summary>
                    public const string Directory = Prefix + "DIRECTORY";
                    /// <summary>
                    /// Gets the environment variable used to configure the directory to mount the secrets volume to
                    /// </summary>
                    public const string MountPath = Prefix + "MOUNT_PATH";

                }

            }

            /// <summary>
            /// Exposes constants about Kubernetes runtime-related environment variables
            /// </summary>
            public static class Kubernetes
            {

                /// <summary>
                /// Gets the prefix for all Kubernetes runtime related environment variables
                /// </summary>
                public const string Prefix = Runtime.Prefix + "K8S_";

                /// <summary>
                /// Gets the environment variable used to configure the path to the Kubeconfig file to use
                /// </summary>
                public const string Kubeconfig = Prefix + "KUBECONFIG";
                /// <summary>
                /// Gets the environment variable used to specify the YAML file used to configure the Kubernetes runner pod
                /// </summary>
                public const string Pod = Prefix + "POD";
                /// <summary>
                /// Gets the environment variable used to configure the namespace to create runner pods into
                /// </summary>
                public const string Namespace = Prefix + "NAMESPACE";
                /// <summary>
                /// Gets the environment variable used to configure the name of the service account that grants the runner the ability to spawn containers when its container platform has been set to `kubernetes`
                /// </summary>
                public const string ServiceAccount = Prefix + "SERVICE_ACCOUNT";

                /// <summary>
                /// Exposes constants about environment variables used to configure the secrets used by a Docker runtime
                /// </summary>
                public static class Secrets
                {

                    /// <summary>
                    /// Gets the prefix for all Kubernetes runtime secrets related environment variables
                    /// </summary>
                    public const string Prefix = Kubernetes.Prefix + "SECRETS_";

                    /// <summary>
                    /// Gets the environment variable used to configure the name of the volume onto which to mount secrets
                    /// </summary>
                    public const string VolumeName = Prefix + "VOLUME_NAME";
                    /// <summary>
                    /// Gets the environment variable used to configure the directory to mount the secrets volume to
                    /// </summary>
                    public const string MountPath = Prefix + "MOUNT_PATH";

                }

            }

            /// <summary>
            /// Exposes constants about Native runtime-related environment variables
            /// </summary>
            public static class Native
            {

                /// <summary>
                /// Gets the prefix for all native runtime related environment variables
                /// </summary>
                public const string Prefix = Runtime.Prefix + "NATIVE_";

                /// <summary>
                /// Gets the environment variable used to configure the working directory that contains the runner binaries
                /// </summary>
                public const string Directory = Prefix + "DIRECTORY";
                /// <summary>
                /// Gets the environment variable used to configure the path to the runner's executable file
                /// </summary>
                public const string Executable = Prefix + "EXECUTABLE";
                /// <summary>
                /// Gets the environment variable used to configure the directory that contains the secrets made available to runners
                /// </summary>
                public const string SecretsDirectory = Prefix + "SECRETS_DIRECTORY";

            }

        }

        /// <summary>
        /// Exposes constants about secrets-related environment variables
        /// </summary>
        public static class Secrets
        {

            /// <summary>
            /// Gets the prefix for all secrets related environment variables
            /// </summary>
            public const string Prefix = EnvironmentVariables.Prefix + "SECRETS_";
            /// <summary>
            /// Gets the name of the environment variable used to configure the path to the directory that contains secrets files
            /// </summary>
            public const string Directory = Prefix + "DIRECTORY";

        }

        /// <summary>
        /// Exposes constants about service account related environment variables
        /// </summary>
        public static class ServiceAccount
        {

            /// <summary>
            /// Gets the prefix for all operator related environment variables
            /// </summary>
            public const string Prefix = EnvironmentVariables.Prefix + "SERVICEACCOUNT_";

            /// <summary>
            /// Gets the environment variable used to configure the service account name of a Synapse Runner
            /// </summary>
            public const string Name = Prefix + "NAME";
            /// <summary>
            /// Gets the environment variable used to configure the key of a Synapse Runner's service account
            /// </summary>
            public const string Key = Prefix + "KEY";

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
    /// Exposes constants about Synapse containers
    /// </summary>
    public static class Containers
    {

        /// <summary>
        /// Exposes constants about Synapse container images
        /// </summary>
        public static class Images
        {

            static string? _version;

            /// <summary>
            /// Gets the name of the Synapse container image registry
            /// </summary>
            public const string ImageRegistry = "ghcr.io/serverlessworkflow/synapse";
            /// <summary>
            /// Gets the current version of Synapse container images
            /// </summary>
            public static string Version
            {
                get
                {
                    if (!string.IsNullOrWhiteSpace(_version)) return _version;
                    _version = typeof(SynapseDefaults).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "latest";
                    if (_version.EndsWith('-')) _version = _version[..^1];
                    if (_version.Contains('+')) _version = _version[.._version.IndexOf('+')];
                    return _version;
                }
            }
            /// <summary>
            /// Gets the name of the Synapse API container image
            /// </summary>
            public static readonly string Api = $"{ImageRegistry}/api:{Version}";
            /// <summary>
            /// Gets the name of the Synapse Correlator container image
            /// </summary>
            public static readonly string Correlator = $"{ImageRegistry}/correlator:{Version}";
            /// <summary>
            /// Gets the name of the Synapse Operator container image
            /// </summary>
            public static readonly string Operator = $"{ImageRegistry}/operator:{Version}";
            /// <summary>
            /// Gets the name of the Synapse Runner container image
            /// </summary>
            public static readonly string Runner = $"{ImageRegistry}/runner:{Version}";

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
            /// <summary>
            /// Gets the label used by Synapse to indicate the qualified name of the workflow instance that owns a concept, such as a correlation
            /// </summary>
            public const string WorkflowInstance = Prefix + "workflow-instance";

        }

    }

    /// <summary>
    /// Exposes constants about Synapse tasks
    /// </summary>
    public static class Tasks
    {

        /// <summary>
        /// Exposes Synapse task metadata properties
        /// </summary>
        public static class Metadata
        {

            /// <summary>
            /// Exposes constants about the extension property used to determine whether or not to prefix the path of child tasks with the task's keyword (ex: `do`)
            /// </summary>
            public static class PathPrefix
            {

                /// <summary>
                /// Gets the name of the 'PathPrefix' property
                /// </summary>
                public const string Name = "PathPrefix";
                /// <summary>
                /// Gets the type of the 'PathPrefix' property
                /// </summary>
                public static readonly Type Type = typeof(bool);

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