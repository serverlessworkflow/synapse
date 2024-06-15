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

namespace Synapse.Cli.Commands.Config;

/// <summary>
/// Represents the <see cref="Command"/> used to configure the API used by the Synapse CLI
/// </summary>
internal class SetApiCommand
    : Command
{

    /// <summary>
    /// Gets the <see cref="SetApiCommand"/>'s name
    /// </summary>
    public const string CommandName = "set-api";
    /// <summary>
    /// Gets the <see cref="SetApiCommand"/>'s description
    /// </summary>
    public const string CommandDescription = "Configures the API used by the Synapse CLI";

    /// <inheritdoc/>
    public SetApiCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api, IOptionsManager optionsManager, IOptionsMonitor<ApplicationOptions> applicationOptions)
        : base(serviceProvider, loggerFactory, api, CommandName, CommandDescription)
    {
        this.OptionsManager = optionsManager;
        this.ApplicationOptions = applicationOptions;
        this.Add(new Argument<string>("name") { Description = "The name of the API configuration to create or update." });
        this.Add(CommandOptions.Server);
        this.Add(CommandOptions.Token);
        this.Handler = CommandHandler.Create<string, Uri, string>(HandleAsync);
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
    /// Handles the <see cref="SetApiCommand"/>
    /// </summary>
    /// <param name="name">The name of the API configuration to update</param>
    /// <param name="server">The uri of the API server to use</param>
    /// <param name="token">The token used to authenticate on the API server</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task HandleAsync(string name, Uri server, string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(server);
        if (!this.ApplicationOptions.CurrentValue.Api.Configurations.TryGetValue(name, out var apiConfig) || apiConfig == null) apiConfig = new ApiConfiguration()
        {
            Server = server,
            Token = token
        };
        apiConfig.Server = server;
        apiConfig.Token = token;
        this.ApplicationOptions.CurrentValue.Api.Configurations[name] = apiConfig;
        if (this.ApplicationOptions.CurrentValue.Api.Configurations.Count == 1) this.ApplicationOptions.CurrentValue.Api.Current = name;
        await this.OptionsManager.UpdateOptionsAsync(this.ApplicationOptions.CurrentValue);
    }

    static class CommandOptions
    {

        public static Option<Uri> Server => new(["-s", "--server"], "The address of the API server to use");

        public static Option<string> Token => new(["-t", "--token"], "The token used to authenticate on the API server");

    }

}