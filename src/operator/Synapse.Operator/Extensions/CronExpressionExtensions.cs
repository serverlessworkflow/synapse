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

using Cronos;

namespace Synapse.Operator;

/// <summary>
/// Defines extensions for <see cref="CronExpression"/>s
/// </summary>
public static class CronExpressionExtensions
{

    /// <summary>
    /// Calculates next occurrence starting with <paramref name="from"/> (optionally <paramref name="inclusive"/>) in UTC time zone
    /// </summary>
    /// <param name="expression">The extended <see cref="CronExpression"/></param>
    /// <param name="from">The date and time from which to compute the next occurrence</param>
    /// <param name="inclusive">A boolean indicating whether or not the the next occurrence should be computed in an inclusive manner</param>
    /// <returns>The <see cref="CronExpression"/>'s next occurrence, if any</returns>
    public static DateTimeOffset? GetNextOccurrence(this CronExpression expression, DateTimeOffset from, bool inclusive = false)
    {
        var fromUtc = from.DateTime.ToUniversalTime();
        var nextOccurrenceUtc = expression.GetNextOccurrence(fromUtc, inclusive);
        if (!nextOccurrenceUtc.HasValue) return null;
        return new DateTimeOffset(nextOccurrenceUtc.Value, TimeSpan.Zero).ToOffset(from.Offset);
    }

}
