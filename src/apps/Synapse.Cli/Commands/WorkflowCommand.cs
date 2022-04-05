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

using Microsoft.Extensions.DependencyInjection;
using Synapse.Cli.Commands.Workflows;

namespace Synapse.Cli.Commands
{
    /// <summary>
    /// Represents the <see cref="Command"/> used to manage <see cref="V1Workflow"/>s
    /// </summary>
    public class WorkflowCommand
        : Command
    {

        /// <summary>
        /// Gets the <see cref="WorkflowCommand"/>'s name
        /// </summary>
        public const string CommandName = "workflow";
        /// <summary>
        /// Gets the <see cref="WorkflowCommand"/>'s description
        /// </summary>
        public const string CommandDescription = "Manages workflows";

        /// <inheritdoc/>
        public WorkflowCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseManagementApi synapseManagementApi)
            : base(serviceProvider, loggerFactory, synapseManagementApi, CommandName, CommandDescription)
        {
            this.AddAlias("workflows");
            this.AddAlias("wf");
            this.AddCommand(ActivatorUtilities.CreateInstance<DeployWorkflowCommand>(this.ServiceProvider));
            this.AddCommand(ActivatorUtilities.CreateInstance<RunWorkflowCommand>(this.ServiceProvider));
            this.AddCommand(ActivatorUtilities.CreateInstance<GetWorkflowCommand>(this.ServiceProvider));
            this.AddCommand(ActivatorUtilities.CreateInstance<ListWorkflowsCommand>(this.ServiceProvider));
            this.AddCommand(ActivatorUtilities.CreateInstance<DeleteWorkflowCommand>(this.ServiceProvider));
        }

    }

}
