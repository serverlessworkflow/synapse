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

namespace Synapse.Cli.Commands.Namespaces;

/// <summary>
/// Represents the <see cref="Command"/> used to list<see cref="Namespace"/>s
/// </summary>
internal class ListNamespacesCommand
    : Command
{

    /// <summary>
    /// Gets the <see cref="ListNamespacesCommand"/>'s name
    /// </summary>
    public const string CommandName = "list";
    /// <summary>
    /// Gets the <see cref="ListNamespacesCommand"/>'s description
    /// </summary>
    public const string CommandDescription = "Lists namespaces";

    /// <inheritdoc/>
    public ListNamespacesCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api)
        : base(serviceProvider, loggerFactory, api, CommandName, CommandDescription)
    {
        this.AddAlias("ls");
        this.Handler = CommandHandler.Create(this.HandleAsync);
    }

    /// <summary>
    /// Handles the <see cref="ListNamespacesCommand"/>
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task HandleAsync()
    {
        var table = new Table();
        var isEmpty = true;
        table.Border(TableBorder.None);
        table.AddColumn("NAME");
        table.AddColumn("CREATED AT", column =>
        {
            column.Alignment = Justify.Center;
        });
        await foreach (var @namespace in await this.Api.Namespaces.ListAsync())
        {
            isEmpty = false;
            table.AddRow
            (
                @namespace.GetName(),
                @namespace.Metadata.CreationTimestamp?.ToOffset(DateTimeOffset.Now.Offset).DateTime.FromNow() ?? "-"
            );
        }
        if (isEmpty)
        {
            AnsiConsole.WriteLine($"No resource found");
            return;
        }
        AnsiConsole.Write(table);
    }

}
