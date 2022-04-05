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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Synapse.Cli.Commands.WorkflowInstances
{

    /// <summary>
    /// Represents the <see cref="Command"/> used to get a <see cref="V1WorkflowInstance"/>
    /// </summary>
    internal class GetWorkflowInstanceCommand
        : Command
    {

        /// <summary>
        /// Gets the <see cref="GetWorkflowInstanceCommand"/>'s name
        /// </summary>
        public const string CommandName = "get";
        /// <summary>
        /// Gets the <see cref="GetWorkflowInstanceCommand"/>'s description
        /// </summary>
        public const string CommandDescription = "Gets the workflow instance with the specified id";

        /// <summary>
        /// Initializes a new <see cref="GetWorkflowInstanceCommand"/>
        /// </summary>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="synapseManagementApi">The service used to interact with the remote Synapse Management API</param>
        public GetWorkflowInstanceCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseManagementApi synapseManagementApi)
            : base(serviceProvider, loggerFactory, synapseManagementApi, CommandName, CommandDescription)
        {
            this.AddArgument(new Argument("id", "The id of the workflow instance to get"));
            this.AddOption(CommandOptions.Output);
            this.Handler = CommandHandler.Create<string, bool>(this.HandleAsync);
        }

        /// <summary>
        /// Handles the <see cref="GetWorkflowInstanceCommand"/>
        /// </summary>
        /// <param name="id">The id of the workflow instance to get</param>
        /// <param name="output">A boolean indicating whether or not to only show the output of the specified wporkflow instance</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public async Task HandleAsync(string id, bool output)
        {
            var workflowInstance = await this.SynapseManagementApi.GetWorkflowInstanceByIdAsync(id);
            var jobj = JObject.FromObject(workflowInstance);
            var outputJson = jobj.ToString(Formatting.Indented);
            if (output)
            {
                switch (workflowInstance.Status)
                {
                    case V1WorkflowInstanceStatus.Completed:
                        outputJson = jobj.Property(nameof(V1WorkflowInstance.Output), StringComparison.OrdinalIgnoreCase)!.ToString(Formatting.Indented);
                        break;
                    case V1WorkflowInstanceStatus.Faulted:
                        outputJson = jobj.Property(nameof(V1WorkflowInstance.Error), StringComparison.OrdinalIgnoreCase)!.ToString(Formatting.Indented);
                        break;
                    default:

                        break;
                }
            }
            Console.WriteLine(outputJson);
        }

        private static class CommandOptions
        {

            public static Option<bool> Output
            {
                get
                {
                    var option = new Option<bool>("--output")
                    {
                        Description = "Shows only the output of the specified workflow instance, or the error it encountered in case it did not successfully complete"
                    };
                    option.AddAlias("-o");
                    return option;
                }
            }

        }

    }

}
