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
using Synapse.Cli.Commands.Operators;

namespace Synapse.Cli.Commands;

/// <summary>
/// Represents the <see cref="Command"/> used to manage <see cref="Operator"/>s
/// </summary>
public class OperatorCommand
    : Command
{

    /// <summary>
    /// Gets the <see cref="OperatorCommand"/>'s name
    /// </summary>
    public const string CommandName = "operator";
    /// <summary>
    /// Gets the <see cref="OperatorCommand"/>'s description
    /// </summary>
    public const string CommandDescription = "Manages operators";

    /// <inheritdoc/>
    public OperatorCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api)
        : base(serviceProvider, loggerFactory, api, CommandName, CommandDescription)
    {
        this.AddAlias("operators");
        this.AddAlias("op");
        this.AddCommand(ActivatorUtilities.CreateInstance<GetOperatorCommand>(this.ServiceProvider));
        this.AddCommand(ActivatorUtilities.CreateInstance<ListOperatorsCommand>(this.ServiceProvider));
    }

}