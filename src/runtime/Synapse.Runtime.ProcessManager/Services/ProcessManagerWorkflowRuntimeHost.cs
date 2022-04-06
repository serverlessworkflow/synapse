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
using Microsoft.Extensions.Logging;
using Synapse.Application.Services;
using Synapse.Domain.Models;
using System.Diagnostics;

namespace Synapse.Runtime.ProcessManager.Services
{

    /// <summary>
    /// Represents the Kubernetes implementation if the <see cref="IWorkflowRuntimeHost"/>
    /// </summary>
    public class ProcessManagerWorkflowRuntimeHost
        : WorkflowRuntimeHostBase
    {

        /// <inheritdoc/>
        public ProcessManagerWorkflowRuntimeHost(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {

        }

        /// <inheritdoc/>
        public override async Task<string> ScheduleAsync(V1WorkflowInstance workflowInstance, DateTimeOffset at, CancellationToken cancellationToken = default)
        {
            if (workflowInstance == null)
                throw new ArgumentNullException(nameof(workflowInstance));
            throw new NotImplementedException(); //todo: implement
        }

        /// <inheritdoc/>
        public override async Task<string> StartAsync(V1WorkflowInstance workflowInstance, CancellationToken cancellationToken = default)
        {
            if (workflowInstance == null)
                throw new ArgumentNullException(nameof(workflowInstance));
            var process = new Process() 
            {
                StartInfo = new()
                {
                    CreateNoWindow = true,
                    ErrorDialog = false,
                    WorkingDirectory = "",
                    FileName = "",
                    Arguments = "",
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
            if(!process.Start())
            {
                //todo: throw exception
            }
            return process.Id.ToString();
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException(); //todo: implement
        }

    }

}
