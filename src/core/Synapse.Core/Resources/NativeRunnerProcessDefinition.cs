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
/// Represents the definition of a native workflow runner process
/// </summary>
[DataContract]
public record NativeRunnerProcessDefinition
{

    /// <summary>
    /// Gets/sets the path to the file to execute to run a workflow instance
    /// </summary>
    [DataMember(Order = 1, Name = "executable"), JsonPropertyOrder(1), JsonPropertyName("executable"), YamlMember(Order = 1, Alias = "executable")]
    public virtual string Executable { get; set; } = Path.Combine(AppContext.BaseDirectory, "bin", "runner", "Synapse.Runner");

    /// <summary>
    /// Gets/sets the working directory
    /// </summary>
    [DataMember(Order = 2, Name = "directory"), JsonPropertyOrder(2), JsonPropertyName("directory"), YamlMember(Order = 2, Alias = "directory")]
    public virtual string Directory { get; set; } = Path.Combine(AppContext.BaseDirectory, "bin", "runner");

}
