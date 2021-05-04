using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Synapse.Domain.Models;
using System.Runtime.Serialization;

namespace Synapse
{

    /// <summary>
    /// Enumerates all possible types of <see cref="V1WorkflowExecutionResult"/>
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum V1WorkflowExecutionResultType
    {
        /// <summary>
        /// Indicates that the <see cref="V1WorkflowActivity"/> should proceed to the next <see cref="V1WorkflowActivity"/>
        /// </summary>
        [EnumMember(Value = "NEXT")]
        Next,
        /// <summary>
        /// Indicates the <see cref="V1WorkflowActivity"/> ends the <see cref="V1Workflow"/>'s execution
        /// </summary>
        [EnumMember(Value = "END")]
        End,
        /// <summary>
        /// Indicates that the <see cref="V1WorkflowActivity"/> has been terminated
        /// </summary>
        [EnumMember(Value = "TERMINATE")]
        Terminate
    }

}
