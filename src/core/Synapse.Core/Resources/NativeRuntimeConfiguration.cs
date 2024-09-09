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

namespace Synapse.Resources;

/// <summary>
/// Represents an object used to configure a native runtime
/// </summary>
[DataContract]
public record NativeRuntimeConfiguration
{

    /// <summary>
    /// Gets the default path to the directory that contains the runner binaries
    /// </summary>
    public static readonly string DefaultDirectory = Path.Combine(AppContext.BaseDirectory, "bin", "runner");
    /// <summary>
    /// Gets the default path to the runner executable file
    /// </summary>
    public const string DefaultExecutable = "Synapse.Runner";

    /// <summary>
    /// Initializes a new <see cref="NativeRuntimeConfiguration"/>
    /// </summary>
    public NativeRuntimeConfiguration()
    {
        var env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runtime.Native.Directory);
        if (!string.IsNullOrWhiteSpace(env))
        {
            if (!System.IO.Directory.Exists(env)) throw new FileNotFoundException("The runner directory does not exist or cannot be found", env);
            this.Directory = env;
        }
        env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runtime.Native.Executable);
        if (!string.IsNullOrWhiteSpace(env))
        {
            var filePath = Path.Combine(this.Directory, env);
            if (!File.Exists(filePath)) throw new FileNotFoundException("The runner executable file does not exist or cannot be found", filePath);
            this.Executable = env;
        }
    }

    /// <summary>
    /// Gets/sets the runner's working directory
    /// </summary>
    [DataMember(Order = 1, Name = "directory"), JsonPropertyOrder(1), JsonPropertyName("directory"), YamlMember(Order = 1, Alias = "directory")]
    public virtual string Directory { get; set; } = DefaultDirectory;

    /// <summary>
    /// Gets/sets the path to the file to execute to run a workflow instance
    /// </summary>
    [DataMember(Order = 2, Name = "executable"), JsonPropertyOrder(2), JsonPropertyName("executable"), YamlMember(Order = 2, Alias = "executable")]
    public virtual string Executable { get; set; } = DefaultExecutable;


}
