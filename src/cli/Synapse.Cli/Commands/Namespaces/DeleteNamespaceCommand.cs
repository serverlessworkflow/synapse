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

using Neuroglia.Data;
using Neuroglia.Data.Infrastructure.ResourceOriented;

namespace Synapse.Cli.Commands.Namespaces;

/// <summary>
/// Represents the <see cref="Command"/> used to delete a single <see cref="Namespace"/>
/// </summary>
internal class DeleteNamespaceCommand
    : Command
{

    /// <summary>
    /// Gets the <see cref="DeleteNamespaceCommand"/>'s name
    /// </summary>
    public const string CommandName = "delete";
    /// <summary>
    /// Gets the <see cref="DeleteNamespaceCommand"/>'s description
    /// </summary>
    public const string CommandDescription = "Deletes the specified namespace";

    /// <inheritdoc/>
    public DeleteNamespaceCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api)
        : base(serviceProvider, loggerFactory, api, CommandName, CommandDescription)
    {
        this.AddAlias("del");
        this.Add(new Argument<string>("name") { Description = "The name of the namespace to delete" });
        this.Add(CommandOptions.Confirm);
        this.Handler = CommandHandler.Create<string, bool>(this.HandleAsync);
    }

    /// <summary>
    /// Handles the <see cref="DeleteNamespaceCommand"/>
    /// </summary>
    /// <param name="name">The name of the namespace to delete</param>
    /// <param name="y">A boolean indicating whether or not to ask for the user's confirmation</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task HandleAsync(string name, bool y)
    {
        if (!y)
        {
            Console.Write($"Are you sure you wish to delete the namespace '{name}'? Press 'y' to confirm, or any other key to cancel: ");
            var inputKey = Console.ReadKey();
            Console.WriteLine();
            if (inputKey.Key != ConsoleKey.Y) return;
        }
        await this.Api.Namespaces.DeleteAsync(name);
        Console.WriteLine($"namespace/{name} deleted");
    }

    static class CommandOptions
    {

        public static Option<bool> Confirm => new(["-y", "--yes"], () => false, "Delete the @namespace(s) without prompting confirmation");

    }

}
