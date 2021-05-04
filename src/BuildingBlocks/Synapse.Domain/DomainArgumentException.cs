using System;

namespace Synapse.Domain
{
    /// <summary>
    /// Represents an argument-related <see cref="DomainException"/>
    /// </summary>
    public class DomainArgumentException
       : DomainException
    {

        /// <summary>
        /// Initializes a new <see cref="DomainArgumentException"/>
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="argumentName">The argument name</param>
        public DomainArgumentException(string message, string argumentName)
            : base(message)
        {
            this.ArgumentName = argumentName;
        }

        /// <summary>
        /// Initializes a new <see cref="DomainArgumentException"/>
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="argumentName">The argument name</param>
        /// <param name="innerException">The inner exception, if any</param>
        public DomainArgumentException(string message, string argumentName, Exception innerException)
            : base(message, innerException)
        {
            this.ArgumentName = argumentName;
        }

        /// <summary>
        /// Gets the name of the argument at the origin of the exception
        /// </summary>
        public string ArgumentName { get; private set; }

    }

}
