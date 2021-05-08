using Newtonsoft.Json;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Describes an error
    /// </summary>
    public class V1Error
    {

        /// <summary>
        /// Initializes a new <see cref="V1Error"/>
        /// </summary>
        protected V1Error()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1Error"/>
        /// </summary>
        /// <param name="code">The error's code</param>
        /// <param name="message">The error's message</param>
        public V1Error(string code, string message)
        {
            this.Code = code;
            this.Message = message;
        }

        /// <summary>
        /// Gets the error's code
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }

        /// <summary>
        /// Gets the error's message
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

    }

}
