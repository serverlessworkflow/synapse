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

namespace Synapse.Integration.Models
{

	/// <summary>
	/// Represent an instance of a V1Workflow
	/// </summary>
	[DataContract]
	[Queryable]
	public partial class V1WorkflowInstance
		: Entity
	{

		/// <summary>
		/// The id of the instanciated V1Workflow.<para></para>  The workflow id is used as the first out of the two components of the V1WorkflowInstance's id
		/// </summary>
		[DataMember(Name = "workflowId", Order = 1)]
		[Description("The id of the instanciated V1Workflow.<para></para>  The workflow id is used as the first out of the two components of the V1WorkflowInstance's id")]
		public virtual string WorkflowId { get; set; }

		/// <summary>
		/// The V1WorkflowInstance's key.<para></para>  The key is used as the second out of the two components of the V1WorkflowInstance's id
		/// </summary>
		[DataMember(Name = "key", Order = 2)]
		[Description("The V1WorkflowInstance's key.<para></para>  The key is used as the second out of the two components of the V1WorkflowInstance's id")]
		public virtual string Key { get; set; }

		/// <summary>
		/// The V1WorkflowInstance's activation type
		/// </summary>
		[DataMember(Name = "activationType", Order = 3)]
		[Description("The V1WorkflowInstance's activation type")]
		public virtual V1WorkflowInstanceActivationType ActivationType { get; set; }

		/// <summary>
		/// The id of the V1WorkflowInstance's parent, if any
		/// </summary>
		[DataMember(Name = "parentId", Order = 4)]
		[Description("The id of the V1WorkflowInstance's parent, if any")]
		public virtual string ParentId { get; set; }

		/// <summary>
		/// The V1WorkflowInstance's input
		/// </summary>
		[DataMember(Name = "input", Order = 5)]
		[Description("The V1WorkflowInstance's input")]
		public virtual Dynamic Input { get; set; }

		/// <summary>
		/// An IReadOnlyCollection`1 containing descriptors of the CloudEvents that have triggered the V1WorkflowInstance
		/// </summary>
		[DataMember(Name = "triggerEvents", Order = 6)]
		[Description("An IReadOnlyCollection`1 containing descriptors of the CloudEvents that have triggered the V1WorkflowInstance")]
		public virtual ICollection<V1Event> TriggerEvents { get; set; }

		/// <summary>
		/// The V1WorkflowInstance's status
		/// </summary>
		[DataMember(Name = "status", Order = 7)]
		[Description("The V1WorkflowInstance's status")]
		public virtual V1WorkflowInstanceStatus Status { get; set; }

		/// <summary>
		/// The date and time at which the V1WorkflowInstance has started
		/// </summary>
		[DataMember(Name = "startedAt", Order = 8)]
		[Description("The date and time at which the V1WorkflowInstance has started")]
		public virtual DateTime? StartedAt { get; set; }

		/// <summary>
		/// The date and time at which the V1WorkflowInstance has been executed<para></para>  The value is set when the V1WorkflowInstance has been cancelled, faults or completes.
		/// </summary>
		[DataMember(Name = "executedAt", Order = 9)]
		[Description("The date and time at which the V1WorkflowInstance has been executed<para></para>  The value is set when the V1WorkflowInstance has been cancelled, faults or completes.")]
		public virtual DateTime? ExecutedAt { get; set; }

		/// <summary>
		/// The V1WorkflowInstance's V1CorrelationContext
		/// </summary>
		[DataMember(Name = "correlationContext", Order = 10)]
		[Description("The V1WorkflowInstance's V1CorrelationContext")]
		public virtual V1CorrelationContext CorrelationContext { get; set; }

		/// <summary>
		/// An IReadOnlyCollection`1 containing the sessions the V1WorkflowInstance is made out of
		/// </summary>
		[DataMember(Name = "sessions", Order = 11)]
		[Description("An IReadOnlyCollection`1 containing the sessions the V1WorkflowInstance is made out of")]
		public virtual ICollection<V1WorkflowRuntimeSession> Sessions { get; set; }

		/// <summary>
		/// The currently active V1WorkflowRuntimeSession, if any
		/// </summary>
		[DataMember(Name = "activeSession", Order = 12)]
		[Description("The currently active V1WorkflowRuntimeSession, if any")]
		public virtual V1WorkflowRuntimeSession ActiveSession { get; set; }

		/// <summary>
		/// An IReadOnlyCollection`1 containing the activities the V1WorkflowInstance is made out of
		/// </summary>
		[DataMember(Name = "activities", Order = 13)]
		[Description("An IReadOnlyCollection`1 containing the activities the V1WorkflowInstance is made out of")]
		public virtual ICollection<V1WorkflowActivity> Activities { get; set; }

		/// <summary>
		/// The Error that caused the V1WorkflowInstance to end prematurily
		/// </summary>
		[DataMember(Name = "error", Order = 14)]
		[Description("The Error that caused the V1WorkflowInstance to end prematurily")]
		public virtual Error Error { get; set; }

		/// <summary>
		/// The V1WorkflowInstance's output
		/// </summary>
		[DataMember(Name = "output", Order = 15)]
		[Description("The V1WorkflowInstance's output")]
		public virtual Dynamic Output { get; set; }

    }

}
