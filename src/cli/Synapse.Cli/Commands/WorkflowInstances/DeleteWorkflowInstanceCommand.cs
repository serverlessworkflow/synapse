// Copyright © 2024-Present The Synapse Authors
//
// Licensed under the Apache License, Version 2.0 (the "License"),
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Synapse.Cli.Commands.WorkflowInstances;

/// <summary>
/// Represents the <see cref="Command"/> used to delete a single <see cref="WorkflowInstance"/>
/// </summary>
internal class DeleteWorkflowInstanceCommand
    : Command
{

    /// <summary>
    /// Gets the <see cref="DeleteWorkflowInstanceCommand"/>'s name
    /// </summary>
    public const string CommandName = "delete";
    /// <summary>
    /// Gets the <see cref="DeleteWorkflowInstanceCommand"/>'s description
    /// </summary>
    public const string CommandDescription = "Deletes the specified workflow instance";

    /// <inheritdoc/>
    public DeleteWorkflowInstanceCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api)
        : base(serviceProvider, loggerFactory, api, CommandName, CommandDescription)
    {
        this.AddAlias("del");
        this.Add(new Argument<string>("name") { Description = "The name of the workflow instance to delete" });
        this.Add(CommandOptions.Namespace);
        this.Add(CommandOptions.Confirm);
        this.Handler = CommandHandler.Create<string, string, bool>(this.HandleAsync);
    }

    /// <summary>
    /// Handles the <see cref="DeleteWorkflowInstanceCommand"/>
    /// </summary>
    /// <param name="namespace">The namespace of the workflow to delete</param>
    /// <param name="name">The name of the workflow to delete</param>
    /// <param name="y">A boolean indicating whether or not to ask for the user's confirmation</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task HandleAsync(string name, string @namespace, bool y)
    {
        if (!y)
        {
            Console.Write($"Are you sure you wish to delete the workflow instance '{name}.{@namespace}'? Press 'y' to confirm, or any other key to cancel: ");
            var inputKey = Console.ReadKey();
            Console.WriteLine();
            if (inputKey.Key != ConsoleKey.Y) return;
        }
        await this.Api.WorkflowInstances.DeleteAsync(name, @namespace);
        Console.WriteLine($"workflow-instance/{name} deleted");
    }

    static class CommandOptions
    {

        public static Option<string> Namespace => new([ "-n", "--namespace" ], () => "default", "The namespace the workflow instance to delete belongs to.");

        public static Option<bool> Confirm => new(["-y", "--yes"], () => false, "Delete the workflow instance without prompting confirmation.");

    }

}
