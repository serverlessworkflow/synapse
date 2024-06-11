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

namespace Synapse.Cli.Commands.Workflows;

/// <summary>
/// Represents the <see cref="Command"/> used to delete a single <see cref="Workflow"/>
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
    public DeleteWorkflowCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api)
        : base(serviceProvider, loggerFactory, api, CommandName, CommandDescription)
    {
        this.AddAlias("del");
        this.Add(new Argument<string>("name") { Description = "The name of the workflow to delete" });
        this.Add(CommandOptions.Namespace);
        this.Add(CommandOptions.Version);
        this.Add(CommandOptions.Confirm);
        this.Handler = CommandHandler.Create<string, string, string, bool>(this.HandleAsync);
    }

    /// <summary>
    /// Handles the <see cref="DeleteWorkflowCommand"/>
    /// </summary>
    /// <param name="namespace">The namespace of the workflow to delete</param>
    /// <param name="name">The name of the workflow to delete</param>
    /// <param name="version">The version of the workflow to delete</param>
    /// <param name="y">A boolean indicating whether or not to ask for the user's confirmation</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task HandleAsync(string name, string @namespace, string version, bool y)
    {
        if (!y)
        {
            if (string.IsNullOrWhiteSpace(version)) Console.Write($"Are you sure you wish to delete all version of the workflow '{name}.{@namespace}'? Press 'y' to confirm, or any other key to cancel: ");
            else Console.Write($"Are you sure you wish to delete the workflow '{name}.{@namespace}:{version}'? Press 'y' to confirm, or any other key to cancel: ");
            var inputKey = Console.ReadKey();
            Console.WriteLine();
            if (inputKey.Key != ConsoleKey.Y) return;
        }
        if (string.IsNullOrWhiteSpace(version))
        {
            await this.Api.Workflows.DeleteAsync(name, @namespace);
            Console.WriteLine($"workflow/{name} deleted");
        }
        else
        {
            var workflow = await this.Api.Workflows.GetAsync(name, @namespace) ?? throw new NullReferenceException($"Failed to find the specified workflow '{name}.{@namespace}'");
            var definition = workflow.Spec.Versions.FirstOrDefault(v => v.Document.Version == version) ?? throw new NullReferenceException($"Failed to find the specified workflow version '{name}.{@namespace}:{version}'");
            var originalWorkflow = workflow.Clone()!;
            workflow.Spec.Versions.Remove(definition);
            var patch = JsonPatchUtility.CreateJsonPatchFromDiff(originalWorkflow, workflow);
            await this.Api.Workflows.PatchAsync(name, @namespace, new(PatchType.JsonPatch, patch));
            Console.WriteLine($"workflow/{name}:{version} deleted");
        }
    }

    static class CommandOptions
    {

        public static Option<string> Namespace => new([ "-n", "--namespace" ], () => "default", "The namespace the workflow to delete belongs to");

        public static Option<string> Version => new(["-v", "--version"], () => string.Empty, "The version of the workflow to delete. Note that failing to specify the version will delete all version of the specified workflow");

        public static Option<bool> Confirm => new(["-y", "--yes"], () => false, "Delete the workflow(s) without prompting confirmation");

    }

}
