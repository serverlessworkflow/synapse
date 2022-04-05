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

namespace Synapse.Cli.Commands.Correlations
{
    /// <summary>
    /// Represents the <see cref="Command"/> used to list <see cref="V1Correlation"/>s
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
        public const string CommandDescription = "Lists/filters correlations";

        /// <inheritdoc/>
        public ListCorrelationsCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseManagementApi synapseManagementApi)
            : base(serviceProvider, loggerFactory, synapseManagementApi, CommandName, CommandDescription)
        {
            this.AddAlias("ls");
            this.AddOption(CommandOptions.Filter);
            this.Handler = CommandHandler.Create<string>(this.HandleAsync);
        }

        /// <summary>
        /// Handles the <see cref="ListCorrelationsCommand"/>
        /// </summary>
        /// <param name="filter">The ODATA filter to use</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public async Task HandleAsync(string filter)
        {
            var query = string.Empty;
            var filters = new List<string>();
            if (!string.IsNullOrWhiteSpace(filter))
                filters.Add(filter);
            if (filters.Any())
                query = $"$filter={string.Join(" AND ", filters)}";
            var correlations = await this.SynapseManagementApi.GetCorrelationsAsync(query);
            var table = new Table();
            table.AddColumn("Id");
            table.AddColumn("Lifetime");
            table.AddColumn("Outcome type");
            table.AddColumn("Outcome target");
            table.AddColumn("Condition type");
            table.AddColumn("Conditions");
            table.AddColumn("Contexts");
            table.AddColumn("Created at");
            table.AddColumn("Last modified");
            foreach (var correlation in correlations)
            {
                table.AddRow(correlation.Id, correlation.Lifetime.ToString(), correlation.Outcome.Target, correlation.Outcome.Type.ToString(), correlation.ConditionType.ToString(), correlation.Conditions.Count.ToString(), correlation.Contexts.Count.ToString(), correlation.CreatedAt.ToString(), correlation.LastModified.ToString());
            }
            AnsiConsole.Write(table);
        }

        private static class CommandOptions
        {

            public static Option<string> Filter
            {
                get
                {
                    var option = new Option<string>("--filter")
                    {
                        Description = "The ODATA filter to apply"
                    };
                    option.AddAlias("-f");
                    return option;
                }
            }

        }

    }

}
