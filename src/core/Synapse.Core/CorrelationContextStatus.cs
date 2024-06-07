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
/// Exposes all default statuses of a correlation context 
/// </summary>
public static class CorrelationContextStatus
{

    /// <summary>
    /// Indicates that the context is currently active and in use.
    /// </summary>
    public const string Active = "active";
    /// <summary>
    /// Indicates that the context is inactive or paused
    /// </summary>
    public const string Inactive = "inactive";
    /// <summary>
    /// Indicates that the correlation process has been successfully completed.
    /// </summary>
    public const string Completed = "completed";
    /// <summary>
    /// Indicates that the correlation process has been cancelled.
    /// </summary>
    public const string Cancelled = "cancelled";

    /// <summary>
    /// Gets a new <see cref="IEnumerable{T}"/> used to enumerate the default correlation context statuses
    /// </summary>
    /// <returns>A new <see cref="IEnumerable{T}"/> used to enumerate the default correlation context statuses</returns>
    public static IEnumerable<string> AsEnumerable()
    {
        yield return Active;
        yield return Inactive;
        yield return Completed;
        yield return Cancelled;
    }

}