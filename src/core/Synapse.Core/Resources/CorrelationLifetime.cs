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
/// Enumerates default correlation lifetimes
/// </summary>
public static class CorrelationLifetime
{

    /// <summary>
    /// Indicates a durable correlation
    /// </summary>
    public const string Durable = "durable";

    /// <summary>
    /// Indicates an ephemeral correlation, which is a single use correlation
    /// </summary>
    public const string Ephemeral = "ephemeral";

    /// <summary>
    /// Gets all supported correlation lifetime
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> supporting all default correlation lifetime</returns>
    public static IEnumerable<string> AsEnumerable()
    {
        yield return Durable;
        yield return Ephemeral;
    }

}