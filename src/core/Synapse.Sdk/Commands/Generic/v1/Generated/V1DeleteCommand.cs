
namespace Synapse.Sdk.Commands.Generic
{

	/// <summary>
	/// Represents the ICommand used to delete an existing IAggregateRoot by key
	/// </summary>
	public partial class V1DeleteCommand
		: DataTransferObject
	{

		/// <summary>
		/// The key of the entity to delete
		/// </summary>
		[Description("The key of the entity to delete")]
		public virtual object Key { get; set; }

    }

}
