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
	/// Represents a managed FunctionDefinition collection
	/// </summary>
	[DataContract]
	[Queryable]
	public partial class V1FunctionDefinitionCollection
		: Entity
	{

		/// <summary>
		/// The V1FunctionDefinitionCollection's name
		/// </summary>
		[DataMember(Name = "name", Order = 1)]
		[Description("The V1FunctionDefinitionCollection's name")]
		public virtual string Name { get; set; }

		/// <summary>
		/// The V1FunctionDefinitionCollection's version
		/// </summary>
		[DataMember(Name = "version", Order = 2)]
		[Description("The V1FunctionDefinitionCollection's version")]
		public virtual string Version { get; set; }

		/// <summary>
		/// The V1FunctionDefinitionCollection's description
		/// </summary>
		[DataMember(Name = "description", Order = 3)]
		[Description("The V1FunctionDefinitionCollection's description")]
		public virtual string Description { get; set; }

		/// <summary>
		/// An IReadOnlyCollection`1 containing the FunctionDefinitions the V1FunctionDefinitionCollection is made out of
		/// </summary>
		[DataMember(Name = "functions", Order = 4)]
		[Description("An IReadOnlyCollection`1 containing the FunctionDefinitions the V1FunctionDefinitionCollection is made out of")]
		public virtual ICollection<FunctionDefinition> Functions { get; set; }

    }

}