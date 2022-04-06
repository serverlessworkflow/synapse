using Neuroglia;

namespace Synapse.Dashboard
{
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
