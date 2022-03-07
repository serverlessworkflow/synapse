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

using Microsoft.AspNetCore.OData;
using Neuroglia.Caching;
using Neuroglia.Data.Expressions.JQ;
using ProtoBuf.Grpc.Server;
using Swashbuckle.AspNetCore.SwaggerUI;
using Synapse.Apis.Management.Grpc;
using Synapse.Apis.Management.Http;
using Synapse.Apis.Runtime.Grpc.Services;
using Synapse.Application.Configuration;
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
        .UseDockerRuntimeHost()
        .UseHttpManagementApi()
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
app.UseSwagger(builder =>
{
    builder.RouteTemplate = "api/{documentName}/doc/oas.{json|yaml}";
});
app.UseSwaggerUI(builder =>
{
    builder.DocExpansion(DocExpansion.None);
    builder.SwaggerEndpoint("/api/v1/doc/oas.json", "Synapse API v1");
    builder.RoutePrefix = "api/doc";
});
app.MapControllers();
app.MapGrpcService<SynapseGrpcManagementApi>();
app.MapGrpcService<SynapseGrpcRuntimeApi>();
app.MapHub<SynapseWebSocketApi>("/api/ws");
app.MapFallbackToFile("index.html");
app.MapFallbackToFile("/workflows/{param?}", "index.html");
app.MapFallbackToFile("/workflow-instances/{param?}", "index.html");
await app.RunAsync();