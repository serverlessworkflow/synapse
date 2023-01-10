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

using Neuroglia.Data.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Text.RegularExpressions;

namespace Synapse.Worker
{

    /// <summary>
    /// Defines extensions for <see cref="IExpressionEvaluator"/>s
    /// </summary>
    public static class IWorkflowRuntimeContextExtensions
    {

        /// <summary>
        /// Evaluates an object against the specified data
        /// </summary>
        /// <param name="context">The current <see cref="IWorkflowRuntimeContext"/></param>
        /// <param name="expressionObject">The object to evaluate</param>
        /// <param name="data">The data to evaluate the object against</param>
        /// <param name="authorization">The current <see cref="AuthorizationInfo"/>, if any</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The evaluated object</returns>
        public static async Task<object?> EvaluateObjectAsync(this IWorkflowRuntimeContext context, object expressionObject, object data, AuthorizationInfo? authorization, CancellationToken cancellationToken = default)
        {
            var json = JsonConvert.SerializeObject(expressionObject, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }); ;
            foreach (var match in Regex.Matches(json, @"""\$\{.+?\}""", RegexOptions.Compiled).Cast<Match>())
            {
                var expression = Regex.Unescape(match.Value[3..^2].Trim().Replace(@"\""", @""""));
                var evaluationResult = await context.EvaluateAsync(expression, data, authorization, cancellationToken);
                if (evaluationResult == null) continue;
                var valueToken = JToken.FromObject(evaluationResult);
                var value = null as string;
                if (valueToken != null)
                {
                    value = valueToken.Type switch
                    {
                        JTokenType.String => JsonConvert.SerializeObject(valueToken),
                        _ => valueToken.ToString(),
                    };
                }
                if (string.IsNullOrEmpty(value)) value = "null";
                json = json.Replace(match.Value, value);
            }
            return JsonConvert.DeserializeObject<ExpandoObject>(json)!;
        }

        /// <summary>
        /// Evaluates an object against the specified data
        /// </summary>
        /// <param name="context">The current <see cref="IWorkflowRuntimeContext"/></param>
        /// <param name="expressionObject">The object to evaluate</param>
        /// <param name="data">The data to evaluate the object against</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The evaluated object</returns>
        public static Task<object?> EvaluateObjectAsync(this IWorkflowRuntimeContext context, object expressionObject, object data, CancellationToken cancellationToken = default)
        {
            return EvaluateObjectAsync(context, expressionObject, data, null, cancellationToken);
        }

        /// <summary>
        /// Evaluates the specified condition expression
        /// </summary>
        /// <param name="context">The <see cref="IWorkflowRuntimeContext"/> to use</param>
        /// <param name="expression">The condition expression to evaluate</param>
        /// <param name="data">The data to perform the evaluation against</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A boolean indicating whether or not the condition expression matches to the specified data</returns>
        public static async Task<bool> EvaluateConditionAsync(this IWorkflowRuntimeContext context, string expression, object data, CancellationToken cancellationToken = default)
        {
            var result = await context.EvaluateAsync(expression, data, cancellationToken);
            if (result == null)
                return false;
            if (result is bool success)
                return success;
            else
                return true;
        }

        /// <summary>
        /// Filters the input of the specified <see cref="StateDefinition"/>
        /// </summary>
        /// <param name="context">The <see cref="IWorkflowRuntimeContext"/> to use</param>
        /// <param name="state">The <see cref="StateDefinition"/> to filter the input for</param>
        /// <param name="input">The input data to filter</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The filtered input</returns>
        public static async Task<object?> FilterInputAsync(this IWorkflowRuntimeContext context, StateDefinition state, object input, CancellationToken cancellationToken = default)
        {
            if (state.DataFilter == null
                || string.IsNullOrWhiteSpace(state.DataFilter.Input))
                return input;
            return await context.EvaluateAsync(state.DataFilter.Input, input, cancellationToken);
        }

        /// <summary>
        /// Filters the input of the specified <see cref="ActionDefinition"/>
        /// </summary>
        /// <param name="context">The <see cref="IWorkflowRuntimeContext"/> to use</param>
        /// <param name="action">The <see cref="ActionDefinition"/> to filter the input for</param>
        /// <param name="input">The input data to filter</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The filtered input</returns>
        public static async Task<object?> FilterInputAsync(this IWorkflowRuntimeContext context, ActionDefinition action, object input, CancellationToken cancellationToken = default)
        {
            if (action.ActionDataFilter == null
                 || string.IsNullOrWhiteSpace(action.ActionDataFilter.FromStateData))
                return input;
            return await context.EvaluateAsync(action.ActionDataFilter.FromStateData, input, cancellationToken);
        }

        /// <summary>
        /// Filters the output of the specified <see cref="StateDefinition"/>
        /// </summary>
        /// <param name="context">The <see cref="IWorkflowRuntimeContext"/> to use</param>
        /// <param name="state">The <see cref="StateDefinition"/> to filter the output for</param>
        /// <param name="output">The output data to filter</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The filtered output</returns>
        public static async Task<object?> FilterOutputAsync(this IWorkflowRuntimeContext context, StateDefinition state, object output, CancellationToken cancellationToken = default)
        {
            if (state.DataFilter == null || string.IsNullOrWhiteSpace(state.DataFilter.Output)) return output;
            return await context.EvaluateAsync(state.DataFilter.Output, output, cancellationToken);
        }

        /// <summary>
        /// Filters the output of the specified <see cref="ActionDefinition"/>
        /// </summary>
        /// <param name="context">The <see cref="IWorkflowRuntimeContext"/> to use</param>
        /// <param name="action">The <see cref="ActionDefinition"/> to filter the output for</param>
        /// <param name="output">The output data to filter</param>
        /// <param name="authorization">The current <see cref="AuthorizationInfo"/>, if any</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The filtered output</returns>
        public static async Task<object?> FilterOutputAsync(this IWorkflowRuntimeContext context, ActionDefinition action, object output, AuthorizationInfo? authorization, CancellationToken cancellationToken = default)
        {
            if (action.ActionDataFilter == null
                || string.IsNullOrWhiteSpace(action.ActionDataFilter.Results))
                return output;
            return await context.EvaluateAsync(action.ActionDataFilter.Results, output, authorization, cancellationToken);
        }

        /// <summary>
        /// Filters the output of the specified <see cref="ActionDefinition"/>
        /// </summary>
        /// <param name="context">The <see cref="IWorkflowRuntimeContext"/> to use</param>
        /// <param name="action">The <see cref="ActionDefinition"/> to filter the output for</param>
        /// <param name="output">The output data to filter</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The filtered output</returns>
        public static Task<object?> FilterOutputAsync(this IWorkflowRuntimeContext context, ActionDefinition action, object output, CancellationToken cancellationToken = default)
        {
            return FilterOutputAsync(context, action, output, cancellationToken);
        }

        /// <summary>
        /// Filters output data based on the specified <see cref="EventDataFilterDefinition"/>
        /// </summary>
        /// <param name="context">The <see cref="IWorkflowRuntimeContext"/> to use</param>
        /// <param name="filter">The <see cref="EventDataFilterDefinition"/> used to filter the specified output</param>
        /// <param name="output">The output data to filter</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The filtered output</returns>
        public static async Task<object?> FilterOutputAsync(this IWorkflowRuntimeContext context, EventDataFilterDefinition filter, object output, CancellationToken cancellationToken = default)
        {
            if (filter == null
                || string.IsNullOrWhiteSpace(filter.Data))
                return output;
            return await context.EvaluateAsync(filter.Data, output, cancellationToken);
        }

    }

}