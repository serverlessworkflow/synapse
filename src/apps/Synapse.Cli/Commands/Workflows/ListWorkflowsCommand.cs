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

using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Cli.Commands.Workflows
{

    /// <summary>
    /// Represents the <see cref="Command"/> used to list <see cref="V1Workflow"/>s
    /// </summary>
    internal class ListWorkflowsCommand
        : Command
    {

        /// <summary>
        /// Gets the <see cref="ListWorkflowsCommand"/>'s name
        /// </summary>
        public const string CommandName = "list";
        /// <summary>
        /// Gets the <see cref="ListWorkflowsCommand"/>'s description
        /// </summary>
        public const string CommandDescription = "Lists all workflows";

        /// <inheritdoc/>
        public ListWorkflowsCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseManagementApi synapseManagementApi) 
            : base(serviceProvider, loggerFactory, synapseManagementApi, CommandName, CommandDescription)
        {
            this.AddAlias("ls");
            this.AddOption(CommandOptions.Id);
            this.AddOption(CommandOptions.Filter);
            this.Handler = CommandHandler.Create<string, string>(this.HandleAsync);
        }

        /// <summary>
        /// Handles the <see cref="ListWorkflowsCommand"/>
        /// </summary>
        /// <param name="id">The id of the workflows to list</param>
        /// <param name="filter">The ODATA filter to use</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public async Task HandleAsync(string id, string filter)
        {
            var query = string.Empty;
            var filters = new List<string>();
            if (!string.IsNullOrWhiteSpace(id))
                filters.Add($"startswith({nameof(V1Workflow.Definition)}/{nameof(WorkflowDefinition.Id)},'{id}')");
            if (!string.IsNullOrWhiteSpace(filter))
                filters.Add(filter);
            if (filters.Any())
                query = $"$filter={string.Join(" AND ", filters)}";
            var workflows = await this.SynapseManagementApi.GetWorkflowsAsync(query);
            var table = new Table();
            table.AddColumn("Id");
            table.AddColumn("Version");
            table.AddColumn("Created at");
            table.AddColumn("Last instanciated at");
            foreach(var workflow in workflows)
            {
                table.AddRow(workflow.Definition.Id!, workflow.Definition.Version, workflow.CreatedAt.ToString(), (workflow.LastInstanciated.HasValue ? workflow.LastInstanciated.ToString() : "-")!);
            }
            AnsiConsole.Write(table);
        }

        private static class CommandOptions
        {

            public static Option<string> Id
            {
                get
                {
                    var option = new Option<string>("--id")
                    {
                        Description = "The id of the workflow to list the versions of"
                    };
                    return option;
                }
            }

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
