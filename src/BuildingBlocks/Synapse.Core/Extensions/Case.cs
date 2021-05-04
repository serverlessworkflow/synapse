using System;

namespace Synapse
{
    /// <summary>
    /// Enumerates all string cases
    /// </summary>
    [Flags]
    public enum Case
    {
        /// <summary>
        /// Indicates lowercase
        /// </summary>
        Lower = 1,
        /// <summary>
        /// Indicates uppercase
        /// </summary>
        Upper = 2
    }

}
