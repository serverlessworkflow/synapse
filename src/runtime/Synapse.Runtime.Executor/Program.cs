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
using Microsoft.Extensions.Hosting;
using Neuroglia.Data.Expressions.JQ;
using Synapse.Ports.Grpc;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddLogging(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                options.TimestampFormat = "[HH:mm:ss] ";
            });
        });
        services.AddServerlessWorkflow();
        services.AddSynapseGrpcApiClient();
        services.AddSynapseGrpcRuntimeApiClient();

        services.AddNewtonsoftJsonSerializer();
        services.AddJQExpressionEvaluator();
        services.AddHttpClient();

        services.AddSingleton<WorkflowActivityProcessorFactory>();
        services.AddSingleton<IWorkflowActivityProcessorFactory>(provider => provider.GetRequiredService<WorkflowActivityProcessorFactory>());

        services.AddSingleton<WorkflowRuntimeContext>();

        services.AddSingleton<IWorkflowRuntimeContext>(provider => provider.GetRequiredService<WorkflowRuntimeContext>());

        services.AddSingleton<WorkflowRuntime>();
        services.AddSingleton<IWorkflowRuntime>(provider => provider.GetRequiredService<WorkflowRuntime>());
        services.AddHostedService(provider => provider.GetRequiredService<IWorkflowRuntime>());
    })
    .Build();
await host.RunAsync();