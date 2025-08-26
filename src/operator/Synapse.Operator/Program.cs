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

using Synapse.Runtime.Docker.Services;
using Synapse.Runtime.Kubernetes.Services;

if (args.Length != 0 && args.Contains("--debug") && !Debugger.IsAttached) Debugger.Launch();

var builder = Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", true, true);
        config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, true);
        config.AddEnvironmentVariables("SYNAPSE_OPERATOR");
        config.AddCommandLine(args);
        config.AddKeyPerFile("/run/secrets/synapse", true, true);
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<OperatorOptions>(context.Configuration);
        services.AddLogging(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                options.TimestampFormat = "[HH:mm:ss] ";
            });
        });
        services.AddSingleton<IUserAccessor, ApplicationUserAccessor>();
        services.AddSynapse(context.Configuration);
        services.AddScoped<NativeRuntime>();
        services.AddScoped<DockerRuntime>();
        services.AddScoped<KubernetesRuntime>();
        services.AddScoped<IWorkflowRuntime>(provider => 
        {
            var options = provider.GetRequiredService<IOptionsMonitor<OperatorOptions>>().CurrentValue;
            return options.Runner.Runtime.Mode switch
            {
                OperatorRuntimeMode.Native => provider.GetRequiredService<NativeRuntime>(),
                OperatorRuntimeMode.Docker => provider.GetRequiredService<DockerRuntime>(),
                OperatorRuntimeMode.Kubernetes => provider.GetRequiredService<KubernetesRuntime>(),
                _ => throw new NotSupportedException($"The specified operator runtime mode '{options.Runner.Runtime.Mode}' is not supported")
            };
        });

        services.AddScoped<OperatorController>();
        services.AddScoped<IOperatorController>(provider => provider.GetRequiredService<OperatorController>());

        services.AddScoped<WorkflowController>();
        services.AddScoped<IResourceController<Workflow>>(provider => provider.GetRequiredService<WorkflowController>());
        services.AddScoped<IWorkflowController>(provider => provider.GetRequiredService<WorkflowController>());

        services.AddScoped<WorkflowInstanceController>();
        services.AddScoped<IResourceController<WorkflowInstance>>(provider => provider.GetRequiredService<WorkflowInstanceController>());

        services.AddSingleton<OperatorApplication>();
        services.AddHostedService(provider => provider.GetRequiredService<OperatorApplication>());
        services.AddSingleton<RunnerConfigurationMonitor>();
        services.AddHostedService(provider => provider.GetRequiredService<RunnerConfigurationMonitor>());
        services.AddSingleton<IOptionsMonitor<RunnerConfiguration>>(provider => provider.GetRequiredService<RunnerConfigurationMonitor>());
    });

using var app = builder.Build();

await app.RunAsync();