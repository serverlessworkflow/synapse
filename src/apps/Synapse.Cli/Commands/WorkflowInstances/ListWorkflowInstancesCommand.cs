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

namespace Synapse.Cli.Commands.WorkflowInstances
{

    /// <summary>
    /// Represents the <see cref="Command"/> used to list <see cref="V1Workflow"/>s
    /// </summary>
    internal class ListWorkflowInstancesCommand
        : Command
    {

        /// <summary>
        /// Gets the <see cref="ListWorkflowInstancesCommand"/>'s name
        /// </summary>
        public const string CommandName = "list";
        /// <summary>
        /// Gets the <see cref="ListWorkflowInstancesCommand"/>'s description
        /// </summary>
        public const string CommandDescription = "Lists/filters workflow instances";

        /// <inheritdoc/>
        public ListWorkflowInstancesCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseManagementApi synapseManagementApi)
            : base(serviceProvider, loggerFactory, synapseManagementApi, CommandName, CommandDescription)
        {
            this.AddAlias("ls");
            this.AddOption(CommandOptions.Filter);
            this.Handler = CommandHandler.Create<string>(this.HandleAsync);
        }

        /// <summary>
        /// Handles the <see cref="ListWorkflowInstancesCommand"/>
        /// </summary>
        /// <param name="filter">The ODATA filter to apply</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public async Task HandleAsync(string filter)
        {
            var query = string.Empty;
            if (!string.IsNullOrWhiteSpace(filter))
                query = $"$filter={filter}";
            var workflowInstances = await this.SynapseManagementApi.GetWorkflowInstancesAsync(query);
            var table = new Table();
            table.Expand = true;
            table.Border(TableBorder.None);
            table.AddColumn("ID");
            table.AddColumn("WORKFLOW");
            table.AddColumn("KEY");
            table.AddColumn("STATUS");
            table.AddColumn("ACTIVATION TYPE");
            table.AddColumn("CREATED AT");
            table.AddColumn("STARTED AT");
            table.AddColumn("EXECUTED AT");
            foreach (var workflowInstance in workflowInstances)
            {
                var status = workflowInstance.Status switch
                {
                    V1WorkflowInstanceStatus.Starting | V1WorkflowInstanceStatus.Resuming => $"[navyblue]{workflowInstance.Status}[/]",
                    V1WorkflowInstanceStatus.Running  => $"[dodgerblue2]{workflowInstance.Status}[/]",
                    V1WorkflowInstanceStatus.Suspending => $"[indianred1]{workflowInstance.Status}[/]",
                    V1WorkflowInstanceStatus.Suspended => $"[orangered1]{workflowInstance.Status}[/]",
                    V1WorkflowInstanceStatus.Cancelling => $"[purple_2]{workflowInstance.Status}[/]",
                    V1WorkflowInstanceStatus.Cancelled => $"[mediumvioletred]{workflowInstance.Status}[/]",
                    V1WorkflowInstanceStatus.Faulted => $"[red3_1]{workflowInstance.Status}[/]",
                    V1WorkflowInstanceStatus.Completed => $"[chartreuse2_1]{workflowInstance.Status}[/]",
                    _ => workflowInstance.Status.ToString()
                };
                table.AddRow
                (
                    workflowInstance.Id, 
                    workflowInstance.WorkflowId, 
                    workflowInstance.Key, 
                    status, 
                    workflowInstance.ActivationType.ToString(),
                    workflowInstance.CreatedAt.ToString(), 
                    (workflowInstance.StartedAt.HasValue ? workflowInstance.StartedAt.ToString() : "-")!, 
                    (workflowInstance.ExecutedAt.HasValue ? workflowInstance.ExecutedAt.ToString() : "-")!
                );
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
