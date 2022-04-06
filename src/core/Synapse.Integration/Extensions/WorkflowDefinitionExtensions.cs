/*
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

namespace Synapse
{

    /// <summary>
    /// Defines extensions for <see cref="WorkflowDefinition"/>s
    /// </summary>
    public static class WorkflowDefinitionExtensions
    {

        /// <summary>
        /// Gets the <see cref="WorkflowDefinition"/>'s unique identifier, which is a concatenation of the <see cref="WorkflowDefinition.Id"/> and <see cref="WorkflowDefinition.Version"/> properties
        /// </summary>
        /// <param name="workflowDefinition">The <see cref="WorkflowDefinition"/> to get the unique identifier of</param>
        /// <returns>The <see cref="WorkflowDefinition"/>'s unique identifier</returns>
        public static string GetUniqueIdentifier(this WorkflowDefinition workflowDefinition)
        {
            if (string.IsNullOrWhiteSpace(workflowDefinition.Id))
                throw new InvalidDataException($"The specified workflow definition must define the 'id' property");
            if (string.IsNullOrWhiteSpace(workflowDefinition.Version))
                throw new InvalidDataException($"The specified workflow definition must define the 'version' property");
            return $"{workflowDefinition.Id}:{workflowDefinition.Version}";
        }

    }

}
