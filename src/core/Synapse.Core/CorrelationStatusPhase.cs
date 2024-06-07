// Copyright © 2024-Present Neuroglia SRL. All rights reserved.
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
/// Exposes all default correlation status phases
/// </summary>
public static class CorrelationStatusPhase
{

    /// <summary>
    /// Indicates that the correlation is pending processing by a correlator
    /// </summary>
    public const string Pending = "pending";
    /// <summary>
    /// Indicates that the correlation has been picked up by a correlator and is actively correlating ingested cloud events
    /// </summary>
    public const string Active = "active";
    /// <summary>
    /// Indicates that the correlation is inactive and is not correlating events
    /// </summary>
    public const string Inactive = "inactive";
    /// <summary>
    /// Indicates that an ephemeral correlation has been completed
    /// </summary>
    public const string Completed = "completed";
    /// <summary>
    /// Indicates that the correlation has been cancelled
    /// </summary>
    public const string Cancelled = "cancelled";

    /// <summary>
    /// Gets an <see cref="IEnumerable{T}"/> containing default correlation status phases
    /// </summary>
    /// <returns>A new <see cref="IEnumerable{T}"/> containing default correlation status phases</returns>
    public static IEnumerable<string> AsEnumerable()
    {
        yield return Pending;
        yield return Active;
        yield return Inactive;
        yield return Completed;
        yield return Cancelled;
    }

}