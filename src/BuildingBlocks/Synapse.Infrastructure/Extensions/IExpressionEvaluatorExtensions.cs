using Newtonsoft.Json.Linq;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Services;

namespace Synapse
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
        public static bool EvaluateCondition(this IExpressionEvaluator evaluator, string expression, JToken data)
        {
            JToken result = evaluator.Evaluate(expression, data);
            if (result == null)
                return false;
            if (result.Type == JTokenType.Boolean)
                return result.ToObject<bool>();
            return true;
        }

        /// <summary>
        /// Filters the input of the specified <see cref="StateDefinition"/>
        /// </summary>
        /// <param name="evaluator">The <see cref="IExpressionEvaluator"/> to use</param>
        /// <param name="state">The <see cref="StateDefinition"/> to filter the input for</param>
        /// <param name="input">The input data to filter</param>
        /// <returns>The filtered input</returns>
        public static JToken FilterInput(this IExpressionEvaluator evaluator, StateDefinition state, JToken input)
        {
            if (state.DataFilter == null
                || string.IsNullOrWhiteSpace(state.DataFilter.Input))
                return input;
            return evaluator.Evaluate(state.DataFilter.Input, input);
        }

        /// <summary>
        /// Filters the output of the specified <see cref="StateDefinition"/>
        /// </summary>
        /// <param name="evaluator">The <see cref="IExpressionEvaluator"/> to use</param>
        /// <param name="state">The <see cref="StateDefinition"/> to filter the output for</param>
        /// <param name="output">The output data to filter</param>
        /// <returns>The filtered output</returns>
        public static JToken FilterOutput(this IExpressionEvaluator evaluator, StateDefinition state, JToken output)
        {
            if (state.DataFilter == null
                || string.IsNullOrWhiteSpace(state.DataFilter.Output))
                return output;
            return evaluator.Evaluate(state.DataFilter.Output, output);
        }

        /// <summary>
        /// Filters the input of the specified <see cref="ActionDefinition"/>
        /// </summary>
        /// <param name="evaluator">The <see cref="IExpressionEvaluator"/> to use</param>
        /// <param name="action">The <see cref="ActionDefinition"/> to filter the input for</param>
        /// <param name="input">The input data to filter</param>
        /// <returns>The filtered input</returns>
        public static JToken FilterInput(this IExpressionEvaluator evaluator, ActionDefinition action, JToken input)
        {
            if (action.DataFilter == null
                || string.IsNullOrWhiteSpace(action.DataFilter.FromStateData))
                return input;
            return evaluator.Evaluate(action.DataFilter.FromStateData, input);
        }

        /// <summary>
        /// Filters the output of the specified <see cref="ActionDefinition"/>
        /// </summary>
        /// <param name="evaluator">The <see cref="IExpressionEvaluator"/> to use</param>
        /// <param name="action">The <see cref="ActionDefinition"/> to filter the output for</param>
        /// <param name="output">The output data to filter</param>
        /// <returns>The filtered output</returns>
        public static JToken FilterResults(this IExpressionEvaluator evaluator, ActionDefinition action, JToken output)
        {
            if (action.DataFilter == null
                || string.IsNullOrWhiteSpace(action.DataFilter.Results))
                return output;
            return evaluator.Evaluate(action.DataFilter.Results, output);
        }

        /// <summary>
        /// Filters output data based on the specified <see cref="EventDataFilterDefinition"/>
        /// </summary>
        /// <param name="evaluator">The <see cref="IExpressionEvaluator"/> to use</param>
        /// <param name="filter">The <see cref="EventDataFilterDefinition"/> used to filter the specified output</param>
        /// <param name="output">The output data to filter</param>
        /// <returns>The filtered output</returns>
        public static JToken FilterOutput(this IExpressionEvaluator evaluator, EventDataFilterDefinition filter, JToken output)
        {
            if (filter == null
                || string.IsNullOrWhiteSpace(filter.Data))
                return output;
            return evaluator.Evaluate(filter.Data, output);
        }

    }

}
