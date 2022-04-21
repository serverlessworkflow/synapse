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

using CloudNative.CloudEvents;

namespace Synapse
{

    /// <summary>
    /// Exposes the environment variables used by the application
    /// </summary>
    public static class EnvironmentVariables
    {

        /// <summary>
        /// Gets the prefix for all Synapse environment variables
        /// </summary>
        public const string Prefix = "SYNAPSE_";

        /// <summary>
        /// Exposes constants about the environment variable used to skip certificate validation
        /// </summary>
        public static class SkipCertificateValidation
        {

            /// <summary>
            /// Gets the name of the environment variable used to skip certificate validation
            /// </summary>
            public const string Name = Prefix + "SKIP_CERTIFICATE_VALIDATION";

            /// <summary>
            /// Gets the value of the environment variable used to skip certificate validation
            /// </summary>
            public static string Value = Environment.GetEnvironmentVariable(Name);

        }

        /// <summary>
        /// Exposes constants about api-related environment variables
        /// </summary>
        public static class Api
        {

            /// <summary>
            /// Gets the prefix for all api related environment variables
            /// </summary>
            public const string Prefix = EnvironmentVariables.Prefix + "API_";

            /// <summary>
            /// Exposes constants about the api host name environment variable
            /// </summary>
            public static class HostName
            {

                /// <summary>
                /// Gets the name of the api host environment variable
                /// </summary>
                public const string Name = Prefix + "HOSTNAME";

                /// <summary>
                /// Gets the value of the api host environment variable
                /// </summary>
                public static string Value = Environment.GetEnvironmentVariable(Name);

            }

            /// <summary>
            /// Exposes constants about HTTP api-related environment variables
            /// </summary>
            public static class Http
            {

                /// <summary>
                /// Gets the prefix for all HTTP api related environment variables
                /// </summary>
                public const string Prefix = EnvironmentVariables.Prefix + "HTTP_";

                /// <summary>
                /// Exposes constants about the HTTP api scheme environment variable
                /// </summary>
                public static class Scheme
                {

                    /// <summary>
                    /// Gets the name of the HTTP api scheme environment variable
                    /// </summary>
                    public const string Name = Prefix + "SCHEME";

                    /// <summary>
                    /// Gets the value of the HTTP api scheme environment variable
                    /// </summary>
                    public static string Value = Environment.GetEnvironmentVariable(Name);

                }

                /// <summary>
                /// Exposes constants about the HTTP api port environment variable
                /// </summary>
                public static class Port
                {

                    /// <summary>
                    /// Gets the name of the HTTP api port environment variable
                    /// </summary>
                    public const string Name = Prefix + "PORT";

                    /// <summary>
                    /// Gets the value of the HTTP api port environment variable
                    /// </summary>
                    public static string Value = Environment.GetEnvironmentVariable(Name);

                }

            }

            /// <summary>
            /// Exposes constants about GRPC api-related environment variables
            /// </summary>
            public static class Grpc
            {

                /// <summary>
                /// Gets the prefix for all GRPC api related environment variables
                /// </summary>
                public const string Prefix = EnvironmentVariables.Prefix + "GRPC_";

                /// <summary>
                /// Exposes constants about the GRPC api scheme environment variable
                /// </summary>
                public static class Scheme
                {

                    /// <summary>
                    /// Gets the name of the GRPC api scheme environment variable
                    /// </summary>
                    public const string Name = Prefix + "SCHEME";

                    /// <summary>
                    /// Gets the value of the GRPC api scheme environment variable
                    /// </summary>
                    public static string Value = Environment.GetEnvironmentVariable(Name);

                }

                /// <summary>
                /// Exposes constants about the GRPC api port environment variable
                /// </summary>
                public static class Port
                {

                    /// <summary>
                    /// Gets the name of the GRPC api port environment variable
                    /// </summary>
                    public const string Name = Prefix + "PORT";

                    /// <summary>
                    /// Gets the value of the GRPC api port environment variable
                    /// </summary>
                    public static string Value = Environment.GetEnvironmentVariable(Name);

                }

            }

        }

        /// <summary>
        /// Exposes constants about <see cref="CloudEvent"/>-related environment variables
        /// </summary>
        public static class CloudEvents
        {

            /// <summary>
            /// Gets the prefix for all <see cref="CloudEvent"/>-related environment variables
            /// </summary>
            public const string Prefix = EnvironmentVariables.Prefix + "CLOUDEVENTS_";

            /// <summary>
            /// Exposes constants about <see cref="CloudEvent"/> sink related environment variables
            /// </summary>
            public static class Sink
            {

                /// <summary>
                /// Gets the prefix for all <see cref="CloudEvent"/> sink related environment variables
                /// </summary>
                public const string Prefix = CloudEvents.Prefix + "SINK_";

                /// <summary>
                /// Exposes constants about the <see cref="CloudEvent"/> sink uri environment variable
                /// </summary>
                public static class Uri
                {

