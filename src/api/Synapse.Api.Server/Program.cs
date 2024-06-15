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

using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);
var applicationOptions = new ApiServerOptions();
builder.Configuration.Bind(applicationOptions);
if (applicationOptions.Authentication.Tokens.Count < 1) throw new Exception("The Synapse API server requires that at least one static user token be configured");

builder.Services.Configure<ApiServerOptions>(builder.Configuration);
builder.Services.AddResponseCompression();
builder.Services.AddSynapse(builder.Configuration);
builder.Services.AddSynapseApi();
builder.Services.AddSynapseHttpApi();

var authentication = builder.Services.AddAuthentication(FallbackPolicySchemeDefaults.AuthenticationScheme);
authentication.AddScheme<StaticBearerAuthenticationOptions, StaticBearerAuthenticationHandler>(StaticBearerDefaults.AuthenticationScheme, options =>
{
    foreach(var token in applicationOptions.Authentication.Tokens) options.AddToken(token.Key, new(token.Value.Select(kvp => new Claim(kvp.Key, kvp.Value)), StaticBearerDefaults.AuthenticationScheme, JwtClaimTypes.Name, JwtClaimTypes.Role));
});
authentication.AddScheme<ServiceAccountAuthenticationOptions, ServiceAccountAuthenticationHandler>(ServiceAccountAuthenticationDefaults.AuthenticationScheme, options =>
{

});
authentication.AddPolicyScheme(FallbackPolicySchemeDefaults.AuthenticationScheme, FallbackPolicySchemeDefaults.AuthenticationScheme, options =>
{
    options.ForwardDefaultSelector = context =>
    {
        var authorizationValue = context.Request.Headers.Authorization.ToString();
        var bearerPrefix = $"{JwtBearerDefaults.AuthenticationScheme} ";
        if (authorizationValue?.StartsWith(bearerPrefix) == true)
        {
            var token = authorizationValue[bearerPrefix.Length..].Trim();
            var staticBearerOptions = context.RequestServices.GetRequiredService<IOptionsMonitor<StaticBearerAuthenticationOptions>>().Get(StaticBearerDefaults.AuthenticationScheme);
            if (staticBearerOptions.Tokens.ContainsKey(token)) return StaticBearerDefaults.AuthenticationScheme;
        }
        return ServiceAccountAuthenticationDefaults.AuthenticationScheme;
    };
});
if (applicationOptions.Authentication.Jwt != null)
{
    authentication.AddJwtBearer(options =>
    {
        options.Authority = applicationOptions.Authentication.Jwt.Authority;
        options.Audience = applicationOptions.Authentication.Jwt.Audience;
        options.TokenValidationParameters = new()
        {
            ValidAudience = applicationOptions.Authentication.Jwt.Audience,
            ValidateAudience = !string.IsNullOrWhiteSpace(applicationOptions.Authentication.Jwt.Audience),
            ValidIssuer = applicationOptions.Authentication.Jwt.Issuer,
            ValidateIssuer = !string.IsNullOrWhiteSpace(applicationOptions.Authentication.Jwt.Issuer),
            IssuerSigningKey = applicationOptions.Authentication.Jwt.GetSigningKey()
        };
    });
}
if (applicationOptions.Authentication.Oidc != null)
{
    authentication.AddOpenIdConnect(options =>
    {
        options.Authority = applicationOptions.Authentication.Oidc.Authority;
        options.ClientId = applicationOptions.Authentication.Oidc.ClientId;
        options.ClientSecret = applicationOptions.Authentication.Oidc.ClientSecret;
        options.Resource = applicationOptions.Authentication.Oidc.Resource;
        options.ResponseMode = applicationOptions.Authentication.Oidc.ResponseMode;
        options.ResponseType = applicationOptions.Authentication.Oidc.ResponseType;
        options.UsePkce = applicationOptions.Authentication.Oidc.UsePkce;
        applicationOptions.Authentication.Oidc.Scope?.ForEach(options.Scope.Add);
        options.TokenValidationParameters = new()
        {
            ValidAudience = applicationOptions.Authentication.Oidc.Audience,
            ValidateAudience = !string.IsNullOrWhiteSpace(applicationOptions.Authentication.Oidc.Audience),
            ValidIssuer = applicationOptions.Authentication.Oidc.Issuer,
            ValidateIssuer = !string.IsNullOrWhiteSpace(applicationOptions.Authentication.Oidc.Issuer),
            IssuerSigningKey = applicationOptions.Authentication.Oidc.GetSigningKey()
        };
    });
}
using var app = builder.Build();
var options = app.Services.GetRequiredService<IOptions<ApiServerOptions>>().Value;

if (app.Environment.IsDevelopment() && options.ServeDashboard) app.UseWebAssemblyDebugging();
app.UseResponseCompression();
if (options.ServeDashboard) app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler(handler =>
{
    handler.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var problemDetails = exceptionHandlerPathFeature?.Error switch
        {
            ProblemDetailsException problemDetailsException => problemDetailsException.Problem,
            _ => new ProblemDetails(ErrorType.Runtime, ErrorTitle.Runtime, ErrorStatus.Runtime, exceptionHandlerPathFeature?.Error.Message)
        };
        var json = context.RequestServices.GetRequiredService<IJsonSerializer>().SerializeToText(problemDetails);
        context.Response.ContentType = MediaTypeNames.Application.Json;
        context.Response.StatusCode = problemDetails.Status;
        await context.Response.WriteAsync(json).ConfigureAwait(false);
    });
});
app.UseSwagger(builder =>
{
    builder.RouteTemplate = "api/{documentName}/doc/oas.{json|yaml}";
});
app.UseSwaggerUI(builder =>
{
    builder.DocExpansion(DocExpansion.None);
    builder.SwaggerEndpoint("/api/v1/doc/oas.json", "Synapse API v1");
    builder.RoutePrefix = "api/doc";
    builder.DisplayOperationId();
});
app.MapControllers().RequireAuthorization();
app.MapHub<ResourceEventWatchHub>("api/ws/resources/watch");
app.MapFallbackToFile("index.html");

await app.RunAsync();

/// <summary>
/// The API server's program
/// </summary>
public partial class Program { }