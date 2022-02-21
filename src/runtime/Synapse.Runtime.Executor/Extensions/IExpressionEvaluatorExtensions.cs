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

    }

}