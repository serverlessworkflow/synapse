using Synapse.Domain.Models;

namespace Synapse.Services
{
    /// <summary>
    /// Defines the fundamentals of a service used to create <see cref="IExpressionEvaluator"/>s
    /// </summary>
    public interface IExpressionEvaluatorFactory
    {

        /// <summary>
        /// Creates a new <see cref="IExpressionEvaluator"/> for the specified <see cref="V1Workflow"/>
        /// </summary>
        /// <param name="workflow">The <see cref="V1Workflow"/> to create a new <see cref="IExpressionEvaluator"/> for</param>
        /// <returns>A new <see cref="IExpressionEvaluator"/></returns>
        public IExpressionEvaluator Create(V1Workflow workflow);

    }

}
