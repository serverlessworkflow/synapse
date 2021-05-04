using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents the result of a <see cref="V1WorkflowActivity"/>'s execution
    /// </summary>
    public class V1WorkflowExecutionResult
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowExecutionResult"/>
        /// </summary>
        protected V1WorkflowExecutionResult()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowExecutionResult"/>
        /// </summary>
        /// <param name="type">The <see cref="V1WorkflowExecutionResult"/>'s type</param>
        public V1WorkflowExecutionResult(V1WorkflowExecutionResultType type)
        {
            this.Type = type;
        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowExecutionResult"/>
        /// </summary>
        /// <param name="type">The <see cref="V1WorkflowExecutionResult"/>'s type</param>
        /// <param name="output">The <see cref="V1WorkflowExecutionResult"/>'s output</param>
        public V1WorkflowExecutionResult(V1WorkflowExecutionResultType type, JToken output)
            : this(type)
        {
            this.Output = output;
        }

        /// <summary>
        /// Gets the <see cref="V1WorkflowExecutionResult"/>'s type
        /// </summary>
        [JsonProperty("type")]
        public V1WorkflowExecutionResultType Type { get; set; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowActivity"/>'s output
        /// </summary>
        [JsonProperty("output")]
        public JToken Output { get; set; }

        /// <summary>
        /// Creates a new <see cref="V1WorkflowExecutionResult"/> of type <see cref="V1WorkflowExecutionResultType.Next"/>
        /// </summary>
        /// <param name="output">The data to output</param>
        /// <returns>A new <see cref="V1WorkflowExecutionResult"/></returns>
        public static V1WorkflowExecutionResult Next(JToken output = null)
        {
            return new V1WorkflowExecutionResult(V1WorkflowExecutionResultType.Next, output);
        }

        /// <summary>
        /// Creates a new <see cref="V1WorkflowExecutionResult"/> of type <see cref="V1WorkflowExecutionResultType.End"/>
        /// </summary>
        /// <param name="output">The data to output</param>
        /// <returns>A new <see cref="V1WorkflowExecutionResult"/></returns>
        public static V1WorkflowExecutionResult End(JToken output)
        {
            return new V1WorkflowExecutionResult(V1WorkflowExecutionResultType.End, output);
        }

        /// <summary>
        /// Creates a new <see cref="V1WorkflowExecutionResult"/> of type <see cref="V1WorkflowExecutionResultType.Terminate"/>
        /// </summary>
        /// <returns>A new <see cref="V1WorkflowExecutionResult"/></returns>
        public static V1WorkflowExecutionResult Terminate()
        {
            return new V1WorkflowExecutionResult(V1WorkflowExecutionResultType.Terminate);
        }

    }

}
