
namespace Synapse.Sdk.Commands.Generic
{

	/// <summary>
	/// Represents the ICommand used to patch an existing IAggregateRoot
	/// </summary>
	public partial class V1PatchCommand
		: DataTransferObject
	{

		/// <summary>
		/// The key of the entity to patch
		/// </summary>
		[Description("The key of the entity to patch")]
		public virtual object Key { get; set; }

		/// <summary>
		/// The JsonPatchDocument`1 to apply
		/// </summary>
		[Description("The JsonPatchDocument`1 to apply")]
		public virtual JsonPatchDocument Patch { get; set; }

    }

}
