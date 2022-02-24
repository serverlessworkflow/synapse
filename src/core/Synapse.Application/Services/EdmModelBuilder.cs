﻿/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
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
 *
 */
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Integration.Models;
using System.Dynamic;

namespace Synapse.Application.Services
{

    /// <summary>
    /// Represents the default implementation of the <see cref="IEdmModelBuilder"/>
    /// </summary>
    public class EdmModelBuilder
        : IEdmModelBuilder
    {

        /// <inheritdoc/>
        public virtual IEdmModel Build()
        {
            ODataConventionModelBuilder builder = new();
            builder.EnableLowerCamelCase();

            builder.EntitySet<V1WorkflowDto>("V1Workflows"); //todo: fix name?
            builder.EntitySet<V1WorkflowInstanceDto>("V1WorkflowInstances"); //todo: fix name?
            builder.EntitySet<V1WorkflowActivityDto>("V1WorkflowActivities"); //todo: fix name?

            builder.AddComplexType(typeof(Any));
            builder.AddComplexType(typeof(ExpandoObject));
            builder.AddComplexType(typeof(WorkflowDefinition));
            builder.AddComplexType(typeof(NameValueCollection<string>));

            return builder.GetEdmModel();
        }

    }

}