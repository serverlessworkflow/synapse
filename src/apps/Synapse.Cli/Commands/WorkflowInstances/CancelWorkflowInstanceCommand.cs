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

namespace Synapse.Cli.Commands.WorkflowInstances
{
    /// <summary>
    /// Represents the <see cref="Command"/> used to cancel the execution of a <see cref="V1WorkflowInstance"/>
    /// </summary>
    internal class CancelWorkflowInstanceCommand
        : Command
    {

        /// <summary>
        /// Gets the <see cref="CancelWorkflowInstanceCommand"/>'s name
        /// </summary>
        public const string CommandName = "cancel";
        /// <summary>
        /// Gets the <see cref="CancelWorkflowInstanceCommand"/>'s description
        /// </summary>
        public const string CommandDescription = "Cancels the execution of the specified workflow instance";

        /// <inheritdoc/>
        public CancelWorkflowInstanceCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseManagementApi synapseManagementApi)
            : base(serviceProvider, loggerFactory, synapseManagementApi, CommandName, CommandDescription)
        {
            this.Add(new Argument<string>("id") { Description = "The id of the workflow instance to cancel the execution of (ex: myworkflow-XqGg49onskelivig7ND6ig)" });
            this.Handler = CommandHandler.Create<string>(this.HandleAsync);
        }

        /// <summary>
        /// Handles the <see cref="CancelWorkflowInstanceCommand"/>
        /// </summary>
        /// <param name="id">The id of the workflow instance to cancel the execution of</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public async Task HandleAsync(string id)
        {
            await this.SynapseManagementApi.CancelWorkflowInstanceAsync(id);
            Console.WriteLine($"The execution of the workflow instance with id '{id}' has been successfully cancelled");
        }

    }

}
