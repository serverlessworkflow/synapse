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

using DynamicGrpc;
using Google.Protobuf.Reflection;
using Grpc.Core;
using Neuroglia.Data.Expressions;

namespace Synapse.Runner.Services.Executors;

/// <summary>
/// Represents an <see cref="ITaskExecutor"/> used to execute http <see cref="GrpcCallDefinition"/>s
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="executionContextFactory">The service used to create <see cref="ITaskExecutionContext"/>s</param>
/// <param name="executorFactory">The service used to create <see cref="ITaskExecutor"/>s</param>
/// <param name="context">The current <see cref="ITaskExecutionContext"/></param>
/// <param name="serializer">The service used to serialize/deserialize objects to/from JSON</param>
/// <param name="externalResources">The service used to provide external resources</param>
public class GrpcCallExecutor(IServiceProvider serviceProvider, ILogger<GrpcCallExecutor> logger, ITaskExecutionContextFactory executionContextFactory, ITaskExecutorFactory executorFactory, ITaskExecutionContext<CallTaskDefinition> context, IJsonSerializer serializer, IExternalResourceProvider externalResources)
    : TaskExecutor<CallTaskDefinition>(serviceProvider, logger, executionContextFactory, executorFactory, context, serializer)
{

    /// <summary>
    /// Gets the service used to provide external resources
    /// </summary>
    protected IExternalResourceProvider ExternalResources { get; } = externalResources;
    
    /// <summary>
    /// Gets the definition of the GRPC call to perform
    /// </summary>
    protected GrpcCallDefinition? Grpc { get; set; }

    /// <summary>
    /// Gets the service used to perform GRPC calls
    /// </summary>
    protected DynamicGrpcClient? GrpcClient { get; set; }

    /// <inheritdoc/>
    protected override async Task DoInitializeAsync(CancellationToken cancellationToken)
    {
        try
        {
            this.Grpc = (GrpcCallDefinition)this.JsonSerializer.Convert(this.Task.Definition.With, typeof(GrpcCallDefinition))!;
            var fileDescriptor = await this.GetProtoFileDescriptorAsync(this.Grpc.Proto, cancellationToken).ConfigureAwait(false);
            var channelCredentials = ChannelCredentials.Insecure; //todo
            var channel = this.Grpc.Service.Port.HasValue
                ? new Channel(this.Grpc.Service.Host, this.Grpc.Service.Port.Value, channelCredentials)
                : new Channel(this.Grpc.Service.Host, channelCredentials);
            var callInvoker = new DefaultCallInvoker(channel);
            this.GrpcClient = DynamicGrpcClient.FromDescriptorProtos(callInvoker: callInvoker, [fileDescriptor]);
        }
        catch (ErrorRaisedException ex) { await this.SetErrorAsync(ex.Error, cancellationToken).ConfigureAwait(false); }
        catch (Exception ex)
        {
            await this.SetErrorAsync(new()
            {
                Status = ErrorStatus.Validation,
                Type = ErrorType.Validation,
                Title = ErrorTitle.Validation,
                Detail = $"Invalid/missing call parameters for function 'grpc': {ex.Message}"
            }, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    protected override async Task DoExecuteAsync(CancellationToken cancellationToken)
    {
        if (this.Grpc == null || this.GrpcClient == null) throw new InvalidOperationException("The executor must be initialized before execution");
        if (!this.GrpcClient.TryFindMethod(this.Grpc.Service.Name, this.Grpc.Method, out _)) 
        {
            await this.SetErrorAsync(Error.Configuration(this.Task.Instance.Reference, $"Failed to find a method with name '{this.Grpc.Method}' in GRPC service with name '{this.Grpc.Service.Name}'"), cancellationToken).ConfigureAwait(false);
            return;
        }
        var request = (await this.Task.Workflow.Expressions.EvaluateAsync<IDictionary<string, object>>(this.Grpc.Arguments ?? new Dictionary<string, object>(), this.Task.Input, this.GetExpressionEvaluationArguments(), cancellationToken).ConfigureAwait(false))!;
        IDictionary<string, object> response;
        try
        {
            response = await this.GrpcClient.AsyncUnaryCall(this.Grpc.Service.Name, this.Grpc.Method, request).ConfigureAwait(false);
        }
        catch(Exception ex)
        {
            this.Logger.LogError("Failed to call the GRPC method '{method}' on '{service}' service at '{host}:{port}': {ex}", this.Grpc.Method, this.Grpc.Service.Name, this.Grpc.Service.Host, this.Grpc.Service.Port, ex.Message);
            await this.SetErrorAsync(Error.Communication(this.Task.Instance.Reference, ErrorStatus.Communication, ex.Message), cancellationToken).ConfigureAwait(false);
            return;
        }
        await this.SetResultAsync(response, this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the <see cref="FileDescriptorProto"/> for the specified proto resource file
    /// </summary>
    /// <param name="resource">A reference to the proto resource file to get the <see cref="FileDescriptorProto"/> for</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="FileDescriptorProto"/> for the specified proto resource file</returns>
    protected virtual async Task<FileDescriptorProto> GetProtoFileDescriptorAsync(ExternalResourceDefinition resource, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(resource);
        var protoFile = new FileInfo(Path.GetTempFileName());
        var protoDescriptorFileName = Path.Combine(protoFile.Directory!.FullName, $"{Path.GetFileNameWithoutExtension(protoFile.Name)}.desc");
        using var stream = await this.ExternalResources.ReadAsync(this.Task.Workflow.Definition, resource, cancellationToken).ConfigureAwait(false);
        {
            using var protoFileStream = new FileStream(protoFile.FullName, FileMode.Create);
            {
                await stream.CopyToAsync(protoFileStream, cancellationToken).ConfigureAwait(false);
                await protoFileStream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        var processInfo = new ProcessStartInfo("protoc", $"{protoFile.FullName} --proto_path ={protoFile.Directory!.FullName} --descriptor_set_out={protoDescriptorFileName}")
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };
        using var process = Process.Start(processInfo)!;
        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        using var protoDescriptorFileStream = new FileStream(protoDescriptorFileName, FileMode.Open);
        var fileDescriptorSet = FileDescriptorSet.Parser.ParseFrom(protoDescriptorFileStream);
        return fileDescriptorSet.File.First();
    }

}
