using System;

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
        /// Exposes constants about Synapse environment variables
        /// </summary>
        public static class EnvironmentVariables
        {

            /// <summary>
            /// Gets the prefix for all Synapse environment variables
            /// </summary>
            public const string Prefix = "SYNAPSE_";

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
            /// Exposes constants about Synapse workflow-related labels
            /// </summary>
            public static class Workflows
            {

                /// <summary>
                /// Gets the prefix for all Synapse workflow-related labels
                /// </summary>
                public const string Prefix = "workflows." + Namespace + "/";

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

    }

}
