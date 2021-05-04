using Synapse.Domain.Models;
using System.Collections.Generic;

namespace Synapse.Domain
{
    /// <summary>
    /// Represents a <see cref="DomainException"/> thrown whenever the validation of an entity has failed
    /// </summary>
    public class DomainValidationException
        : DomainException
    {

        /// <summary>
        /// Initializes a new <see cref="DomainValidationException"/>
        /// </summary>
        /// <param name="errors">An array containing the <see cref="V1Error"/>s that describe the validation failures</param>
        public DomainValidationException(params V1Error[] errors)
        {
            this.ValidationErrors = errors;
        }

        /// <summary>
        /// Gets an <see cref="IReadOnlyCollection{T}"/> containing the <see cref="V1Error"/>s that describe the validation failures
        /// </summary>
        public IReadOnlyCollection<V1Error> ValidationErrors { get; }

    }

}
