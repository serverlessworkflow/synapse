using System;
using System.IO;

namespace Synapse.Operator.Application.Configuration
{

    /// <summary>
    /// Represents the options used to configure the Synapse Operator
    /// </summary>
    public class ApplicationOptions
    {

        public RunnerOptions Runner { get; set; } = new RunnerOptions();

    }

    public class RunnerOptions
    {

        public static string DefaultDeploymentFilePath = Path.Combine(AppContext.BaseDirectory, "Assets", "Charts", "runner.yaml");
        public const string DefaultNamespace = "default";

        public string DeploymentFilePath { get; set; } = DefaultDeploymentFilePath;

        public string Namespace { get; set; } = DefaultNamespace;

    }

}
