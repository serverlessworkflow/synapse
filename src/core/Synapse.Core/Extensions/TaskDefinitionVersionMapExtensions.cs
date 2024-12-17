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

using Semver;

namespace Synapse;

/// <summary>
/// Defines extensions for <see cref="EquatableDictionary{TKey, TValue}"/> instances that contains <see cref="TaskDefinition"/> version mappings
/// </summary>
public static class TaskDefinitionVersionMapExtensions
{

    /// <summary>
    /// Gets the latest version of the <see cref="TaskDefinition"/>
    /// </summary>
    /// <param name="definitions">An <see cref="IEnumerable{T}"/> containing the <see cref="TaskDefinition"/>s to get the latest of</param>
    /// <returns>The latest <see cref="TaskDefinition"/></returns>
    public static TaskDefinition GetLatest(this Map<string, TaskDefinition> definitions) => definitions.OrderByDescending(kvp => SemVersion.Parse(kvp.Key, SemVersionStyles.Strict), SemVersion.PrecedenceComparer).First().Value;

    /// <summary>
    /// Gets the latest version of the <see cref="TaskDefinition"/>
    /// </summary>
    /// <param name="definitions">An <see cref="IEnumerable{T}"/> containing the <see cref="TaskDefinition"/>s to get the latest of</param>
    /// <returns>The latest version</returns>
    public static string GetLatestVersion(this Map<string, TaskDefinition> definitions) => definitions.OrderByDescending(kvp => SemVersion.Parse(kvp.Key, SemVersionStyles.Strict), SemVersion.PrecedenceComparer).First().Key;

    /// <summary>
    /// Gets the specified <see cref="TaskDefinition"/> version
    /// </summary>
    /// <param name="definitions">An <see cref="IEnumerable{T}"/> containing the <see cref="TaskDefinition"/>s to get the specified version from</param>
    /// <param name="version">The version of the <see cref="TaskDefinition"/> to get</param>
    /// <returns>The <see cref="TaskDefinition"/> with the specified version, if any</returns>
    public static TaskDefinition? Get(this Map<string, TaskDefinition> definitions, string version) => definitions.FirstOrDefault(kvp => kvp.Key == version)?.Value;

}