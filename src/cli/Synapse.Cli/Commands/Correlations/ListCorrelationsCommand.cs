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

namespace Synapse.Cli.Commands.Correlations;

/// <summary>
/// Represents the <see cref="Command"/> used to list<see cref="Correlation"/>s
/// </summary>
internal class ListCorrelationsCommand
    : Command
{

    /// <summary>
    /// Gets the <see cref="ListCorrelationsCommand"/>'s name
    /// </summary>
    public const string CommandName = "list";
    /// <summary>
    /// Gets the <see cref="ListCorrelationsCommand"/>'s description
    /// </summary>
    public const string CommandDescription = "Lists correlations";

    /// <inheritdoc/>
    public ListCorrelationsCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api)
        : base(serviceProvider, loggerFactory, api, CommandName, CommandDescription)
    {
        this.AddAlias("ls");
        this.Add(CommandOptions.Namespace);
        this.Handler = CommandHandler.Create(this.HandleAsync);
    }

    /// <summary>
    /// Handles the <see cref="ListCorrelationsCommand"/>
    /// </summary>
    /// <param name="namespace">The namespace the workflow to list belong to</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task HandleAsync(string @namespace)
    {
        var table = new Table();
        var isEmpty = true;
        table.Border(TableBorder.None);
        table.AddColumn("NAME");
        table.AddColumn("CREATED AT", column =>
        {
            column.Alignment = Justify.Center;
        });
        await foreach (var correlation in await this.Api.Correlations.ListAsync(@namespace))
        {
            isEmpty = false;
            table.AddRow
            (
                correlation.GetName(),
                correlation.Metadata.CreationTimestamp.ToString()!
            );
        }
        if (isEmpty)
        {
            AnsiConsole.WriteLine($"No resource found");
            return;
        }
        AnsiConsole.Write(table);
    }

    static class CommandOptions
    {

        public static Option<string> Namespace => new(["-n", "--namespace"], () => string.Empty, "The namespace the correlation to list belong to.");

    }

}
