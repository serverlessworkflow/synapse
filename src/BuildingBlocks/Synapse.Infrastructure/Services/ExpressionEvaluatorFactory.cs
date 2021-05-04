using Synapse.Domain.Models;
using System;

namespace Synapse.Services
{

    /// <summary>
    /// Represents the default implementation of the <see cref="IExpressionEvaluatorFactory"/> interface
    /// </summary>
    public class ExpressionEvaluatorFactory
        : IExpressionEvaluatorFactory
    {

        /// <inheritdoc/>
        public virtual IExpressionEvaluator Create(V1Workflow workflow)
        {
            if (workflow == null)
                throw new ArgumentNullException(nameof(workflow));
            IExpressionEvaluator expressionEvaluator = workflow.Spec.Definition.ExpressionLanguage switch
            {
                "jq" => new JQExpressionEvaluator(workflow),
                _ => throw new NotSupportedException($"The specified expression language '{workflow.Spec.Definition.ExpressionLanguage}' is not supported."),
            };
            return expressionEvaluator;
        }

    }

}
