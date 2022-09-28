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
	/// Represents an object used to filter events
	/// </summary>
	[DataContract]
	public partial class V1EventFilter
	{

		/// <summary>
		/// An IDictionary`2 containing the attributes to filter V1Events by
		/// </summary>
		[DataMember(Name = "attributes", Order = 1)]
		[Description("An IDictionary`2 containing the attributes to filter V1Events by")]
		public virtual NameValueCollection<string> Attributes { get; set; }

		/// <summary>
		/// An IReadOnlyDictionary`2 containing the attributes key/value to use when correlating an incoming event to the V1Correlation
		/// </summary>
		[DataMember(Name = "correlationMappings", Order = 2)]
		[Description("An IReadOnlyDictionary`2 containing the attributes key/value to use when correlating an incoming event to the V1Correlation")]
		public virtual NameValueCollection<string> CorrelationMappings { get; set; }

    }

}
