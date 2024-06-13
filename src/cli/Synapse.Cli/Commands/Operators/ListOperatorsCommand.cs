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

using moment.net;
using Neuroglia.Data.Infrastructure.ResourceOriented;

namespace Synapse.Cli.Commands.Operators;

/// <summary>
/// Represents the <see cref="Command"/> used to list<see cref="Operator"/>s
/// </summary>
internal class ListOperatorsCommand
    : Command
{

    /// <summary>
    /// Gets the <see cref="ListOperatorsCommand"/>'s name
    /// </summary>
    public const string CommandName = "list";
    /// <summary>
    /// Gets the <see cref="ListOperatorsCommand"/>'s description
    /// </summary>
    public const string CommandDescription = "Lists operators";

    /// <inheritdoc/>
    public ListOperatorsCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api)
        : base(serviceProvider, loggerFactory, api, CommandName, CommandDescription)
    {
        this.AddAlias("ls");
        this.Add(CommandOptions.Namespace);
        this.Handler = CommandHandler.Create<string>(this.HandleAsync);
    }

    /// <summary>
    /// Handles the <see cref="ListOperatorsCommand"/>
    /// </summary>
    /// <param name="namespace">The namespace the operators to list belong to</param>
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
        table.AddColumn("STATUS", column =>
        {
            column.Alignment = Justify.Center;
        });
        table.AddColumn("CREATED AT", column =>
        {
            column.Alignment = Justify.Center;
        });
        await foreach (var @operator in await this.Api.Operators.ListAsync(@namespace))
        {
            isEmpty = false;
            table.AddRow
            (
                @operator.GetName(),
                @operator.GetNamespace()!,
                (@operator.Status?.Phase ?? OperatorStatusPhase.Stopped).ToUpperInvariant(),
                @operator.Metadata.CreationTimestamp?.ToOffset(DateTimeOffset.Now.Offset).DateTime.FromNow() ?? "-"
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

        public static Option<string> Namespace => new(["-n", "--namespace"], () => string.Empty, "The namespace the operators to list belong to.");

    }

}
