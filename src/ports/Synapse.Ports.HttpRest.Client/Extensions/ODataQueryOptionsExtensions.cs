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

namespace Synapse
{

    /// <summary>
    /// Defines extensions for <see cref="Microsoft.AspNetCore.OData.Query.ODataQueryOptions"/>
    /// </summary>
    public static class ODataQueryOptionsExtensions
    {

        /// <summary>
        /// Converts the specified <see cref="Microsoft.AspNetCore.OData.Query.ODataQueryOptions"/> to an ODATA query string
        /// </summary>
        /// <param name="queryOptions">The <see cref="Microsoft.AspNetCore.OData.Query.ODataQueryOptions"/> to convert</param>
        /// <returns>An ODATA query string</returns>
        public static string ToQueryString(this Microsoft.AspNetCore.OData.Query.ODataQueryOptions queryOptions)
        {
            if(queryOptions == null)
                throw new ArgumentNullException(nameof(queryOptions));
            var values = new List<string>(12);
            if (!string.IsNullOrWhiteSpace(queryOptions.RawValues.Apply))
                values.Add($"$apply={queryOptions.RawValues.Apply}");
            if (!string.IsNullOrWhiteSpace(queryOptions.RawValues.Compute))
                values.Add($"$compute={queryOptions.RawValues.Compute}");
            if (!string.IsNullOrWhiteSpace(queryOptions.RawValues.Count))
                values.Add($"$count={queryOptions.RawValues.Count}");
            if (!string.IsNullOrWhiteSpace(queryOptions.RawValues.DeltaToken))
                values.Add($"$deltatoken={queryOptions.RawValues.DeltaToken}");
            if (!string.IsNullOrWhiteSpace(queryOptions.RawValues.Expand))
                values.Add($"$expand={queryOptions.RawValues.Expand}");
            if (!string.IsNullOrWhiteSpace(queryOptions.RawValues.Filter))
                values.Add($"$filter={queryOptions.RawValues.Filter}");
            if (!string.IsNullOrWhiteSpace(queryOptions.RawValues.Format))
                values.Add($"$format={queryOptions.RawValues.Format}");
            if (!string.IsNullOrWhiteSpace(queryOptions.RawValues.OrderBy))
                values.Add($"$orderBy={queryOptions.RawValues.OrderBy}");
            if (!string.IsNullOrWhiteSpace(queryOptions.RawValues.Search))
                values.Add($"$search={queryOptions.RawValues.Search}");
            if (!string.IsNullOrWhiteSpace(queryOptions.RawValues.Select))
                values.Add($"$select={queryOptions.RawValues.Select}");
            if (!string.IsNullOrWhiteSpace(queryOptions.RawValues.Skip))
                values.Add($"$skip={queryOptions.RawValues.Skip}");
            if (!string.IsNullOrWhiteSpace(queryOptions.RawValues.SkipToken))
                values.Add($"$skiptoken={queryOptions.RawValues.SkipToken}");
            if (!string.IsNullOrWhiteSpace(queryOptions.RawValues.Top))
                values.Add($"$top={queryOptions.RawValues.Top}");
            return string.Join("&", values);
        }

    }

}
