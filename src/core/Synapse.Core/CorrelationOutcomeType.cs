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
/// Exposes default correlation outcome types
/// </summary>
public static class CorrelationOutcomeType
{

    /// <summary>
    /// Indicates that the correlation correlates to an existing workflow instance
    /// </summary>
    public const string Correlate = "correlate";
    /// <summary>
    /// Indicates that the correlation starts a new instance of a workflow
    /// </summary>
    public const string Start = "start";

    /// <summary>
    /// Gets an <see cref="IEnumerable{T}"/> containing default correlation outcome types
    /// </summary>
    /// <returns>A new <see cref="IEnumerable{T}"/> containing default correlation outcome types</returns>
    public static IEnumerable<string> AsEnumerable()
    {
        yield return Start;
        yield return Correlate;
    }

}