﻿// Copyright © 2024-Present The Synapse Authors
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

namespace Synapse;

/// <summary>
/// Enumerates all default operator runtime modes
/// </summary>
public static class OperatorRuntimeMode
{

    /// <summary>
    /// Gets the native operator runtime mode
    /// </summary>
    public const string Native = "native";
    /// <summary>
    /// Gets the Docker operator runtime mode
    /// </summary>
    public const string Docker = "docker";
    /// <summary>
    /// Gets the Kubernetes operator runtime mode
    /// </summary>
    public const string Kubernetes = "kubernetes";

    /// <summary>
    /// Gets a new <see cref="IEnumerable{T}"/> containing all default operator runtime modes
    /// </summary>
    /// <returns>A new <see cref="IEnumerable{T}"/> containing all default operator runtime modes</returns>
    public static IEnumerable<string> AsEnumerable()
    {
        yield return Native;
        yield return Docker;
        yield return Kubernetes;
    }

}
