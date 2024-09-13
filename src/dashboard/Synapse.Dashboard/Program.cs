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

using FluentValidation;
using Microsoft.AspNetCore.Components.Authorization;
using Neuroglia.Blazor.Dagre;
using ServerlessWorkflow.Sdk.Models;
using ServerlessWorkflow.Sdk.Validation;
using System.Text.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddLogging();
builder.Services.AddSerialization();
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.WriteIndented = true;
});
builder.Services.AddScoped(provider => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSynapseHttpApiClient(options =>
{
    options.BaseAddress = new(builder.HostEnvironment.BaseAddress);
    options.TokenFactory = provider => provider.GetRequiredService<ISecurityTokenManager>().GetTokenAsync();
});
builder.Services.AddFlux(flux =>
{
    flux.ScanMarkupTypeAssembly<App>();
});
builder.Services.AddSingleton<ISecurityTokenManager, SecurityTokenManager>();
builder.Services.AddBlazorBootstrap();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<ApplicationAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<ApplicationAuthenticationStateProvider>());
builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Authentication:OIDC", options.ProviderOptions);
});
// Seems to bring conflict between scope & singleton services
//builder.Services.AddServerlessWorkflowValidation();
/* From AddServerlessWorkflowValidation */
builder.Services.AddScoped<IWorkflowDefinitionValidator, WorkflowDefinitionValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<WorkflowDefinition>();
var defaultPropertyNameResolver = ValidatorOptions.Global.PropertyNameResolver;
ValidatorOptions.Global.PropertyNameResolver = (type, member, lambda) =>
{
    return member == null ? defaultPropertyNameResolver(type, member, lambda) : member.Name.ToCamelCase();
};
/* End of AddServerlessWorkflowValidation */
builder.Services.AddSingleton<IMonacoEditorHelper, MonacoEditorHelper>();
builder.Services.AddScoped<IApplicationLayout, ApplicationLayout>();
builder.Services.AddScoped<SpecificationSchemaManager>();
builder.Services.AddSingleton<JSInterop>();
builder.Services.AddSingleton<MonacoInterop>();
builder.Services.AddSingleton<IGraphLayoutService, GraphLayoutService>();
builder.Services.AddSingleton<IWorkflowGraphBuilder, WorkflowGraphBuilder>();
builder.Services.AddSingleton<IBreadcrumbManager, BreadcrumbManager>();

await builder.Build().RunAsync();