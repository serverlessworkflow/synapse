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

if (args.Length != 0 && args.Contains("--debug") && !Debugger.IsAttached) Debugger.Launch();

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<CorrelatorOptions>(builder.Configuration);
builder.Services.AddLogging(builder =>
{
    builder.AddSimpleConsole(options =>
    {
        options.TimestampFormat = "[HH:mm:ss] ";
    });
});
builder.Services.AddSingleton<IUserAccessor, ApplicationUserAccessor>();
builder.Services.AddSynapse(builder.Configuration);
builder.Services.AddJQExpressionEvaluator();
builder.Services.AddJavaScriptExpressionEvaluator();

builder.Services.AddCloudEventBus();
builder.Services.AddTransient<IRequestHandler<IngestCloudEventCommand, IOperationResult>, IngestCloudEventCommandHandler>();

builder.Services.AddScoped<CorrelatorController>();
builder.Services.AddScoped<ICorrelatorController>(provider => provider.GetRequiredService<CorrelatorController>());

builder.Services.AddScoped<CorrelationController>();
builder.Services.AddScoped<IResourceController<Correlation>>(provider => provider.GetRequiredService<CorrelationController>());

builder.Services.AddHostedService<CorrelatorApplication>();
builder.Services.AddResponseCompression();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        JsonSerializer.DefaultOptionsConfiguration(options.JsonSerializerOptions);
    });
builder.Services.AddSwaggerGen(builder =>
{
    builder.CustomOperationIds(o =>
    {
        var action = (ControllerActionDescriptor)o.ActionDescriptor;
        return $"{action.ActionName}".ToCamelCase();
    });
    builder.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    builder.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Synapse Correlator REST API",
        Version = "v1",
        Description = "The Open API documentation for the Synapse Correlator REST API",
        License = new OpenApiLicense()
        {
            Name = "Apache-2.0",
            Url = new("https://raw.githubusercontent.com/synapse/dsl/main/LICENSE")
        },
        Contact = new()
        {
            Name = "The Synapse Authors",
            Url = new Uri("https://github.com/serverlessworkflow/synapse")
        }
    });
    builder.IncludeXmlComments(typeof(Workflow).Assembly.Location.Replace(".dll", ".xml"));
});

using var app = builder.Build();

app.UseResponseCompression();
app.UseCloudEvents();
app.UseRouting();
app.UseExceptionHandler(handler =>
{
    handler.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var problemDetails = exceptionHandlerPathFeature?.Error switch
        {
            ProblemDetailsException problemDetailsException => problemDetailsException.Problem,
            _ => new Neuroglia.ProblemDetails(ErrorType.Runtime, ErrorTitle.Runtime, ErrorStatus.Runtime, exceptionHandlerPathFeature?.Error.Message)
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

await app.RunAsync();

/// <summary>
/// The Correlator's program
/// </summary>
public partial class Program { }