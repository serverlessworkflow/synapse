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

namespace Synapse.Runner.Configuration;

/// <summary>
/// Enumerates all modes of execution for the Synapse Runner application
/// </summary>
public enum RunnerExecutionMode
{
    /// <summary>
    /// The runner operates in a connected mode, interacting with the remote API.
    /// In this mode, the runner depends on external services and APIs for its functionality.
    /// </summary>
    [EnumMember(Value = "connected")]
    Connected,
    /// <summary>
    /// The runner operates in a standalone mode, functioning independently without relying on the remote API.
    /// This mode is ideal for scenarios where external dependencies are unavailable or not required.
    /// </summary>
    [EnumMember(Value = "standalone")]
    StandAlone
}