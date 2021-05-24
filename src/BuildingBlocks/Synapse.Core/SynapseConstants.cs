using CloudNative.CloudEvents;
using System;
using System.Linq;
using System.Reflection;

namespace Synapse
{

    /// <summary>
    /// Exposes constants about Synapse
    /// </summary>
    public static class SynapseConstants
    {

        /// <summary>
        /// Gets Synapse's resource namespace
        /// </summary>
        public const string Namespace = "synapse.io";
        /// <summary>
        /// Gets Synapse's version
        /// </summary>
        public static string Version = typeof(SynapseConstants).Assembly.GetCustomAttributes().OfType<AssemblyInformationalVersionAttribute>().First().InformationalVersion;

        /// <summary>
        /// Exposes constants about Synapse environment variables
        /// </summary>
        public static class EnvironmentVariables
        {

            /// <summary>
            /// Gets the prefix for all Synapse environment variables
            /// </summary>
            public const string Prefix = "SYNAPSE_";

            /// <summary>
            /// Exposes constants about <see cref="CloudEvent"/>-related environment variables
            /// </summary>
            public static class CloudEvents
            {

                /// <summary>
                /// Gets the prefix for all <see cref="CloudEvent"/>-related environment variables
                /// </summary>
                public const string Prefix = EnvironmentVariables.Prefix + "CLOUDEVENT_";

                /// <summary>
                /// Exposes constants about the <see cref="CloudEvent"/> sink environment variable
                /// </summary>
                public static class Sink
                {

                    /// <summary>
                    /// Gets the name of the <see cref="CloudEvent"/> sink environment variable
                    /// </summary>
                    public const string Name = Prefix + "SINK";

                    /// <summary>
                    /// Gets the value of the <see cref="CloudEvent"/> sink environment variable
                    /// </summary>
                    public static string Value = Environment.GetEnvironmentVariable(Name);

                }

            }

            /// <summary>
            /// Exposes constants about Kuberneres environment variables
            /// </summary>
            public static class Kubernetes
            {

                /// <summary>
                /// Gets the prefix for all Kubernetes environment variables
                /// </summary>
                public const string Prefix = "KUBERNETES_";

                /// <summary>
                /// Exposes constants about the Kubernetes namespace environment variable
                /// </summary>
                public static class Namespace
                {

                    /// <summary>
                    /// Gets the name of the Kubernetes namespace environment variable
                    /// </summary>
                    public const string Name = Prefix + "NAMESPACE";

                    /// <summary>
                    /// Gets the value of the Kubernetes namespace environment variable
                    /// </summary>
                    public static string Value = Environment.GetEnvironmentVariable(Name);

                }

                /// <summary>
                /// Exposes constants about the Kubernetes pod name environment variable
                /// </summary>
                public static class PodName
                {

                    /// <summary>
                    /// Gets the name of the Kubernetes pod name environment variable
                    /// </summary>
                    public const string Name = Prefix + "POD_NAME";

                    /// <summary>
                    /// Gets the value of the Kubernetes pod name environment variable
                    /// </summary>
                    public static string Value = Environment.GetEnvironmentVariable(Name);

                }

            }

            /// <summary>
            /// Exposes constants about Synapse runtime-related environment variables
            /// </summary>
            public static class Runtime
            {

                /// <summary>
                /// Gets the prefix for all Synapse runtime-related environment variables
                /// </summary>
                public const string Prefix = EnvironmentVariables.Prefix + "RUNTIME_";

                /// <summary>
                /// Exposes constants about Synapse correlation-related environment variables
                /// </summary>
                public static class Correlation
                {

                    /// <summary>
                    /// Gets the prefix for all Synapse correlation-related environment variables
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
                    /// Exposes constants about the Synapse runtime correlation max duration environment variable
                    /// </summary>
                    public static class MaxDuration
                    {

                        /// <summary>
                        /// Gets the name of the Synapse runtime correlation max duration environment variable
                        /// </summary>
                        public const string Name = Prefix + "MAXDURATION";

                        /// <summary>
                        /// Gets the value of the Synapse runtime correlation max duration environment variable
                        /// </summary>
                        public static string Value = Environment.GetEnvironmentVariable(Name);

                    }

                }

                /// <summary>
                /// Exposes constants about the Synapse runtime grace period environment variable
                /// </summary>
                public static class GracePeriod
                {

                    /// <summary>
                    /// Gets the name of the Synapse runtime grace period environment variable
                    /// </summary>
                    public const string Name = Prefix + "GRACEPERIOD";

                    /// <summary>
                    /// Gets the value of the Synapse runtime grace period environment variable
                    /// </summary>
                    public static string Value = Environment.GetEnvironmentVariable(Name);

                }

                /// <summary>
                /// Exposes constants about the Synapse runtime startup mode environment variable
                /// </summary>
                public static class Startup
                {

                    /// <summary>
                    /// Gets the name of the Synapse runtime startup mode environment variable
                    /// </summary>
                    public const string Name = Prefix + "STARTUP";

                    /// <summary>
                    /// Gets the value of the Synapse runtime startup mode environment variable
                    /// </summary>
                    public static string Value = Environment.GetEnvironmentVariable(Name);

                }

            }

            /// <summary>
            /// Exposes constants about Synapse workflow-related environment variables
            /// </summary>
            public static class Workflows
            {

                /// <summary>
                /// Gets the prefix for all Synapse workflow-related environment variables
                /// </summary>
                public const string Prefix = EnvironmentVariables.Prefix + "WORKFLOW_";

                /// <summary>
                /// Exposes constants about the Synapse workflow id environment variable
                /// </summary>
                public static class Id
                {