                    /// <summary>
                    /// Gets the name of the <see cref="CloudEvent"/> sink uri environment variable
                    /// </summary>
                    public const string Name = Prefix + "URI";

                    /// <summary>
                    /// Gets the value of the <see cref="CloudEvent"/> sink uri environment variable
                    /// </summary>
                    public static string Value = Environment.GetEnvironmentVariable(Name);

                }

            }

        }

        /// <summary>
        /// Exposes constants about persistence-related environment variables
        /// </summary>
        public static class Persistence
        {

            /// <summary>
            /// Gets the prefix for all persistence related environment variables
            /// </summary>
            public const string Prefix = EnvironmentVariables.Prefix + "PERSISTENCE_";

            /// <summary>
            /// Exposes constants about write model-related environment variables
            /// </summary>
            public static class WriteModel
            {

                /// <summary>
                /// Gets the prefix for all write model related environment variables
                /// </summary>
                public const string Prefix = Persistence.Prefix + "WRITEMODEL_";

                /// <summary>
                /// Exposes constants about the default write model repository environment variable
                /// </summary>
                public static class DefaultRepository
                {

                    /// <summary>
                    /// Gets the name of the default write model repository environment variable
                    /// </summary>
                    public const string Name = Prefix + "DEFAULT_REPOSITORY";

                    /// <summary>
                    /// Gets the value of the default write model repository environment variable
                    /// </summary>
                    public static string Value = Environment.GetEnvironmentVariable(Name);

                }

            }

            /// <summary>
            /// Exposes constants about read model-related environment variables
            /// </summary>
            public static class ReadModel
            {

                /// <summary>
                /// Gets the prefix for all read model related environment variables
                /// </summary>
                public const string Prefix = Persistence.Prefix + "READMODEL_";

                /// <summary>
                /// Exposes constants about the default read model repository environment variable
                /// </summary>
                public static class DefaultRepository
                {

                    /// <summary>
                    /// Gets the name of the default read model repository environment variable
                    /// </summary>
                    public const string Name = Prefix + "DEFAULT_REPOSITORY";

                    /// <summary>
                    /// Gets the value of the default read model repository environment variable
                    /// </summary>
                    public static string Value = Environment.GetEnvironmentVariable(Name);

                }

            }

        }

        /// <summary>
        /// Exposes constants about plugins-related environment variables
        /// </summary>
        public static class Plugins
        {

            /// <summary>
            /// Gets the prefix for all plugins related environment variables
            /// </summary>
            public const string Prefix = EnvironmentVariables.Prefix + "PLUGINS_";

            /// <summary>
            /// Exposes constants about the plugins directory environment variable
            /// </summary>
            public static class Directory
            {

                /// <summary>
                /// Gets the name of the plugins directory environment variable
                /// </summary>
                public const string Name = Prefix + "DIRECTORY";

                /// <summary>
                /// Gets the value of the plugins directory environment variable
                /// </summary>
                public static string Value = Environment.GetEnvironmentVariable(Name);

            }

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
            /// Exposes constants about the workflow instance id environment variable
            /// </summary>
            public static class WorkflowInstanceId
            {

                /// <summary>
                /// Gets the name of the workflow instance id environment variable
                /// </summary>
                public const string Name = Prefix + "WORKFLOW_INSTANCE_ID";

                /// <summary>
                /// Gets the value of the workflow instance id environment variable
                /// </summary>
                public static string Value = Environment.GetEnvironmentVariable(Name);

            }

            /// <summary>
            /// Exposes constants about correlation related environment variables
            /// </summary>
            public static class Correlation
            {

                /// <summary>
                /// Gets the prefix for all correlation related environment variables
                /// </summary>
                public const string Prefix = Runtime.Prefix + "CORRELATION_";

                /// <summary>
                /// Exposes constants about the Synapse runtime correlation mode environment variable
                /// </summary>
                public static class Mode
                {

                    /// <summary>
                    /// Gets the name of the Synapse runtime correlation mode environment variable
                    /// </summary>
                    public const string Name = Prefix + "MODE";

                    /// <summary>
                    /// Gets the value of the Synapse runtime correlation mode environment variable
                    /// </summary>
                    public static string Value = Environment.GetEnvironmentVariable(Name);

                }

                /// <summary>
                /// Exposes constants about the environment variable that regulates the maximum amount of time an active correlation can be running before going to sleep
                /// </summary>
                public static class MaxActiveDuration
                {

                    /// <summary>
                    /// Gets the name of the workflow instance id environment variable
                    /// </summary>
                    public const string Name = Prefix + "MAX_ACTIVE_DURATION";

                    /// <summary>
                    /// Gets the value of the workflow instance id environment variable
                    /// </summary>
                    public static string Value = Environment.GetEnvironmentVariable(Name);

                }

            }

        }

    }

}
