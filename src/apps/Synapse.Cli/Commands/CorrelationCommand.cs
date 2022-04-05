/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using Microsoft.Extensions.DependencyInjection;
using Synapse.Cli.Commands.Correlations;

namespace Synapse.Cli.Commands
{
    /// <summary>
    /// Represents the <see cref="Command"/> used to manage <see cref="V1Correlation"/>s
    /// </summary>
    public class CorrelationCommand
        : Command
    {

        /// <summary>
        /// Gets the <see cref="CorrelationCommand"/>'s name
        /// </summary>
        public const string CommandName = "correlations";
        /// <summary>
        /// Gets the <see cref="CorrelationCommand"/>'s description
        /// </summary>
        public const string CommandDescription = "Manages correlations";

        /// <inheritdoc/>
        public CorrelationCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseManagementApi synapseManagementApi)
            : base(serviceProvider, loggerFactory, synapseManagementApi, CommandName, CommandDescription)
        {
            this.AddAlias("correl");
            this.AddAlias("crl");
            this.AddCommand(ActivatorUtilities.CreateInstance<GetCorrelationCommand>(this.ServiceProvider));
            this.AddCommand(ActivatorUtilities.CreateInstance<ListCorrelationsCommand>(this.ServiceProvider));
        }

    }

}
