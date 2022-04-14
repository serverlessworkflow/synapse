
namespace Synapse.Sdk.Commands.Workflows
{

	/// <summary>
	/// Represents the ICommand used to create a new V1Workflow
	/// </summary>
	public partial class V1CreateWorkflowCommand
		: DataTransferObject
	{

		/// <summary>
		/// The WorkflowDefinition of the workflow to create
		/// </summary>
		[Description("The WorkflowDefinition of the workflow to create")]
		public virtual WorkflowDefinition Definition { get; set; }

    }

}
