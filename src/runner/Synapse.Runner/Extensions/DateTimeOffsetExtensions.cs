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

using Synapse.Runner;

namespace Synapse;

/// <summary>
/// Defines extensions for <see cref="DateTimeOffset"/>s
/// </summary>
public static class DateTimeOffsetExtensions
{

    /// <summary>
    /// Gets an object used to describe the <see cref="DateTimeOffset"/>
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTimeOffset"/> to describe</param>
    /// <returns>A new <see cref="DateTimeDescriptor"/></returns>
    public static DateTimeDescriptor GetDescriptor(this DateTimeOffset dateTime)
    {
        return new()
        {
            Iso8601 = dateTime.ToString(),
            Epoch = dateTime.AsEpoch()
        };
    }

    /// <summary>
    /// Converts the datetime into a new <see cref="Epoch"/>
    /// </summary>
    /// <param name="dateTime">The datetime to convert</param>
    /// <returns>A new <see cref="Epoch"/></returns>
    public static Epoch AsEpoch(this DateTimeOffset dateTime)
    {
        var duration = dateTime.Subtract(new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero));
        return new()
        {
            Milliseconds = (int)duration.TotalMilliseconds
        };
    }

}