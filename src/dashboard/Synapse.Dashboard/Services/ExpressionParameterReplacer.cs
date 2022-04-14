using System.Linq.Expressions;

namespace Synapse.Services
{
    public class ExpressionParameterReplacer
        : ExpressionVisitor
    {

        public ExpressionParameterReplacer(ParameterExpression parameterExpression)
        {
            this.ParameterExpression = parameterExpression;
        }

        protected ParameterExpression ParameterExpression { get; }

        /// <inheritdoc/>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return base.VisitParameter(this.ParameterExpression);
        }

    }

}
