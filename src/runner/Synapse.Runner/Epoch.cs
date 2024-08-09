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

using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Synapse.Runner;

/// <summary>
/// Represents an epoch, which is the duration elapsed between a datetime and midnight of 1970-01-01 UTC
/// </summary>
[DataContract]
public record Epoch
{

    /// <summary>
    /// Gets/sets the epoch's total milliseconds
    /// </summary>
    [DataMember(Name = "ms", Order = 1), JsonPropertyName("ms"), JsonPropertyOrder(1), YamlMember(Alias = "ms", Order = 1)]
    public virtual int Milliseconds { get; set; }

}