using System;

namespace Synapse.Domain
{
    /// <summary>
    /// Represents a <see cref="DomainException"/> thrown when the application failed to resolve a reference to an object
    /// </summary>
    public class DomainNullReferenceException
        : DomainException
    {

        /// <summary>
        /// Initializes a new <see cref="DomainNullReferenceException"/>
        /// </summary>
        /// <param name="message">The <see cref="Exception"/> message</param>
        public DomainNullReferenceException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Initializes a new <see cref="DomainNullReferenceException"/>
        /// </summary>
        /// <param name="message">The <see cref="Exception"/> message</param>
        /// <param name="innerException">The inner <see cref="Exception"/></param>
        public DomainNullReferenceException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

    }

}
