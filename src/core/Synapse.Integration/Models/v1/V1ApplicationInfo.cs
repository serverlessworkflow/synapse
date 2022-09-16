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

namespace Synapse.Integration.Models
{

    /// <summary>
    /// Describes the running Synapse instance
    /// </summary>
    [DataContract]
    public class V1ApplicationInfo
    {

        /// <summary>
        /// Initializes a new <see cref="V1ApplicationInfo"/>
        /// </summary>
        protected V1ApplicationInfo()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1ApplicationInfo"/>
        /// </summary>
        /// <param name="name">The name of the running Synapse application</param>
        /// <param name="version">The version of the running Synapse application</param>
        /// <param name="osDescription">The description of the OS the Synapse application is running on</param>
        /// <param name="frameworkDescription">The description of the .NET Framework the Synapse application is running on</param>
        /// <param name="serverlessWorkflowSdkVersion">The version of the Serverless Workflow SDK used by the Synapse application</param>
        /// <param name="environmentName">The name of the Synapse application's environment</param>
        /// <param name="workflowRuntime">The name of the Synapse workflow runtime</param>
        /// <param name="supportedRuntimeExpressionLanguages">A list containing all supported runtime expression languages</param>
        /// <param name="environmentVariables">An <see cref="IDictionary{TKey, TValue}"/> containing the Synapse application's environment variables</param>
        /// <param name="plugins">A collection containing the metadata of all installed plugins</param>
        public V1ApplicationInfo(string name, string version, string osDescription, string frameworkDescription, string serverlessWorkflowSdkVersion, string environmentName, 
            string workflowRuntime, IEnumerable<string> supportedRuntimeExpressionLanguages, IDictionary<string, string> environmentVariables, IEnumerable<V1PluginInfo> plugins)
        {
            this.Name = name;
            this.Version = version;
            this.OSDescription = osDescription;
            this.FrameworkDescription = frameworkDescription;
            this.ServerlessWorkflowSdkVersion = serverlessWorkflowSdkVersion;
            this.EnvironmentName = environmentName;
            this.WorkflowRuntimeName = workflowRuntime;
            this.SupportedRuntimeExpressionLanguages = supportedRuntimeExpressionLanguages.ToList();
            this.EnvironmentVariables = environmentVariables;
            this.Plugins = plugins.ToList();
        }

        /// <summary>
        /// Gets the Synapse application's name
        /// </summary>
        [DataMember(Order = 1)]
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets the version of Synapse 
        /// </summary>
        [DataMember(Order = 2)]
        public virtual string Version { get; set; }

        /// <summary>
        /// Gets the description of the OS the Synapse application is running on
        /// </summary>
        [DataMember(Order = 3)]
        public virtual string OSDescription { get; set; }

        /// <summary>
        /// Gets the description of the .NET Framework the Synapse application is running on
        /// </summary>
        [DataMember(Order = 4)]
        public virtual string FrameworkDescription { get; set; }

        /// <summary>
        /// Gets the version of the Serverless Workflow SDK used by the Synapse application
        /// </summary>
        [DataMember(Order = 5)]
        public virtual string ServerlessWorkflowSdkVersion { get; set; }

        /// <summary>
        /// Gets the name of the Synapse application's environment
        /// </summary>
        [DataMember(Order = 6)]
        public virtual string EnvironmentName { get; set; }

        /// <summary>
        /// Gets the name of the Synapse workflow runtime
        /// </summary>
        [DataMember(Order = 7)]
        public virtual string WorkflowRuntimeName { get; set; }

        /// <summary>
        /// Gets a list containing all supported runtime expression languages
        /// </summary>
        [DataMember(Order = 8)]
        public virtual List<string> SupportedRuntimeExpressionLanguages { get; set; }

        /// <summary>
        /// Gets the Synapse application's environment variables
        /// </summary>
        [DataMember(Order = 9)]
        public virtual IDictionary<string, string> EnvironmentVariables { get; set; }

        /// <summary>
        /// Gets a collection containing installed plugins
        /// </summary>
        [DataMember(Order = 10)]
        public virtual ICollection<V1PluginInfo> Plugins { get; set; }

    }

}
