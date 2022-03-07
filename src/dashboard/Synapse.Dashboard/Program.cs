using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Neuroglia.Data;
using ServerlessWorkflow.Sdk;
using Simple.OData.Client;
using Synapse;
using Synapse.Dashboard;
using Synapse.Dashboard.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSynapseRestApiClient(http => http.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
builder.Services.AddServerlessWorkflow();
builder.Services.AddPluralizer();
builder.Services.AddSingleton<IODataClient>(new ODataClient(new ODataClientSettings()
{
    BaseUri = new($"http://localhost:9600/api/odata"),
    PayloadFormat = ODataPayloadFormat.Json
}));
//builder.Services.AddNewtonsoftJsonSerializer(); //todo: include this in the add synapse rest api client call
builder.Services.AddScoped<ILayoutService, LayoutService>();
builder.Services.AddSingleton<IToastManager, ToastManager>();
builder.Services.AddSingleton<IAccordionManager, AccordionManager>();
builder.Services.AddSingleton<IIntegrationEventStream, IntegrationEventStream>();
builder.Services.AddSingleton<IMonacoEditorHelper, MonacoEditorHelper>();
builder.Services.AddScoped<IWorkflowDiagramBuilder, WorkflowDiagramBuilder>();
builder.Services.AddSingleton(provider =>
{
    return new HubConnectionBuilder()
        .WithUrl($"http://localhost:9600/api/ws")
        .WithAutomaticReconnect()
        .AddNewtonsoftJsonProtocol()
        .Build();
});
await builder.Build().RunAsync();
