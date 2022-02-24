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

namespace Synapse.Integration.Events.WorkflowInstances
{

	/// <summary>
	/// Represents the IDomainEvent fired whenever a new V1WorkflowInstance has been created
	/// </summary>
	[DataContract]
	public partial class V1WorkflowInstanceCreatedIntegrationEvent
		: V1IntegrationEvent
	{

		/// <summary>
		/// The id of the instanciated V1Workflow
		/// </summary>
		[DataMember(Name = "WorkflowId", Order = 1)]
		[Description("The id of the instanciated V1Workflow")]
		public virtual string WorkflowId { get; set; }

		/// <summary>
		/// The key of the newly created V1WorkflowInstance
		/// </summary>
		[DataMember(Name = "Key", Order = 2)]
		[Description("The key of the newly created V1WorkflowInstance")]
		public virtual string Key { get; set; }

		/// <summary>
		/// The type of the V1WorkflowInstance's activation
		/// </summary>
		[DataMember(Name = "ActivationType", Order = 3)]
		[Description("The type of the V1WorkflowInstance's activation")]
		public virtual V1WorkflowInstanceActivationType ActivationType { get; set; }

		/// <summary>
		/// The newly created V1WorkflowInstance's input data
		/// </summary>
		[DataMember(Name = "Input", Order = 4)]
		[Description("The newly created V1WorkflowInstance's input data")]
		public virtual Any Input { get; set; }

		/// <summary>
		/// An IEnumerable`1 containing the newly created V1WorkflowInstance's trigger V1CloudEvents
		/// </summary>
		[DataMember(Name = "TriggerEvents", Order = 5)]
		[Description("An IEnumerable`1 containing the newly created V1WorkflowInstance's trigger V1CloudEvents")]
		public virtual IEnumerable<V1CloudEventDto> TriggerEvents { get; set; }

    }

}