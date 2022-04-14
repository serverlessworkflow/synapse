namespace Synapse.Integration.Commands.Generic
{

    /// <summary>
    /// Represents the ICommand used to patch an existing IAggregateRoot
    /// </summary>
	/// <typeparam name="TAggregate">The type of the aggregate to patch</typeparam>
	/// <typeparam name="TKey">The type of key used to uniquely identify the object to patch</typeparam>
    [DataContract]
	public partial class V1PatchCommand<TAggregate, TKey>
		: Command
		where TAggregate : class
	{

		/// <summary>
		/// The id of the entity to patch
		/// </summary>
		[DataMember(Name = "Id", Order = 1)]
		[Description("The id of the entity to patch")]
		public virtual TKey Id { get; set; }

		/// <summary>
		/// The JsonPatchDocument`1 to apply
		/// </summary>
		[DataMember(Name = "Patch", Order = 2)]
		[Description("The JsonPatchDocument`1 to apply")]
		public virtual JsonPatchDocument<TAggregate> Patch { get; set; }

	}

}
