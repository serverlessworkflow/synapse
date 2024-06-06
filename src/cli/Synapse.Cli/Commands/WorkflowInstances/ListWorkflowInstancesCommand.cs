// Copyright © 2024-Present Neuroglia SRL. All rights reserved.
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

namespace Synapse.Cli.Commands.WorkflowInstances;

/// <summary>
/// Represents the <see cref="Command"/> used to list<see cref="WorkflowInstance"/>s
/// </summary>
internal class ListWorkflowInstancesCommand
    : Command
{

    /// <summary>
    /// Gets the <see cref="ListWorkflowInstancesCommand"/>'s name
    /// </summary>
    public const string CommandName = "list";
    /// <summary>
    /// Gets the <see cref="ListWorkflowInstancesCommand"/>'s description
    /// </summary>
    public const string CommandDescription = "Lists workflows";

    /// <inheritdoc/>
    public ListWorkflowInstancesCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api)
        : base(serviceProvider, loggerFactory, api, CommandName, CommandDescription)
    {
        this.AddAlias("ls");
        this.Add(CommandOptions.Namespace);
        this.Handler = CommandHandler.Create<string>(this.HandleAsync);
    }

    /// <summary>
    /// Handles the <see cref="ListWorkflowInstancesCommand"/>
    /// </summary>
    /// <param name="namespace">The namespace the workflow to list belong to</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task HandleAsync(string @namespace)
    {
        var table = new Table();
        var isEmpty = true;
        table.Border(TableBorder.None);
        table.AddColumn("NAME", column =>
        {
            column.NoWrap = true;
        });
        table.AddColumn("NAMESPACE", column =>
        {
            column.NoWrap = true;
        });
        table.AddColumn("DEFINITION", column =>
        {
            column.NoWrap = true;
        });
        table.AddColumn("STATUS", column =>
        {
            column.Alignment = Justify.Center;
        });
        table.AddColumn("CREATED AT", column =>
        {
            column.Alignment = Justify.Center;
        });
        table.AddColumn("STARTED AT", column =>
        {
            column.Alignment = Justify.Center;
        });
        table.AddColumn("ENDED AT", column =>
        {
            column.Alignment = Justify.Center;
        });
        table.AddColumn("DURATION", column =>
        {
            column.Alignment = Justify.Center;
        });
        table.AddColumn("OPERATOR", column =>
        {
            column.NoWrap = true;
            column.Alignment = Justify.Center;
        });
        await foreach (var workflow in await this.Api.WorkflowInstances.ListAsync(@namespace))
        {
            isEmpty = false;
            table.AddRow
            (
                workflow.GetName(),
                workflow.GetNamespace()!,
                workflow.Spec.Definition.ToString(),
                (workflow.Status?.Phase ?? WorkflowInstanceStatusPhase.Pending).ToUpperInvariant(),
                workflow.Metadata.CreationTimestamp.ToString()!,
                workflow.Status?.StartedAt?.ToString() ?? "-",
                workflow.Status?.EndedAt?.ToString() ?? "-",
                workflow.Status?.StartedAt.HasValue == true && workflow.Status?.EndedAt.HasValue == true ? workflow.Status.EndedAt.Value.Subtract(workflow.Status.StartedAt.Value).ToString("hh\\:mm\\:ss\\.fff") : "-",
                workflow.Metadata.Labels?.TryGetValue(SynapseDefaults.Resources.Labels.Operator, out var operatorName) == true && !string.IsNullOrWhiteSpace(operatorName) ? operatorName : "-"
            );
        }
        if(isEmpty)
        {
            AnsiConsole.WriteLine(string.IsNullOrWhiteSpace(@namespace) ? "No resource found" : $"No resource found in {@namespace} namespace");
            return;
        }
        AnsiConsole.Write(table);
    }

    static class CommandOptions
    {

        public static Option<string> Namespace => new(["-n", "--namespace"], () => string.Empty, "The namespace the workflow instances to list belong to.");

    }

}
