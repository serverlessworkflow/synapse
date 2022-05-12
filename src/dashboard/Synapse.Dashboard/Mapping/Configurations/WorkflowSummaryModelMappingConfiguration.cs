/*
 * Copyright © 2022-Present The Synapse Authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */
using AutoMapper;
using Neuroglia.Mapping;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Dashboard.Mapping.Configurations
{
    internal class WorkflowSummaryModelMappingConfiguration
        : IMappingConfiguration<WorkflowSummaryModel, WorkflowDefinition>,
        IMappingConfiguration<WorkflowDefinition, WorkflowSummaryModel>
    {

        void IMappingConfiguration<WorkflowSummaryModel, WorkflowDefinition>.Configure(IMappingExpression<WorkflowSummaryModel, WorkflowDefinition> mapping)
        {

        }

        void IMappingConfiguration<WorkflowDefinition, WorkflowSummaryModel>.Configure(IMappingExpression<WorkflowDefinition, WorkflowSummaryModel> mapping)
        {

        }

    }
}
