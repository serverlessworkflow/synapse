using System.Linq.Expressions;

namespace Synapse.Dashboard
{
    public static class ExpressionExtensions
    {

        public static Expression CombineWith(this Expression left, Expression right)
        {
            return Expression.And(left, right);
        }

    }

}
