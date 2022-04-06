
namespace Synapse.Sdk.Models
{

	/// <summary>
	/// Represents a V1Workflow's spec
	/// </summary>
	public partial class V1WorkflowSpec
	{

		/// <summary>
		/// The V1Workflow's WorkflowDefinition
		/// </summary>
		[Newtonsoft.Json.JsonProperty("definition")]
		[ProtoBuf.ProtoMember(0, Name = "definition")]
		[System.Text.Json.Serialization.JsonPropertyName("definition")]
		[YamlDotNet.Serialization.YamlMember(Alias = "definition")]
		[Description("The V1Workflow's WorkflowDefinition")]
		public virtual WorkflowDefinition Definition { get; set; }

    }

}
