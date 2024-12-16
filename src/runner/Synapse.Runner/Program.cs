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

using Moq;
using Neuroglia.Serialization.Xml;
using NReco.Logging.File;
using ServerlessWorkflow.Sdk.IO;
using Synapse;
using Synapse.Core.Infrastructure.Containers;

if (args.Length != 0 && args.Contains("--debug") && !Debugger.IsAttached) Debugger.Launch();

var builder = Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", true, true);
        config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, true);
        config.AddEnvironmentVariables("SYNAPSE");
        config.AddCommandLine(args, RunnerDefaults.CommandLine.SwitchMappings);
        config.AddKeyPerFile("/run/secrets/synapse", true, true);
    })
    .ConfigureServices((context, services) =>
    {
        var runnerOptions = new RunnerOptions();
        context.Configuration.Bind(runnerOptions);
        services.Configure<RunnerOptions>(context.Configuration);
        switch (runnerOptions.ExecutionMode)
        {
            case RunnerExecutionMode.Connected:
                services.AddSynapseHttpApiClient(http =>
                {
                    var configuration = new ServerlessWorkflow.Sdk.Models.Authentication.OpenIDConnectSchemeDefinition()
                    {
                        Authority = runnerOptions.Api.BaseAddress,
                        Grant = OAuth2GrantType.ClientCredentials,
                        Client = new()
                        {
                            Id = runnerOptions.ServiceAccount.Name,
                            Secret = runnerOptions.ServiceAccount.Key
                        },
                        Scopes = ["api"]
                    };
                    http.BaseAddress = runnerOptions.Api.BaseAddress;
                    http.TokenFactory = async provider =>
                    {
                        var token = await provider.GetRequiredService<IOAuth2TokenManager>().GetTokenAsync(configuration);
                        if (string.IsNullOrWhiteSpace(token.AccessToken)) throw new NullReferenceException("The access token cannot be null");
                        return token.AccessToken;
                    };
                });
                services.AddLogging(builder =>
                {
                    builder.AddSimpleConsole(options =>
                    {
                        options.TimestampFormat = "[HH:mm:ss] ";
                    });
                });
                break;
            case RunnerExecutionMode.StandAlone:
                services.AddScoped(provider => new Mock<ISynapseApiClient>() { DefaultValue = DefaultValue.Mock }.Object);
                services.AddHttpClient();
                services.AddLogging(builder =>
                {
                    builder.ClearProviders();
                    if(!string.IsNullOrWhiteSpace(runnerOptions.Logging.OutputFilePath)) builder.AddFile(runnerOptions.Logging.OutputFilePath);
                });
                break;
            default:
                throw new NotSupportedException($"The specified runner execution mode '{runnerOptions.ExecutionMode}' is not supported");
        }
        switch (runnerOptions.Containers.Platform)
        {
            case ContainerPlatform.Docker:
                services.AddDockerContainerPlatform();
                break;
            case ContainerPlatform.Kubernetes:
                services.AddKubernetesContainerPlatform();
                break;
            default:
                throw new NotSupportedException($"The specified container platform '{runnerOptions.Containers.Platform}' is not supported");
        }
        services.AddSerialization();
        services.AddJsonSerializer(options => options.DefaultBufferSize = 128);
        services.AddSingleton<IXmlSerializer, XmlSerializer>();
        services.AddJQExpressionEvaluator();
        services.AddJavaScriptExpressionEvaluator();
        services.AddServerlessWorkflowIO();
        services.AddNodeJSScriptExecutor();
        services.AddPythonScriptExecutor();
        services.AddSingleton<SecretsManager>();
        services.AddSingleton<ISecretsManager>(provider => provider.GetRequiredService<SecretsManager>());
        services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<SecretsManager>());
        services.AddSingleton<IOAuth2TokenManager, OAuth2TokenManager>();
        services.AddSingleton<ITaskExecutionContextFactory, TaskExecutionContextFactory>();
        services.AddSingleton<ITaskExecutorFactory, TaskExecutorFactory>();
        services.AddSingleton<ISchemaHandlerProvider, SchemaHandlerProvider>();
        services.AddSingleton<ISchemaHandler, AvroSchemaHandler>();
        services.AddSingleton<ISchemaHandler, JsonSchemaHandler>();
        services.AddSingleton<ISchemaHandler, XmlSchemaHandler>();
        services.AddSingleton<IExternalResourceProvider, ExternalResourceProvider>();
        services.AddHostedService<RunnerApplication>();
        if (!runnerOptions.Certificates.Validate) services.ConfigureHttpClientDefaults(httpClient =>
        {
            httpClient.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
        });
    });

using var app = builder.Build();

await app.RunAsync();