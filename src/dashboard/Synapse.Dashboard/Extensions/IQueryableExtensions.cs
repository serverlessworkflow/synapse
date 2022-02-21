using Neuroglia;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace Synapse.Dashboard
{

    /// <summary>
    /// Defines extensions for <see cref="IQueryable"/>s
    /// </summary>
    public static class IQueryableExtensions
    {

        private static readonly MethodInfo WhereMethod = typeof(Queryable).GetMethods().First(m => m.Name == nameof(Queryable.Where));
        private static readonly MethodInfo SkipMethod = typeof(Queryable).GetMethod(nameof(Queryable.Skip));
        private static readonly MethodInfo TakeMethod = typeof(Queryable).GetMethods().First(m => m.Name == nameof(Queryable.Take));
        private static readonly MethodInfo ToListMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList));
        private static readonly MethodInfo ToListAsyncMethod = typeof(AsyncEnumerable).GetMethod(nameof(AsyncEnumerable.ToListAsync));

        public static IQueryable Where(this IQueryable queryable, LambdaExpression lambda)
        {
            if (queryable == null)
                throw new ArgumentNullException(nameof(queryable));
            MethodInfo method = WhereMethod.MakeGenericMethod(queryable.ElementType);
            return (IQueryable)method.Invoke(null, new object[] { queryable, lambda });
        }

        public static IQueryable Skip(this IQueryable queryable, int count)
        {
            if (queryable == null)
                throw new ArgumentNullException(nameof(queryable));
            MethodInfo method = SkipMethod.MakeGenericMethod(queryable.ElementType);
            return (IQueryable)method.Invoke(null, new object[] { queryable, count });
        }

        public static IQueryable Take(this IQueryable queryable, int count)
        {
            if (queryable == null)
                throw new ArgumentNullException(nameof(queryable));
            MethodInfo method = TakeMethod.MakeGenericMethod(queryable.ElementType);
            return (IQueryable)method.Invoke(null, new object[] { queryable, count });
        }

        /// <summary>
        /// Converts the <see cref="IQueryable"/> into a new <see cref="IList"/>
        /// </summary>
        /// <param name="queryable">The <see cref="IQueryable"/> to convert</param>
        /// <returns>A new <see cref="IList"/></returns>
        public static IList ToList(this IQueryable queryable)
        {
            if (queryable == null)
                throw new ArgumentNullException(nameof(queryable));
            MethodInfo method = ToListMethod.MakeGenericMethod(queryable.ElementType);
            return (IList)method.Invoke(null, new object[] { queryable });
        }

        /// <summary>
        /// Converts the <see cref="IQueryable"/> into a new <see cref="IList"/> asynchronously
        /// </summary>
        /// <param name="queryable">The <see cref="IQueryable"/> to convert</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IList"/></returns>
        public static async Task<IList> ToListAsync(this IQueryable queryable, CancellationToken cancellationToken = default)
        {
            if (queryable == null)
                throw new ArgumentNullException(nameof(queryable));
            MethodInfo method = ToListAsyncMethod.MakeGenericMethod(queryable.ElementType);
            return (IList)await method.InvokeAsync(null, new object[] { queryable, cancellationToken });
        }

        /// <summary>
        /// Converts the <see cref="IQueryable{T}"/> into a new <see cref="IList{T}"/> asynchronously
        /// </summary>
        /// <param name="queryable">The <see cref="IQueryable"/> to convert</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IList{T}"/></returns>
        public static async Task<IList<T>> ToListAsync<T>(this IQueryable<T> queryable, CancellationToken cancellationToken = default)
        {
            if (queryable == null)
                throw new ArgumentNullException(nameof(queryable));
            MethodInfo method = ToListAsyncMethod.MakeGenericMethod(queryable.ElementType);
            return (IList<T>)await method.InvokeAsync(null, new object[] { queryable, cancellationToken });
        }

    }

    public static class ExpressionExtensions
    {

        public static Expression CombineWith(this Expression left, Expression right)
        {
            return Expression.And(left, right);
        }

    }

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

    /// <summary>
    /// Defines extensions for <see cref="PropertyInfo"/>s
    /// </summary>
    public static class PropertyInfoExtensions
    {

        /// <summary>
        /// Gets the property's display name
        /// </summary>
        /// <param name="property">The <see cref="PropertyInfo"/> to get the display name for</param>
        /// <returns>The property's display name</returns>
        public static string GetDisplayName(this PropertyInfo property)
        {
            string name = null;
            if (property.TryGetCustomAttribute(out DisplayAttribute displayAttribute))
                name = displayAttribute.Name;
            if (string.IsNullOrWhiteSpace(name))
                name = property.Name;
            return name;
        }

        /// <summary>
        /// Gets the property's display order
        /// </summary>
        /// <param name="property">The <see cref="PropertyInfo"/> to get the display order for</param>
        /// <returns>The property's display order</returns>
        public static int GetDisplayOrder(this PropertyInfo property)
        {
            int? order = 0;
            if (property.TryGetCustomAttribute(out DisplayAttribute displayAttribute))
                order = displayAttribute.GetOrder();
            if (!order.HasValue)
                order = 1;
            return order.Value;
        }

    }

    /// <summary>
    /// Defines extensions for <see cref="Type"/>s
    /// </summary>
    public static class TypeExtensions
    {

        /// <summary>
        /// Determines whether the type implements the <see cref="IAsyncEnumerable{T}"/> interface
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>A boolean indicating whether the type implements the <see cref="IAsyncEnumerable{T}"/> interface</returns>
        public static bool IsAsyncEnumerable(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            return type.GetGenericType(typeof(IAsyncEnumerable<>)) != null;
        }

    }

}
