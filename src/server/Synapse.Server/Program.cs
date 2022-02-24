using Microsoft.AspNetCore.OData;
using Neuroglia.Caching;
using Neuroglia.Data.Expressions.JQ;
using ProtoBuf.Grpc.Server;
using Synapse.Application.Configuration;
using Synapse.Ports.Grpc.Services;
using Synapse.Ports.HttpRest;
using Synapse.Ports.WebSockets;
using Synapse.Ports.WebSockets.Services;
using Synapse.Runtime.Docker;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogging(builder =>
{
    builder.AddSimpleConsole(options =>
    {
        options.TimestampFormat = "[MM/dd/yyyy HH:mm:ss] ";
    });
});
builder.Services.AddMemoryDistributedCache();
builder.Services.AddCodeFirstGrpc();
builder.Services.AddSynapse(builder.Configuration, synapse =>
{
    synapse
        .UseConfiguration(builder.Configuration)
        .UseDockerRuntimeHost()
        .UseHttpRestApi()
        .UseWebSocketApi();
});
builder.Services.AddJQExpressionEvaluator();
using var app = builder.Build();
if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/error");
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseODataRouteDebug();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<SynapseGrpcApi>();
app.MapGrpcService<SynapseGrpcRuntimeApi>();
app.MapHub<SynapseWebSocketApi>("/api/ws");
app.MapFallbackToFile("index.html");
app.MapFallbackToFile("/workflows/{param?}", "index.html");
app.MapFallbackToFile("/workflow-instances/{param?}", "index.html");
await app.RunAsync();