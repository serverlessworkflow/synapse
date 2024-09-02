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
/// Represents the <see cref="Command"/> used to configure the CLI to delete the specified API configuration
/// </summary>
internal class DeleteApiCommand
    : Command
{

    /// <summary>
    /// Gets the <see cref="UseApiCommand"/>'s name
    /// </summary>
    public const string CommandName = "delete-api";
    /// <summary>
    /// Gets the <see cref="UseApiCommand"/>'s description
    /// </summary>
    public const string CommandDescription = "Deletes the API configuration with the specified name.";

    /// <inheritdoc/>
    public DeleteApiCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api, IOptionsManager optionsManager, IOptionsMonitor<ApplicationOptions> applicationOptions)
        : base(serviceProvider, loggerFactory, api, CommandName, CommandDescription)
    {
        this.OptionsManager = optionsManager;
        this.ApplicationOptions = applicationOptions;
        this.Add(new Argument<string>("name") { Description = "The name of the API configuration to delete." });
        this.Handler = CommandHandler.Create<string>(HandleAsync);
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
    /// Handles the <see cref="DeleteApiCommand"/>
    /// </summary>
    /// <param name="name">The name of the API configuration to use</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task HandleAsync(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (this.ApplicationOptions.CurrentValue.Api.Current == name) throw new NotSupportedException($"Failed to delete the API configuration with name '{name}' because it is the API currently in use.");
        if (!this.ApplicationOptions.CurrentValue.Api.Configurations.Remove(name)) throw new NullReferenceException($"Failed to find a configured API with name '{name}'.");
        await this.OptionsManager.UpdateOptionsAsync(this.ApplicationOptions.CurrentValue);
    }

}