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
	/// Represents an event
	/// </summary>
	[DataContract]
	public partial class V1Event
	{

		/// <summary>
		/// The event's id
		/// </summary>
		[DataMember(Name = "Id", Order = 1)]
		[Description("The event's id")]
		public virtual string Id { get; set; }

		/// <summary>
		/// The event's sourceUri
		/// </summary>
		[DataMember(Name = "Source", Order = 2)]
		[Description("The event's sourceUri")]
		public virtual Uri Source { get; set; }

		/// <summary>
		/// The event's spec version
		/// </summary>
		[DataMember(Name = "SpecVersion", Order = 3)]
		[Description("The event's spec version")]
		public virtual string SpecVersion { get; set; }

		/// <summary>
		/// The event's type
		/// </summary>
		[DataMember(Name = "Type", Order = 4)]
		[Description("The event's type")]
		public virtual string Type { get; set; }

		/// <summary>
		/// The event's data content type
		/// </summary>
		[DataMember(Name = "DataContentType", Order = 5)]
		[Description("The event's data content type")]
		public virtual string DataContentType { get; set; }

		/// <summary>
		/// The event's data schema Uri, if any
		/// </summary>
		[DataMember(Name = "DataSchema", Order = 6)]
		[Description("The event's data schema Uri, if any")]
		public virtual Uri DataSchema { get; set; }

		/// <summary>
		/// The event's subject
		/// </summary>
		[DataMember(Name = "Subject", Order = 7)]
		[Description("The event's subject")]
		public virtual string Subject { get; set; }

		/// <summary>
		/// The event's type
		/// </summary>
		[DataMember(Name = "Time", Order = 8)]
		[Description("The event's type")]
		public virtual DateTime? Time { get; set; }

		/// <summary>
		/// The event's data
		/// </summary>
		[DataMember(Name = "Data", Order = 9)]
		[Description("The event's data")]
		public virtual Dynamic Data { get; set; }

		/// <summary>
		/// An IDictionary`2 that contains the event's extension key/value mappings
		/// </summary>
		[DataMember(Name = "Extensions", Order = 10)]
		[Description("An IDictionary`2 that contains the event's extension key/value mappings")]
		public virtual NameValueCollection<Dynamic> Extensions { get; set; }

    }

}
