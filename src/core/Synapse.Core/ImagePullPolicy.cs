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

namespace Synapse;

/// <summary>
/// Enumerates all default container image pull policies
/// </summary>
public static class ImagePullPolicy
{

    /// <summary>
    /// Gets the policy that specifies that the image should always be pulled
    /// </summary>
    public const string Always = "Always";
    /// <summary>
    /// Gets the policy that specifies that the image should be pulled only if it is not already present locally
    /// </summary>
    public const string IfNotPresent = "IfNotPresent";
    /// <summary>
    /// Gets the policy that specifies that the image should never be pulled
    /// </summary>
    public const string Never = "Never";

    /// <summary>
    /// Gets a new <see cref="IEnumerable{T}"/> containing all default container image pull policies
    /// </summary>
    /// <returns>A new <see cref="IEnumerable{T}"/> containing all default container image pull policies</returns>
    public static IEnumerable<string> AsEnumerable()
    {
        yield return Always;
        yield return IfNotPresent;
        yield return Never;
    }

}