﻿
/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0(the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

/* -----------------------------------------------------------------------
 * This file has been automatically generated by a tool
 * -----------------------------------------------------------------------
 */

namespace Synapse.Integration.Events.FunctionDefinitionCollections
{

	/// <summary>
	/// Represents the IDomainEvent fired whenever a new V1FunctionDefinitionCollection has been deleted
	/// </summary>
	[DataContract]
	public partial class V1FunctionDefinitionCollectionDeletedIntegrationEvent
		: V1IntegrationEvent
	{

		/// <summary>
		/// Gets the id of the aggregate that has produced the event
		/// </summary>
		[DataMember(Name = "AggregateId", Order = 1)]
		[Description("Gets the id of the aggregate that has produced the event")]
		public virtual string AggregateId { get; set; }

		/// <summary>
		/// Gets the date and time at which the event has been produced
		/// </summary>
		[DataMember(Name = "CreatedAt", Order = 2)]
		[Description("Gets the date and time at which the event has been produced")]
		public virtual DateTime CreatedAt { get; set; }

    }

}
