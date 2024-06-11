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

using Neuroglia.Data.Infrastructure.ResourceOriented;

namespace Synapse.Cli.Commands.Workflows;

/// <summary>
/// Represents the <see cref="Command"/> used to list<see cref="Workflow"/>s
/// </summary>
internal class ListWorkflowsCommand
    : Command
{

    /// <summary>
    /// Gets the <see cref="ListWorkflowsCommand"/>'s name
    /// </summary>
    public const string CommandName = "list";
    /// <summary>
    /// Gets the <see cref="ListWorkflowsCommand"/>'s description
    /// </summary>
    public const string CommandDescription = "Lists workflows";

    /// <inheritdoc/>
    public ListWorkflowsCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api)
        : base(serviceProvider, loggerFactory, api, CommandName, CommandDescription)
    {
        this.AddAlias("ls");
        this.Add(CommandOptions.Namespace);
        this.Handler = CommandHandler.Create<string>(this.HandleAsync);
    }

    /// <summary>
    /// Handles the <see cref="ListWorkflowsCommand"/>
    /// </summary>
    /// <param name="namespace">The namespace the workflow to list belong to</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task HandleAsync(string @namespace)
    {
        var table = new Table();
        var isEmpty = true;
        table.Border(TableBorder.None);
        table.AddColumn("NAME");
        table.AddColumn("NAMESPACE");
        table.AddColumn("LATEST", column =>
        {
            column.Alignment = Justify.Center;
        });
        table.AddColumn("VERSIONS", column =>
        {
            column.Alignment = Justify.Center;
        });
        table.AddColumn("SCHEDULE", column =>
        {
            column.Alignment = Justify.Center;
        });
        table.AddColumn("OPERATOR", column =>
        {
            column.Alignment = Justify.Center;
        });
        await foreach (var workflow in await this.Api.Workflows.ListAsync(@namespace))
        {
            isEmpty = false;
            table.AddRow
            (
                workflow.GetName(),
                workflow.GetNamespace()!,
                workflow.Spec.Versions.GetLatest().Document.Version,
                workflow.Spec.Versions.Count.ToString(),
                workflow.Spec.Versions.GetLatest().Schedule == null 
                    ? "-" 
                    : workflow.Spec.Versions.GetLatest().Schedule?.After?.ToString()
                    ?? workflow.Spec.Versions.GetLatest().Schedule?.Cron
                    ?? "events",
                workflow.Metadata.Labels?.TryGetValue(SynapseDefaults.Resources.Labels.Operator, out var @operator) == true ? @operator : "-"
            );
        }
        if (isEmpty)
        {
            AnsiConsole.WriteLine(string.IsNullOrWhiteSpace(@namespace) ? "No resource found" : $"No resource found in {@namespace} namespace");
            return;
        }
        AnsiConsole.Write(table);
    }

    static class CommandOptions
    {

        public static Option<string> Namespace => new(["-n", "--namespace"], () => string.Empty, "The namespace the workflow to list belong to.");

    }

}
