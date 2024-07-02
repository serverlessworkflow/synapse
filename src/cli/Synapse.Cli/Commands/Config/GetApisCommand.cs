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

using Microsoft.Extensions.Options;
using Synapse.Cli.Configuration;
using Synapse.Cli.Services;
using Synapse.Resources;

namespace Synapse.Cli.Commands.Config;

/// <summary>
/// Represents the <see cref="Command"/> used to configure the API used by the Synapse CLI
/// </summary>
internal class GetApisCommand
    : Command
{

    /// <summary>
    /// Gets the <see cref="GetApisCommand"/>'s name
    /// </summary>
    public const string CommandName = "get-apis";
    /// <summary>
    /// Gets the <see cref="GetApisCommand"/>'s description
    /// </summary>
    public const string CommandDescription = "Retrieves all configured APIs";

    /// <inheritdoc/>
    public GetApisCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api, IOptionsManager optionsManager, IOptionsMonitor<ApplicationOptions> applicationOptions)
        : base(serviceProvider, loggerFactory, api, CommandName, CommandDescription)
    {
        this.OptionsManager = optionsManager;
        this.ApplicationOptions = applicationOptions;
        this.Handler = CommandHandler.Create(HandleAsync);
    }

    /// <summary>
    /// Gets the service used to manage the application's options
    /// </summary>
    protected IOptionsManager OptionsManager { get; }

    /// <summary>
    /// Gets the current <see cref="ApplicationOptions"/>
    /// </summary>
    protected IOptionsMonitor<ApplicationOptions> ApplicationOptions { get; }

    /// <summary>
    /// Handles the <see cref="GetApisCommand"/>
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task HandleAsync()
    {
        var table = new Table();
        table.Border(TableBorder.None);
        table.AddColumn("CURRENT");
        table.AddColumn("NAME");
        foreach (var apiConfig in this.ApplicationOptions.CurrentValue.Api.Configurations)
        {
            table.AddRow
            (
                this.ApplicationOptions.CurrentValue.Api.Current == apiConfig.Key || this.ApplicationOptions.CurrentValue.Api.Configurations.Count == 1 ? "*" : string.Empty,
                apiConfig.Key
            );
        }
        AnsiConsole.Write(table);
        await Task.CompletedTask;
    }

    static class CommandOptions
    {

        public static Option<Uri> Server => new(["-s", "--server"], "The address of the API server to use");

    }

}