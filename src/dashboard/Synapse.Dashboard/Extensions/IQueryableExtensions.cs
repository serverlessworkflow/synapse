/*
 * Copyright © 2022-Present The Synapse Authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using Neuroglia;
using System.Collections;
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
        private static readonly MethodInfo SkipMethod = typeof(Queryable).GetMethod(nameof(Queryable.Skip))!;
        private static readonly MethodInfo TakeMethod = typeof(Queryable).GetMethods().First(m => m.Name == nameof(Queryable.Take));
        private static readonly MethodInfo ToListMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList))!;
        private static readonly MethodInfo ToListAsyncMethod = typeof(AsyncEnumerable).GetMethod(nameof(AsyncEnumerable.ToListAsync))!;

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

}
