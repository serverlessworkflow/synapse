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

using Synapse.Cli.Commands.Config;

namespace Synapse.Cli.Commands;

/// <summary>
/// Represents the <see cref="Command"/> used to configure the Synapse CLI
/// </summary>
public class ConfigCommand
    : Command
{

    /// <summary>
    /// Gets the <see cref="ConfigCommand"/>'s name
    /// </summary>
    public const string CommandName = "config";
    /// <summary>
    /// Gets the <see cref="ConfigCommand"/>'s description
    /// </summary>
    public const string CommandDescription = "Configures the Synapse CLI";

    /// <inheritdoc/>
    public ConfigCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api)
        : base(serviceProvider, loggerFactory, api, CommandName, CommandDescription)
    {
        this.AddCommand(ActivatorUtilities.CreateInstance<DeleteApiCommand>(this.ServiceProvider));
        this.AddCommand(ActivatorUtilities.CreateInstance<GetApisCommand>(this.ServiceProvider));
        this.AddCommand(ActivatorUtilities.CreateInstance<SetApiCommand>(this.ServiceProvider));
        this.AddCommand(ActivatorUtilities.CreateInstance<UseApiCommand>(this.ServiceProvider));
    }

}
