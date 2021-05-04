using Synapse.Domain.Models;
using System;

namespace Synapse
{

    /// <summary>
    /// Defines extensions for <see cref="Exception"/>s
    /// </summary>
    public static class ExceptionExtensions
    {

        /// <summary>
        /// Converts the <see cref="Exception"/> into a new <see cref="V1Error"/>
        /// </summary>
        /// <param name="ex">The <see cref="Exception"/> to convert</param>
        /// <returns>A new <see cref="V1Error"/></returns>
        public static V1Error ToV1Error(this Exception ex)
        {
            if (ex == null)
                throw new ArgumentNullException(nameof(ex));
            return new V1Error(ex.GetType().Name, ex.Message);
        }

    }

}