                    /// <summary>
                    /// Gets the name of the Synapse workflow id environment variable
                    /// </summary>
                    public const string Name = Prefix + "ID";

                    /// <summary>
                    /// Gets the value of the Synapse workflow version environment variable
                    /// </summary>
                    public static string Value = Environment.GetEnvironmentVariable(Name);

                }

                /// <summary>
                /// Exposes constants about the Synapse workflow version environment variable
                /// </summary>
                public static class Version
                {

                    /// <summary>
                    /// Gets the name of the Synapse workflow version environment variable
                    /// </summary>
                    public const string Name = Prefix + "VERSION";

                    /// <summary>
                    /// Gets the value of the Synapse workflow version environment variable
                    /// </summary>
                    public static string Value = Environment.GetEnvironmentVariable(Name);

                }

                /// <summary>
                /// Exposes constants about the Synapse workflow instance environment variable
                /// </summary>
                public static class Instance
                {

                    /// <summary>
                    /// Gets the name of the Synapse workflow instance environment variable
                    /// </summary>
                    public const string Name = Prefix + "INSTANCE";

                    /// <summary>
                    /// Gets the value of the Synapse workflow instance environment variable
                    /// </summary>
                    public static string Value = Environment.GetEnvironmentVariable(Name);

                }

            }

        }

        /// <summary>
        /// Exposes constants about Synapse's annotations
        /// </summary>
        public static class Labels
        {

            /// <summary>
            /// Gets the prefix for all Synapse labels
            /// </summary>
            public const string Prefix = Namespace + "/";

            /// <summary>
            /// Gets the name of the Synapse label indicating that the marked resource has been scheduled and should be ignored by the operator
            /// </summary>
            public const string Scheduled = "scheduled";

            /// <summary>
            /// Exposes constants about Synapse workflow-related labels
            /// </summary>
            public static class Workflows
            {

                /// <summary>
                /// Gets the prefix for all Synapse workflow-related labels
                /// </summary>
                public const string Prefix = "workflows." + Labels.Prefix;

                /// <summary>
                /// Gets the name of the Synapse workflow id label
                /// </summary>
                public const string Id = Prefix + "id";

                /// <summary>
                /// Gets the name of the Synapse workflow version label
                /// </summary>
                public const string Version = Prefix + "version";

            }

        }

        /// <summary>
        /// Exposes constants about Synapse's logging
        /// </summary>
        public static class Logging
        {

            /// <summary>
            /// Gets the header of all Synapse products logs
            /// </summary>
            public const string Header = @"
                                      ___                       ___           ___           ___         ___           ___     
                                     /  /\          ___        /__/\         /  /\         /  /\       /  /\         /  /\    
                                    /  /:/_        /__/|       \  \:\       /  /::\       /  /::\     /  /:/_       /  /:/_   
                                   /  /:/ /\      |  |:|        \  \:\     /  /:/\:\     /  /:/\:\   /  /:/ /\     /  /:/ /\  
                                  /  /:/ /::\     |  |:|    _____\__\:\   /  /:/~/::\   /  /:/~/:/  /  /:/ /::\   /  /:/ /:/_ 
                                 /__/:/ /:/\:\  __|__|:|   /__/::::::::\ /__/:/ /:/\:\ /__/:/ /:/  /__/:/ /:/\:\ /__/:/ /:/ /\
                                 \  \:\/:/~/:/ /__/::::\   \  \:\~~\~~\/ \  \:\/:/__\/ \  \:\/:/   \  \:\/:/~/:/ \  \:\/:/ /:/
                                  \  \::/ /:/     ~\~~\:\   \  \:\  ~~~   \  \::/       \  \::/     \  \::/ /:/   \  \::/ /:/ 
                                   \__\/ /:/        \  \:\   \  \:\        \  \:\        \  \:\      \__\/ /:/     \  \:\/:/  
                                     /__/:/          \__\/    \  \:\        \  \:\        \  \:\       /__/:/       \  \::/   
                                     \__\/                     \__\/         \__\/         \__\/       \__\/         \__\/    
                                                                                                                    
 _______                               __                         ________              __     ___ __                      ______               __   __                 
|     __|.-----.----.--.--.-----.----.|  |.-----.-----.-----.    |  |  |  |.-----.----.|  |--.'  _|  |.-----.--.--.--.    |   __ \.--.--.-----.|  |_|__|.--------.-----.
|__     ||  -__|   _|  |  |  -__|   _||  ||  -__|__ --|__ --|    |  |  |  ||  _  |   _||    <|   _|  ||  _  |  |  |  |    |      <|  |  |     ||   _|  ||        |  -__|
|_______||_____|__|  \___/|_____|__|  |__||_____|_____|_____|    |________||_____|__|  |__|__|__| |__||_____|________|    |___|__||_____|__|__||____|__||__|__|__|_____|
                                                                                                                                                                                                                                                                                                                                
";

        }

        /// <summary>
        /// Exposes constants about Synapse's K8s Custom Resources
        /// </summary>
        public static class Resources
        {

            /// <summary>
            /// Gets the default group for all Synapse resources
            /// </summary>
            public const string Group = Namespace;

            /// <summary>
            /// Gets the default version for all Synapse resources
            /// </summary>
            public const string Version = "v1alpha1";

            /// <summary>
            /// Gets the default api version for all Synapse resources
            /// </summary>
            public static string ApiVersion = string.Join("/", Group, Version);

        }

        /// <summary>
        /// Exposes constants about Synapse git repository
        /// </summary>
        public static class GitRepository
        {

            /// <summary>
            /// Gets the uri of Synapse's git repository 
            /// </summary>
            public static Uri Uri = new($"https://raw.githubusercontent.com/serverlessworkflow/synapse/{Version}/");

        }

    }

}
