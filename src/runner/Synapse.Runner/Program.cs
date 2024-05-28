using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Synapse.Api.Client;

var builder = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        services.AddLogging();
        services.AddSynapseHttpApiClient(http => { });
    });

using var app = builder.Build();

await app.RunAsync();