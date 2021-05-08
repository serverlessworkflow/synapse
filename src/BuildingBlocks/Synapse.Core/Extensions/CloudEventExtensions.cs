using CloudNative.CloudEvents;
using System;
using System.Collections.Generic;

namespace Synapse
{

    /// <summary>
    /// Defines extensions for <see cref="CloudEvent"/>s
    /// </summary>
    public static class CloudEventExtensions
    {

        /// <summary>
        /// Attempts to retrieve the specified attribute
        /// </summary>
        /// <param name="e">The <see cref="CloudEvent"/> to attempt to get an attribute of</param>
        /// <param name="key">The key of the attribute to retrieve</param>
        /// <param name="value">The attribute's value, if any</param>
        /// <returns>A boolean indicating whether or not the specified attribute could be retrieved</returns>
        public static bool TryGetAttribute(this CloudEvent e, string key, out object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            IDictionary<string, object> attributes = e.GetAttributes();
            return attributes.TryGetValue(key.ToLower(), out value);
        }

        /// <summary>
        /// Attempts to retrieve the specified attribute
        /// </summary>
        /// <param name="e">The <see cref="CloudEvent"/> to attempt to get an attribute of</param>
        /// <param name="key">The key of the attribute to retrieve</param>
        /// <param name="value">The attribute's value, if any</param>
        /// <returns>A boolean indicating whether or not the specified attribute could be retrieved</returns>
        public static bool TryGetAttribute(this CloudEvent e, string key, out string value)
        {
            value = null;
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            IDictionary<string, object> attributes = e.GetAttributes();
            if (!attributes.TryGetValue(key.ToLower(), out object valueObject))
                return false;
            value = valueObject.ToString();
            return true;
        }

    }

}
