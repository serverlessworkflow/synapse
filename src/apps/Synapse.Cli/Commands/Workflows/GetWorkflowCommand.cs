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

namespace Synapse.Cli.Commands.Workflows
{

    /// <summary>
    /// Represents the <see cref="Command"/> used to get a <see cref="V1Workflow"/>
    /// </summary>
    internal class GetWorkflowCommand
        : Command
    {

        /// <summary>
        /// Gets the <see cref="GetWorkflowCommand"/>'s name
        /// </summary>
        public const string CommandName = "get";
        /// <summary>
        /// Gets the <see cref="GetWorkflowCommand"/>'s description
        /// </summary>
        public const string CommandDescription = "Gets the workflow with the specified id";

        /// <summary>
        /// Initializes a new <see cref="GetWorkflowCommand"/>
        /// </summary>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="synapseManagementApi">The service used to interact with the remote Synapse Management API</param>
        public GetWorkflowCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseManagementApi synapseManagementApi)
            : base(serviceProvider, loggerFactory, synapseManagementApi, CommandName, CommandDescription)
        {
            this.AddArgument(new Argument("id", "The id of the workflow to get"));
            this.Handler = CommandHandler.Create<string>(this.HandleAsync);
        }

        /// <summary>
        /// Handles the <see cref="GetWorkflowCommand"/>
        /// </summary>
        /// <param name="id">The id of the workflow to get</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public async Task HandleAsync(string id)
        {
            var workflow = await this.SynapseManagementApi.GetWorkflowByIdAsync(id);
            Console.WriteLine(JObject.FromObject(workflow).ToString(Formatting.Indented));
        }

    }

}
