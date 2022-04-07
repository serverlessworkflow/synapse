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

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Neuroglia.Blazor.Dagre;
using Neuroglia.Blazor.Dagre.Models;
using Neuroglia.Data;
using Neuroglia.Data.Flux;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ServerlessWorkflow.Sdk;
using Simple.OData.Client;
using Synapse;
using Synapse.Dashboard;
using Synapse.Dashboard.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(provider => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSynapseRestApiClient(http => http.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
builder.Services.AddServerlessWorkflow();
builder.Services.AddPluralizer();
builder.Services.AddSingleton<IODataClient>(new ODataClient(new ODataClientSettings()
{
    BaseUri = new($"http://localhost:9600/api/odata"),
    PayloadFormat = ODataPayloadFormat.Json
}));
builder.Services.AddScoped<ILayoutService, LayoutService>();
builder.Services.AddSingleton<IToastManager, ToastManager>();
builder.Services.AddSingleton<IAccordionManager, AccordionManager>();
builder.Services.AddSingleton<IIntegrationEventStream, IntegrationEventStream>();
builder.Services.AddSingleton<IMonacoEditorHelper, MonacoEditorHelper>();
builder.Services.AddSingleton<IBreadcrumbService, BreadcrumbService>();
builder.Services.AddScoped<IWorkflowDiagramBuilder, WorkflowDiagramBuilder>();
builder.Services.AddScoped<IDagreService, DagreService>();
builder.Services.AddScoped<WorkflowGraphBuilder>();
builder.Services.AddFlux(flux =>
{
    flux
        .ScanMarkupTypeAssembly<App>()
        .UseReduxDevTools();
});
builder.Services.AddSingleton(provider =>
{
    return new HubConnectionBuilder()
        .WithUrl($"http://localhost:9600/api/ws")
        .WithAutomaticReconnect()
        .AddNewtonsoftJsonProtocol(options =>
        {
            options.PayloadSerializerSettings = new()
            {
                ContractResolver = new NonPublicSetterContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateParseHandling = DateParseHandling.DateTime,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };
        })
        .Build();
});
await builder.Build().RunAsync();
