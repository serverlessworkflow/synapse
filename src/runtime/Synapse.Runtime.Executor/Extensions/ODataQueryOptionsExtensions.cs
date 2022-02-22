/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

namespace Synapse.Runtime
{
    /// <summary>
    /// Defines extensions for <see cref="ODataQueryOptions"/>
    /// </summary>
    public static class ODataQueryOptionsExtensions
    {

        /// <summary>
        /// Converts the specified <see cref="ODataQueryOptions"/> to an ODATA query string
        /// </summary>
        /// <param name="queryOptions">The <see cref="ODataQueryOptions"/> to convert</param>
        /// <returns>An ODATA query string</returns>
        public static string ToQueryString(this ODataQueryOptions queryOptions)
        {
            if (queryOptions == null)
                throw new ArgumentNullException(nameof(queryOptions));
            var values = new List<string>(12);
            if (!string.IsNullOrWhiteSpace(queryOptions.Compute))
                values.Add($"$compute={queryOptions.Compute}");
            if (queryOptions.Count.HasValue && queryOptions.Count.Value)
                values.Add($"$count={queryOptions.Count}");
            if (!string.IsNullOrWhiteSpace(queryOptions.Expand))
                values.Add($"$expand={queryOptions.Expand}");
            if (!string.IsNullOrWhiteSpace(queryOptions.Filter))
                values.Add($"$filter={queryOptions.Filter}");
            if (!string.IsNullOrWhiteSpace(queryOptions.Format))
                values.Add($"$format={queryOptions.Format}");
            if (!string.IsNullOrWhiteSpace(queryOptions.OrderBy))
                values.Add($"$orderBy={queryOptions.OrderBy}");
            if (!string.IsNullOrWhiteSpace(queryOptions.Search))
                values.Add($"$search={queryOptions.Search}");
            if (!string.IsNullOrWhiteSpace(queryOptions.Select))
                values.Add($"$select={queryOptions.Select}");
            if (queryOptions.Skip.HasValue)
                values.Add($"$skip={queryOptions.Skip}");
            if (queryOptions.Top.HasValue)
                values.Add($"$top={queryOptions.Top}");
            return string.Join("&", values);
        }

    }

}