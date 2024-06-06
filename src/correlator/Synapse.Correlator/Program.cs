// Copyright © 2024-Present Neuroglia SRL. All rights reserved.
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
        config.AddEnvironmentVariables("SYNAPSE_CORRELATOR");
        config.AddCommandLine(args);
        config.AddKeyPerFile("/run/secrets/synapse", true, true);
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<CorrelatorOptions>(context.Configuration);
        services.AddLogging(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                options.TimestampFormat = "[HH:mm:ss] ";
            });
        });
        services.AddSingleton<IUserAccessor, ApplicationUserAccessor>();
        services.AddSynapse(context.Configuration);
        services.AddScoped<CorrelatorController>();
        services.AddScoped<ICorrelatorController>(provider => provider.GetRequiredService<CorrelatorController>());
        services.AddHostedService<Application>();
    });

using var app = builder.Build();

await app.RunAsync();
