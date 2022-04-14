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
using Neuroglia.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ServerlessWorkflow.Sdk;
using Synapse;
using Synapse.Cli;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

var parser = BuildCommandLineParser();
await parser.InvokeAsync(args);

static Parser BuildCommandLineParser()
{
    var serviceProvider = BuildServiceProvider();
    var rootCommand = new RootCommand();
    foreach (var command in serviceProvider.GetServices<Command>())
    {
        rootCommand.AddCommand(command);
    }
    return new CommandLineBuilder(rootCommand)
        .UseDefaults()
        .UseExceptionHandler((ex, context) =>
        {
            AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
            var inner = ex.InnerException;
            while(inner != null)
            {
                AnsiConsole.MarkupLine($"[red]{inner.Message}[/]");
                inner = inner.InnerException;
            }
        })
        .Build();
}

static IServiceProvider BuildServiceProvider()
{
    IServiceCollection services = new ServiceCollection();
    services.AddLogging();
    services.AddNewtonsoftJsonSerializer(settings =>
    {
        settings.ContractResolver = new NonPublicSetterContractResolver();
        settings.NullValueHandling = NullValueHandling.Ignore;
    });
    services.AddServerlessWorkflow();
    services.AddSynapseRestApiClient(http => http.BaseAddress = new Uri("http://localhost:42286")); //todo: config based
    services.AddCliCommands();
    return services.BuildServiceProvider();
}