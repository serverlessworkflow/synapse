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

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddLogging();
builder.Services.AddSerialization();
builder.Services.AddJsonSerializer(options =>
{
    options.WriteIndented = true;
});
builder.Services.AddYamlDotNetSerializer();
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
builder.Services.AddScoped<IApplicationLayout, ApplicationLayout>();
builder.Services.AddSingleton<IMonacoEditorHelper, MonacoEditorHelper>();
builder.Services.AddSingleton<ISecurityTokenManager, SecurityTokenManager>();
builder.Services.AddSingleton<MonacoInterop>();
builder.Services.AddBlazorBootstrap();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<ApplicationAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<ApplicationAuthenticationStateProvider>());
builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Authentication:OIDC", options.ProviderOptions);
});

await builder.Build().RunAsync();