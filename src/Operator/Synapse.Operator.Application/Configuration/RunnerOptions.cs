using System;
using System.IO;

namespace Synapse.Operator.Application.Configuration
{
    /// <summary>
    /// Represents the options used to configure a Synapse Runner
    /// </summary>
    public class RunnerOptions
    {

        /// <summary>
        /// Gets the default Runner deployment file path
        /// </summary>
        public static string DefaultDeploymentFilePath = Path.Combine(AppContext.BaseDirectory, "Assets", "Charts", "runner.yaml");
        /// <summary>
        /// Gets the default namespace for Synapse Runner deployments
        /// </summary>
        public const string DefaultNamespace = "default";

        /// <summary>
        /// Gets the Runner's deployment file path
        /// </summary>
        public string DeploymentFilePath { get; set; } = DefaultDeploymentFilePath;

        /// <summary>
        /// Gets the namespace for Synapse Runner deployments
        /// </summary>
        public string Namespace { get; set; } = DefaultNamespace;

    }

}
