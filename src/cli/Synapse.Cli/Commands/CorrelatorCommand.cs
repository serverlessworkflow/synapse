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
using Synapse.Cli.Commands.Correlators;

namespace Synapse.Cli.Commands;

/// <summary>
/// Represents the <see cref="Command"/> used to manage <see cref="Correlator"/>s
/// </summary>
public class CorrelatorCommand
    : Command
{

    /// <summary>
    /// Gets the <see cref="OperatorCommand"/>'s name
    /// </summary>
    public const string CommandName = "correlator";
    /// <summary>
    /// Gets the <see cref="OperatorCommand"/>'s description
    /// </summary>
    public const string CommandDescription = "Manages correlators";

    /// <inheritdoc/>
    public CorrelatorCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api)
        : base(serviceProvider, loggerFactory, api, CommandName, CommandDescription)
    {
        this.AddAlias("correlators");
        this.AddAlias("cor");
        this.AddCommand(ActivatorUtilities.CreateInstance<GetCorrelatorCommand>(this.ServiceProvider));
        this.AddCommand(ActivatorUtilities.CreateInstance<ListCorrelatorsCommand>(this.ServiceProvider));
    }

}