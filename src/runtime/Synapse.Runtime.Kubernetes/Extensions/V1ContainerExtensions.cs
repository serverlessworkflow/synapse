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

using k8s.Models;

namespace Synapse.Runtime.Kubernetes;

/// <summary>
/// Defines extensions for <see cref="V1Container"/>s
/// </summary>
public static class V1ContainerExtensions
{

    /// <summary>
    /// Sets the specified environment variable
    /// </summary>
    /// <param name="container">The extended <see cref="V1Container"/></param>
    /// <param name="name">The name of the environment variable to set</param>
    /// <param name="value">The value of the environment variable to set</param>
    public static void SetEnvironmentVariable(this V1Container container, string name, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        container.Env ??= [];
        var variable = container.Env.FirstOrDefault(s => s.Name == name);
        if (variable == null) container.Env.Add(new(name, value));
        else variable.Value = value;
    }

}
