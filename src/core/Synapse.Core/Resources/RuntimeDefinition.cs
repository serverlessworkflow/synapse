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

using ServerlessWorkflow.Sdk.Models.Processes;

namespace Synapse.Resources;

/// <summary>
/// Represents an object used to configure the runtime used to spawn workflow instance processes
/// </summary>
[DataContract]
public record RuntimeDefinition
{

    /// <summary>
    /// Gets/sets the options used to configure the runtime, when using the native mode
    /// </summary>
    [DataMember(Order = 1, Name = "native"), JsonPropertyOrder(1), JsonPropertyName("native"), YamlMember(Order = 1, Alias = "native")]
    public virtual NativeRunnerProcessDefinition? Native { get; set; }

    /// <summary>
    /// Gets/sets the options used to configure the runtime, when using the container mode
    /// </summary>
    [DataMember(Order = 2, Name = "container"), JsonPropertyOrder(2), JsonPropertyName("container"), YamlMember(Order = 2, Alias = "container")]
    public virtual ContainerProcessDefinition? Container { get; set; }

    /// <summary>
    /// Gets the runtime mode
    /// </summary>
    [IgnoreDataMember, JsonIgnore, YamlIgnore]
    public virtual string Mode => this.Native != null ? OperatorRuntimeMode.Native : this.Container != null ? OperatorRuntimeMode.Containerized : throw new Exception("The runtime mode must be set");

}
