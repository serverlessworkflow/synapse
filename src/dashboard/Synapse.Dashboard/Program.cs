/*
 * Copyright © 2022-Present The Synapse Authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using CloudNative.CloudEvents;
using CloudNative.CloudEvents.NewtonsoftJson;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Neuroglia.Blazor.Dagre;
using Neuroglia.Data;
using Neuroglia.Data.Flux;
using Neuroglia.Mapping;
using Neuroglia.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ServerlessWorkflow.Sdk;
using Simple.OData.Client;
using Synapse;
using Synapse.Dashboard;
using Synapse.Dashboard.Services;
using Synapse.Integration.Serialization.Converters;
using System;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
var baseAddress = builder.HostEnvironment.BaseAddress;
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddTransient(provider => new HttpClient { BaseAddress = new Uri(baseAddress) });
builder.Services.AddSynapseRestApiClient(http => http.BaseAddress = new Uri(baseAddress));
builder.Services.AddNewtonsoftJsonSerializer(options =>
{
    options.ContractResolver = new NonPublicSetterContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() { ProcessDictionaryKeys = false, OverrideSpecifiedNames = false, ProcessExtensionDataNames = false } };
    options.Converters = new List<JsonConverter>() { new FilteredExpandoObjectConverter() };
    options.NullValueHandling = NullValueHandling.Ignore;
    options.DefaultValueHandling = DefaultValueHandling.Ignore;
    options.DateFormatHandling = DateFormatHandling.IsoDateFormat;
    options.DateParseHandling = DateParseHandling.DateTime;
    options.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
});
builder.Services.AddSingleton<CloudEventFormatter, JsonEventFormatter>();
builder.Services.AddServerlessWorkflow();
builder.Services.AddPluralizer();
builder.Services.AddMapper(typeof(Program).Assembly);
builder.Services.AddSingleton<IODataClient>(new ODataClient(new ODataClientSettings()
{
    BaseUri = new($"{baseAddress}api/odata"),
    PayloadFormat = ODataPayloadFormat.Json
}));
builder.Services.AddSchemaRegistry();
builder.Services.AddScoped<ILayoutService, LayoutService>();
builder.Services.AddSingleton<IToastManager, ToastManager>();
builder.Services.AddSingleton<IAccordionManager, AccordionManager>();
builder.Services.AddSingleton<IIntegrationEventStream, IntegrationEventStream>();
builder.Services.AddSingleton<IMonacoEditorHelper, MonacoEditorHelper>();
builder.Services.AddSingleton<IBreadcrumbManager, BreadcrumbManager>();
builder.Services.AddSingleton<IDagreService, DagreService>();
builder.Services.AddSingleton<ICloner, Cloner>();
builder.Services.AddSingleton<IChartService, ChartService>();
builder.Services.AddSingleton<IYamlConverter, YamlConverter>();
builder.Services.AddScoped<IStyleManager, StyleManager>();
builder.Services.AddScoped<WorkflowGraphBuilder>();
builder.Services.AddFlux(flux =>
{
    if (builder.HostEnvironment.IsDevelopment())
    {
        flux.ScanMarkupTypeAssembly<App>()
            //.UseReduxDevTools() // leads to unresponsive UI, manually enable it if required.
            ;
    }
    else
    {
        flux.ScanMarkupTypeAssembly<App>();
    }
});
builder.Services.AddSingleton(provider =>
{
    return new HubConnectionBuilder()
        .WithUrl($"{baseAddress}api/ws")
        .WithAutomaticReconnect()
        .AddNewtonsoftJsonProtocol(options =>
        {
            options.PayloadSerializerSettings = new()
            {
                ContractResolver = new NonPublicSetterContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() { ProcessDictionaryKeys = false, OverrideSpecifiedNames = false, ProcessExtensionDataNames = false } },
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateParseHandling = DateParseHandling.DateTime,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };
        })
        .Build();
});

JsonConvert.DefaultSettings = () =>
{
    return new JsonSerializerSettings()
    {
        ContractResolver = new NonPublicSetterContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() { ProcessDictionaryKeys = false, OverrideSpecifiedNames = false, ProcessExtensionDataNames = false } },
        Converters = new List<JsonConverter>() { new FilteredExpandoObjectConverter() },
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Ignore,
        DateFormatHandling = DateFormatHandling.IsoDateFormat,
        DateParseHandling = DateParseHandling.DateTime,
        DateTimeZoneHandling = DateTimeZoneHandling.Utc
    };
};

await builder.Build().RunAsync();
