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

namespace Synapse.Cli.Commands.Workflows
{

    /// <summary>
    /// Represents the <see cref="Command"/> used to delete a single <see cref="V1Workflow"/>
    /// </summary>
    internal class DeleteWorkflowCommand
        : Command
    {

        /// <summary>
        /// Gets the <see cref="DeleteWorkflowCommand"/>'s name
        /// </summary>
        public const string CommandName = "delete";
        /// <summary>
        /// Gets the <see cref="DeleteWorkflowCommand"/>'s description
        /// </summary>
        public const string CommandDescription = "Deletes the specified workflow";

        /// <inheritdoc/>
        public DeleteWorkflowCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseManagementApi synapseManagementApi) 
            : base(serviceProvider, loggerFactory, synapseManagementApi, CommandName, CommandDescription)
        {
            this.AddAlias("del");
            this.Add(new Argument<string>("id") { Description = "The id of the workflow to delete (ex: myworkflow:1.0). Note that failing to specify the version will delete all version of the specified workflow" });
            this.Add(CommandOptions.Confirm);
            this.Handler = CommandHandler.Create<string, bool>(this.HandleAsync);
        }

        /// <summary>
        /// Handles the <see cref="DeleteWorkflowCommand"/>
        /// </summary>
        /// <param name="id">The id of the workflow to delete</param>
        /// <param name="y">A boolean indicating whether or not to ask for the user's confirmation</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public async Task HandleAsync(string id, bool y)
        {
            var components = id.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (!y)
                {
                    if (components.Length == 2)
                        Console.Write($"Are you sure you wish to delete the workflow with id '{id}'? Press 'y' to confirm, or any other key to cancel: ");
                    else
                        Console.Write($"Are you sure you wish to delete all version of the workflow with id '{id}'? Press 'y' to confirm, or any other key to cancel: ");
                    var inputKey = Console.ReadKey();
                    Console.WriteLine();
                    if (inputKey.Key != ConsoleKey.Y)
                    {
                        Console.WriteLine("Deletion cancelled");
                        return;
                    }
                }
            await this.SynapseManagementApi.DeleteWorkflowAsync(id);
            if (components.Length == 2)
                Console.WriteLine($"The workflow with id '{id}' has been successfully deleted");
            else
                Console.WriteLine($"All version of the workflow with id '{id}' have been successfully deleted");
        }

        private static class CommandOptions
        {

            public static Option<bool> Confirm
            {
                get
                {
                    var option = new Option<bool>("-y", () => false)
                    {
                        Description = "Delete the workflow(s) without prompting confirmation"
                    };
                    return option;
                }
            }

        }

    }

}
