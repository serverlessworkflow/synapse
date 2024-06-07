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

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<SynapseApiServerOptions>(builder.Configuration);
builder.Services.AddResponseCompression();
builder.Services.AddSynapse(builder.Configuration);
builder.Services.AddSynapseApi();
builder.Services.AddSynapseHttpApi();
using var app = builder.Build();
var options = app.Services.GetRequiredService<IOptions<SynapseApiServerOptions>>().Value;

if (app.Environment.IsDevelopment() && options.ServeDashboard) app.UseWebAssemblyDebugging();
app.UseResponseCompression();
if (options.ServeDashboard) app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();
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
app.MapControllers();
app.MapHub<ResourceEventWatchHub>("api/ws/resources/watch");
app.MapFallbackToFile("index.html");

await app.RunAsync();

/// <summary>
/// The API server's program
/// </summary>
public partial class Program { }