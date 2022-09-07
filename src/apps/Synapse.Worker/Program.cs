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

using GraphQL.Client.Abstractions;
using GraphQL.Client.Abstractions.Websocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Neuroglia.AsyncApi;
using Neuroglia.Data.Expressions.JQ;
using Newtonsoft.Json;
using Synapse.Apis.Management.Grpc;
using Synapse.Apis.Runtime.Grpc;
using Synapse.Integration.Serialization.Converters;
using System.Diagnostics;

if (args.Any() 
    && args.Contains("--debug") 
    && !Debugger.IsAttached)
    Debugger.Launch();
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
using var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile("appsettings.json", true, true);
        config.AddKeyPerFile("/run/secrets/synapse", true, true);
    })
    .ConfigureServices((context, services) =>
    {
        var applicationOptions = new ApplicationOptions();
        context.Configuration.Bind(applicationOptions);
        services.AddSingleton(Options.Create(applicationOptions));

        services.AddLogging(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                options.TimestampFormat = "[HH:mm:ss] ";
            });
        });
        services.AddAsyncApiClientFactory(asyncApi => 
        {
            asyncApi.UseAllBindings();
        });
        services.AddSynapseGrpcManagementApiClient();
        services.AddSynapseGrpcRuntimeApiClient();

        services.AddNewtonsoftJsonSerializer(settings =>
        {
            settings.Converters.Add(new FilteredExpandoObjectConverter());
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.DefaultValueHandling = DefaultValueHandling.Ignore;
        });
        services.AddServerlessWorkflow();
        services.AddJQExpressionEvaluator();
        services.AddHttpClient();
        services.AddTransient<GraphQL.Client.Serializer.Newtonsoft.NewtonsoftJsonSerializer>();
        services.AddTransient<IGraphQLJsonSerializer>(provider => provider.GetRequiredService<GraphQL.Client.Serializer.Newtonsoft.NewtonsoftJsonSerializer>());
        services.AddTransient<IGraphQLWebsocketJsonSerializer>(provider => provider.GetRequiredService<GraphQL.Client.Serializer.Newtonsoft.NewtonsoftJsonSerializer>());

        services.AddSingleton<OAuth2TokenManager>();
        services.AddSingleton<IOAuth2TokenManager>(provider => provider.GetRequiredService<OAuth2TokenManager>());

        services.AddSingleton<FileBasedSecretManager>();
        services.AddSingleton<ISecretManager>(provider => provider.GetRequiredService<FileBasedSecretManager>());
        services.AddHostedService(provider => provider.GetRequiredService<FileBasedSecretManager>());

        services.AddSingleton<WorkflowActivityProcessorFactory>();
        services.AddSingleton<IWorkflowActivityProcessorFactory>(provider => provider.GetRequiredService<WorkflowActivityProcessorFactory>());

        services.AddSingleton<WorkflowRuntimeContext>();
        services.AddSingleton<IWorkflowRuntimeContext>(provider => provider.GetRequiredService<WorkflowRuntimeContext>());

        services.AddSingleton<WorkflowRuntime>();
        services.AddSingleton<IWorkflowRuntime>(provider => provider.GetRequiredService<WorkflowRuntime>());
        services.AddHostedService(provider => provider.GetRequiredService<IWorkflowRuntime>());

        services.AddSingleton<IIntegrationEventBus, IntegrationEventBus>();

        if (applicationOptions.SkipCertificateValidation)
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            services.ConfigureAll<HttpClientFactoryOptions>(options =>
            {
                options.HttpMessageHandlerBuilderActions.Add(builder =>
                {
                    builder.PrimaryHandler = new HttpClientHandler
                    {
                        ClientCertificateOptions = ClientCertificateOption.Manual,
                        ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true
                    };
                });
            });
        }
    })
    .Build();
await host.RunAsync();