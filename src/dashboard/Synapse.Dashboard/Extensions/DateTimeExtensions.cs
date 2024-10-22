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

using moment.net;
using moment.net.Models;
using System.Globalization;

namespace Synapse.Dashboard;

/// <summary>
/// Provides extension methods for <see cref="DateTime"/>
/// </summary>
public static class DateTimeExtensions
{

    /// <summary>
    /// Formats the provided <see cref="DateTimeOffset"/> in a relative fashion (e.g.: 3 minutes ago, Yesterday at 1:00pm...)
    /// </summary>
    /// <param name="dateTime">The extended <see cref="DateTimeOffset"/></param>
    /// <returns>The <see cref="DateTimeOffset"/>, formatted in a relative fashion</returns>
    public static string RelativeFormat(this DateTimeOffset dateTime)
    {
        var localDateTime = dateTime.ToLocalTime();
        var now = DateTimeOffset.Now;
        var delta = now.Subtract(localDateTime);
        if (Math.Abs(delta.Days) >= 1)
        {
            var cultureFormats = CultureInfo.GetCultureInfo("en-US").DateTimeFormat;
            var defaults = new CalendarTimeFormats(CultureInfo.CurrentUICulture);
            var formats = new CalendarTimeFormats(
                defaults.SameDay,
                defaults.NextDay,
                defaults.NextWeek,
                defaults.LastDay,
                defaults.LastWeek,
                $"{cultureFormats.ShortDatePattern} {cultureFormats.ShortTimePattern}"
            );
            return now.DateTime.CalendarTime(localDateTime.DateTime, formats);
        }
        else if (delta < TimeSpan.Zero) return localDateTime.DateTime.ToNow();
        else return localDateTime.DateTime.FromNow();
    }
}
