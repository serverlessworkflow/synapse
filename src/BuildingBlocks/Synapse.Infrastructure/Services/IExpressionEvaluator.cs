using Newtonsoft.Json.Linq;

namespace Synapse.Services
{
    /// <summary>
    /// Defines the fundamentals of a service used to evaluate serverless workflow expressions
    /// </summary>
    public interface IExpressionEvaluator
    {

        /// <summary>
        /// Gets the <see cref="IExpressionEvaluator"/>'s language
        /// </summary>
        string Language { get; }

        /// <summary>
        /// Evaluates the value of the specified serverless workflow expression
        /// </summary>
        /// <param name="expression">The serverless workflow expression to evaluate</param>
        /// <param name="data">The data to evaluate the specified expression against</param>
        /// <returns>A new <see cref="JToken"/> that represents the resolved serverless workflow expression's value</returns>
        JToken Evaluate(string expression, JToken data);

    }

}
