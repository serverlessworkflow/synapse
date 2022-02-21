
namespace Synapse.Sdk.Events.Workflows
{

	/// <summary>
	/// Represents the IDomainEvent fired whenever a new V1Workflow has been created
	/// </summary>
	public partial class V1WorkflowCreatedIntegrationEvent
		: IntegrationEvent
	{

		/// <summary>
		/// The WorkflowDefinition of the newly created V1Workflow
		/// </summary>
		[Description("The WorkflowDefinition of the newly created V1Workflow")]
		public virtual WorkflowDefinition Definition { get; set; }

    }

}
