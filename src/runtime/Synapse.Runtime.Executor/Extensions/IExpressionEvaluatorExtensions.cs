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

namespace Synapse.Runtime
{

    /// <summary>
    /// Defines extensions for <see cref="IExpressionEvaluator"/>s
    /// </summary>
    public static class IExpressionEvaluatorExtensions
    {

        /// <summary>
        /// Evaluates the specified condition expression
        /// </summary>
        /// <param name="evaluator">The <see cref="IExpressionEvaluator"/> to use</param>
        /// <param name="expression">The condition expression to evaluate</param>
        /// <param name="data">The data to perform the evaluation against</param>
        /// <returns>A boolean indicating whether or not the condition expression matches to the specified data</returns>
        public static bool EvaluateCondition(this IExpressionEvaluator evaluator, string expression, object data)
        {
            var result = evaluator.Evaluate(expression, data);
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
        /// <param name="evaluator">The <see cref="IExpressionEvaluator"/> to use</param>
        /// <param name="state">The <see cref="StateDefinition"/> to filter the input for</param>
        /// <param name="input">The input data to filter</param>
        /// <returns>The filtered input</returns>
        public static object? FilterInput(this IExpressionEvaluator evaluator, StateDefinition state, object input)
        {
            if (state.DataFilter == null
                || string.IsNullOrWhiteSpace(state.DataFilter.Input))
                return input;
            return evaluator.Evaluate(state.DataFilter.Input, input);
        }

        /// <summary>
        /// Filters the input of the specified <see cref="ActionDefinition"/>
        /// </summary>
        /// <param name="evaluator">The <see cref="IExpressionEvaluator"/> to use</param>
        /// <param name="action">The <see cref="ActionDefinition"/> to filter the input for</param>
        /// <param name="input">The input data to filter</param>
        /// <returns>The filtered input</returns>
        public static object? FilterInput(this IExpressionEvaluator evaluator, ActionDefinition action, object input)
        {
            if (action.ActionDataFilter == null
                 || string.IsNullOrWhiteSpace(action.ActionDataFilter.FromStateData))
                return input;
            return evaluator.Evaluate(action.ActionDataFilter.FromStateData, input);
        }

        /// <summary>
        /// Filters output data based on the specified <see cref="EventDataFilterDefinition"/>
        /// </summary>
        /// <param name="evaluator">The <see cref="IExpressionEvaluator"/> to use</param>
        /// <param name="filter">The <see cref="EventDataFilterDefinition"/> used to filter the specified output</param>
        /// <param name="output">The output data to filter</param>
        /// <returns>The filtered output</returns>
        public static object? FilterOutput(this IExpressionEvaluator evaluator, EventDataFilterDefinition filter, object output)
        {
            if (filter == null
                || string.IsNullOrWhiteSpace(filter.Data))
                return output;
            return evaluator.Evaluate(filter.Data, output);
        }

        /// <summary>
        /// Filters the output of the specified <see cref="ActionDefinition"/>
        /// </summary>
        /// <param name="evaluator">The <see cref="IExpressionEvaluator"/> to use</param>
        /// <param name="action">The <see cref="ActionDefinition"/> to filter the output for</param>
        /// <param name="output">The output data to filter</param>
        /// <returns>The filtered output</returns>
        public static object? FilterOutput(this IExpressionEvaluator evaluator, ActionDefinition action, object output)
        {
            if (action.ActionDataFilter == null
                || string.IsNullOrWhiteSpace(action.ActionDataFilter.Results))
                return output;
            return evaluator.Evaluate(action.ActionDataFilter.Results, output);
        }

    }

}