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

using System.Net;
using System.Runtime.InteropServices;

namespace Synapse.Runner.Services.Executors;

/// <summary>
/// Represents an <see cref="ITaskExecutor"/> implementation used to execute <see cref="ShellProcessDefinition"/>s
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="executionContextFactory">The service used to create <see cref="ITaskExecutionContext"/>s</param>
/// <param name="executorFactory">The service used to create <see cref="ITaskExecutor"/>s</param>
/// <param name="context">The current <see cref="ITaskExecutionContext"/></param>
/// <param name="serializer">The service used to serialize/deserialize objects to/from JSON</param>
public class ShellProcessExecutor(IServiceProvider serviceProvider, ILogger<ShellProcessExecutor> logger, ITaskExecutionContextFactory executionContextFactory, ITaskExecutorFactory executorFactory, ITaskExecutionContext<RunTaskDefinition> context, IJsonSerializer serializer)
    : TaskExecutor<RunTaskDefinition>(serviceProvider, logger, executionContextFactory, executorFactory, context, serializer)
{

    /// <summary>
    /// Gets the definition of the shell process to run
    /// </summary>
    protected ShellProcessDefinition ProcessDefinition => this.Task.Definition.Run.Shell!;

    /// <inheritdoc/>
    protected override async Task DoExecuteAsync(CancellationToken cancellationToken)
    {
        var fileInfo = string.Empty;
        var arguments = this.ProcessDefinition.Arguments ?? [];
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) arguments[0] = "-c";
        else arguments[0] = "/c";
        var startInfo = new ProcessStartInfo(fileInfo, arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        using var process = Process.Start(startInfo) ?? throw new NullReferenceException($"Failed to create the shell process defined at '{this.Task.Instance.Reference}'");
        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        var rawOutput = (await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false)).Trim();
        var errorMessage = (await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false)).Trim();
        if(process.ExitCode == 0) await this.SetResultAsync(rawOutput, this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
        else await this.SetErrorAsync(new()
        {
            Status = (int)HttpStatusCode.InternalServerError,
            Type = ErrorType.Runtime,
            Title = ErrorTitle.Runtime,
            Detail = errorMessage,
            Instance = this.Task.Instance.Reference
        }, cancellationToken).ConfigureAwait(false);
    }

}
