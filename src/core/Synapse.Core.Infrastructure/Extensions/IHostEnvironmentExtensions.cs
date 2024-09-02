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

using Microsoft.Extensions.Hosting;

namespace Synapse;

/// <summary>
/// Defines extensions for <see cref="IHostEnvironment"/>s
/// </summary>
public static class IHostEnvironmentExtensions
{

    /// <summary>
    /// Determines whether or not the <see cref="IHostEnvironment"/> runs in Docker
    /// </summary>
    /// <param name="env">The <see cref="IHostEnvironment"/> to check</param>
    /// <returns>A boolean indicating whether or not the <see cref="IHostEnvironment"/> runs in Docker</returns>
    public static bool RunsInDocker(this IHostEnvironment env) => File.Exists("/.dockerenv");

    /// <summary>
    /// Determines whether or not the <see cref="IHostEnvironment"/> runs in Kubernetes
    /// </summary>
    /// <param name="env">The <see cref="IHostEnvironment"/> to check</param>
    /// <returns>A boolean indicating whether or not the <see cref="IHostEnvironment"/> runs in Kubernetes</returns>
    public static bool RunsInKubernetes(this IHostEnvironment env) => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST"));

}