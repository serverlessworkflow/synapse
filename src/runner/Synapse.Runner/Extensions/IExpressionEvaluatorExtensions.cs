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

using Neuroglia.Data.Expressions;

namespace Synapse;

/// <summary>
/// Defines extensions for <see cref="IExpressionEvaluator"/>s
/// </summary>
public static class IExpressionEvaluatorExtensions
{

    /// <summary>
    /// Determines whether or not the source mapping matches the specified filters
    /// </summary>
    /// <param name="evaluator">The extended <see cref="IExpressionEvaluator"/></param>
    /// <param name="source">The source key/value mapping to match</param>
    /// <param name="filter">A key/value mapping containing the filters to match</param>
    /// <param name="arguments">A key/value mapping containing the evaluation's arguments, if any</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A boolean indicating whether or not the source mapping matches the specified filters</returns>
    public static async Task<bool> MatchesAsync(this IExpressionEvaluator evaluator, object? source, object? filter, IDictionary<string, object>? arguments = null, CancellationToken cancellationToken = default)
    {
        if (filter == null) return true;
        if (source == null) return false;
        var sourceProperties = (IDictionary<string, object>)JsonSerializer.Default.Convert(source, typeof(IDictionary<string, object>))!;
        var filterProperties = (IDictionary<string, object>)JsonSerializer.Default.Convert(filter, typeof(IDictionary<string, object>))!;
        foreach (var filterProperty in filterProperties)
        {
            if (!sourceProperties.TryGetValue(filterProperty.Key, out object? propertyValue)) return false;
            if (filterProperty.Value is string expression)
            {
                if (expression.IsRuntimeExpression())
                {
                    if (!await evaluator.EvaluateAsync<bool>(expression, propertyValue, arguments, cancellationToken).ConfigureAwait(false)) return false;
                    else continue;
                }
                else if (propertyValue is string propertyValueStr && !Regex.IsMatch(propertyValueStr, expression)) return false;
            }
            if (!propertyValue.Equals(filterProperty.Value)) return false;
        }
        return true;
    }

}