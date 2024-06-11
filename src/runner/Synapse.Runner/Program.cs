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

if (args.Length != 0 && args.Contains("--debug") && !Debugger.IsAttached) Debugger.Launch();

var builder = Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", true, true);
        config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, true);
        config.AddEnvironmentVariables("SYNAPSE");
        config.AddCommandLine(args);
        config.AddKeyPerFile("/run/secrets/synapse", true, true);
    })
    .ConfigureServices((context, services) =>
    {
        var options = new RunnerOptions();
        context.Configuration.Bind(options);
        services.Configure<RunnerOptions>(context.Configuration);
        services.AddLogging(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                options.TimestampFormat = "[HH:mm:ss] ";
            });
        });
        services.AddSerialization();
        services.AddJsonSerializer();
        services.AddYamlDotNetSerializer();
        services.AddJQExpressionEvaluator();
        services.AddJavaScriptExpressionEvaluator();
        services.AddNodeJSScriptExecutor();
        services.AddPythonScriptExecutor();
        services.AddSynapseHttpApiClient(http =>
        {
            http.BaseAddress = options.Api.BaseAddress;
        });
        services.AddSingleton<ITaskExecutionContextFactory, TaskExecutionContextFactory>();
        services.AddSingleton<ITaskExecutorFactory, TaskExecutorFactory>();
        services.AddHostedService<RunnerApplication>();
    });

using var app = builder.Build();

await app.RunAsync();