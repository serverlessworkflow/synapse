using Neuroglia;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Synapse.Dashboard
{
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

}
