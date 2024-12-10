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

using Neuroglia;
using Neuroglia.Data.Expressions;
using Semver;

namespace Synapse.Runner.Services.Executors;

/// <summary>
/// Represents an <see cref="ITaskExecutor"/> implementation used to execute function <see cref="CallDefinition"/>s
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="executionContextFactory">The service used to create <see cref="ITaskExecutionContext"/>s</param>
/// <param name="executorFactory">The service used to create <see cref="ITaskExecutor"/>s</param>
/// <param name="context">The current <see cref="ITaskExecutionContext"/></param>
/// <param name="schemaHandlerProvider">The service used to provide <see cref="ISchemaHandler"/> implementations</param>
/// <param name="jsonSerializer">The service used to serialize/deserialize objects to/from JSON</param>
/// <param name="yamlSerializer">The service used to serialize/deserialize objects to/from YAML</param>
/// <param name="httpClientFactory">The service used to create <see cref="HttpClient"/>s</param>
public class FunctionCallExecutor(IServiceProvider serviceProvider, ILogger<FunctionCallExecutor> logger, ITaskExecutionContextFactory executionContextFactory,
    ITaskExecutorFactory executorFactory, ITaskExecutionContext<CallTaskDefinition> context, ISchemaHandlerProvider schemaHandlerProvider, IJsonSerializer jsonSerializer, IYamlSerializer yamlSerializer, IHttpClientFactory httpClientFactory)
    : TaskExecutor<CallTaskDefinition>(serviceProvider, logger, executionContextFactory, executorFactory, context, schemaHandlerProvider, jsonSerializer)
{

    const string CustomFunctionDefinitionFile = "function.yaml";
    const string GithubHost = "github.com";
    const string GitlabHost = "gitlab";

    /// <summary>
    /// Gets the service used to serialize/deserialize objects to/from YAML
    /// </summary>
    protected IYamlSerializer YamlSerializer { get; } = yamlSerializer;

    /// <summary>
    /// Gets the <see cref="System.Net.Http.HttpClient"/> used to perform HTTP requests
    /// </summary>
    protected HttpClient HttpClient { get; } = httpClientFactory.CreateClient();

    /// <summary>
    /// Gets the definition of the function to execute
    /// </summary>
    protected TaskDefinition? Function { get; private set; }

    /// <inheritdoc/>
    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await base.InitializeAsync(cancellationToken).ConfigureAwait(false);
        if (this.Task.Workflow.Definition.Use?.Functions?.TryGetValue(this.Task.Definition.Call, out var function) == true && function != null) this.Function = function;
        else if (Uri.TryCreate(this.Task.Definition.Call, UriKind.Absolute, out var uri) && (uri.IsFile || !string.IsNullOrWhiteSpace(uri.Host))) this.Function = await this.GetCustomFunctionAsync(new() { Uri = uri }, cancellationToken).ConfigureAwait(false);
        else if (this.Task.Definition.Call.Contains('@'))
        {
            var components = this.Task.Definition.Call.Split('@', StringSplitOptions.RemoveEmptyEntries);
            if (components.Length != 2) throw new NotSupportedException($"Unknown/unsupported function '{this.Task.Definition.Call}'");
            this.Function = await this.GetCustomFunctionAsync(components[0], components[1], cancellationToken).ConfigureAwait(false);
        }
        else throw new NotSupportedException($"Unknown/unsupported function '{this.Task.Definition.Call}'");
    }

    /// <summary>
    /// Gets the custom function at the specified uri
    /// </summary>
    /// <param name="endpoint">The uri of that references the custom function to get</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="TaskDefinition"/> of the custom function defined at the specified uri</returns>
    protected virtual async Task<TaskDefinition> GetCustomFunctionAsync(EndpointDefinition endpoint, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        var uri = endpoint.Uri;
        if (!uri.OriginalString.EndsWith(CustomFunctionDefinitionFile)) uri = new Uri(uri, CustomFunctionDefinitionFile);
        if (uri.Host.Equals(GithubHost, StringComparison.OrdinalIgnoreCase)) uri = this.TransformGithubUriToRawUri(uri);
        else if (uri.Host.Contains(GitlabHost)) uri = this.TransformGitlabUriToRawUri(uri);
        var authentication = endpoint.Authentication == null ? null : await this.Task.Workflow.Expressions.EvaluateAsync<AuthenticationPolicyDefinition>(endpoint.Authentication, this.Task.Input, this.Task.Arguments, cancellationToken).ConfigureAwait(false);
        using var httpClient = this.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient();
        await httpClient.ConfigureAuthenticationAsync(authentication, this.ServiceProvider, this.Task.Workflow.Definition, cancellationToken).ConfigureAwait(false);
        try
        {
            using var response = await httpClient.GetAsync(uri, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var yaml = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var function = this.YamlSerializer.Deserialize<TaskDefinition>(yaml)!;
            return function;
        }
        catch(Exception ex)
        {
            throw new ProblemDetailsException(new(ErrorType.Communication, ErrorTitle.Communication, ErrorStatus.Communication, $"Failed to load the custom function defined at '{endpoint}': {ex.Message}"));
        }
    }

    /// <summary>
    /// Gets the custom function with the specified name from the specified catalog
    /// </summary>
    /// <param name="functionName">The name of the custom function to get</param>
    /// <param name="catalogName">The name of the catalog to get the custom function from</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="TaskDefinition"/> of the custom function with the specified name</returns>
    protected virtual async Task<TaskDefinition> GetCustomFunctionAsync(string functionName, string catalogName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(functionName);
        ArgumentException.ThrowIfNullOrWhiteSpace(catalogName);
        var components = functionName.Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (components.Length != 2) throw new Exception($"The specified value '{functionName}' is not a valid custom function qualified name ({{name}}:{{version}})");
        var name = components[0];
        var version = components[1];
        if (!SemVersion.TryParse(version, SemVersionStyles.Strict, out _)) throw new Exception($"The specified value '{version}' is not a valid semantic version 2.0");
        if (catalogName == SynapseDefaults.Tasks.CustomFunctions.Catalogs.Default)
        {
            var function = await this.Task.Workflow.CustomFunctions.GetAsync(name, cancellationToken).ConfigureAwait(false) ?? throw new NullReferenceException($"Failed to find the specified custom function '{name}'");
            return function.Spec.Versions.Get(version) ?? throw new NullReferenceException($"Failed to find the version '{version}' of the custom function '{name}'");
        }
        else
        {
            if (this.Task.Workflow.Definition.Use?.Catalogs?.TryGetValue(catalogName, out var catalog) != true || catalog == null) throw new NullReferenceException($"Failed to find a catalog with the specified name '{catalogName}'");
            return await this.GetCustomFunctionAsync(new()
            {
                Uri = new(catalog.Endpoint.Uri, $"/functions/{name}/{version}"),
                Authentication = catalog.Endpoint.Authentication
            }, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Transforms the specified Github content <see cref="Uri"/> into a Github raw content <see cref="Uri"/>
    /// </summary>
    /// <param name="uri">The <see cref="Uri"/> to transform</param>
    /// <returns>The Github raw content <see cref="Uri"/></returns>
    protected virtual Uri TransformGithubUriToRawUri(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        if (uri.Host.Equals(GithubHost, StringComparison.OrdinalIgnoreCase)) return uri;
        var rawUri = uri.AbsoluteUri.Replace(GithubHost, "raw.githubusercontent.com", StringComparison.OrdinalIgnoreCase);
        rawUri = rawUri.Replace("/tree/", "/refs/heads/", StringComparison.OrdinalIgnoreCase);
        return new(rawUri, UriKind.Absolute);
    }

    /// <summary>
    /// Transforms the specified Gitlab content <see cref="Uri"/> into a Gitlab raw content <see cref="Uri"/>
    /// </summary>
    /// <param name="uri">The <see cref="Uri"/> to transform</param>
    /// <returns>The Gitlab raw content <see cref="Uri"/></returns>
    protected virtual Uri TransformGitlabUriToRawUri(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        if (!uri.AbsoluteUri.Contains(GitlabHost, StringComparison.OrdinalIgnoreCase)) return uri;
        var rawUri = uri.AbsoluteUri.Replace("/-/blob/", "/-/raw/", StringComparison.OrdinalIgnoreCase);
        return new(rawUri, UriKind.Absolute);
    }

    /// <inheritdoc/>
    protected override async Task<ITaskExecutor> CreateTaskExecutorAsync(TaskInstance task, TaskDefinition definition, IDictionary<string, object> contextData, IDictionary<string, object>? arguments = null, CancellationToken cancellationToken = default)
    {
        var executor = await base.CreateTaskExecutorAsync(task, definition, contextData, this.Task.Arguments, cancellationToken).ConfigureAwait(false);
        executor.SubscribeAsync(
            _ => System.Threading.Tasks.Task.CompletedTask,
            async ex => await this.OnSubTaskFaultAsync(executor, this.CancellationTokenSource?.Token ?? default).ConfigureAwait(false));
        return executor;
    }

    /// <inheritdoc/>
    protected override async Task DoExecuteAsync(CancellationToken cancellationToken)
    {
        if (this.Function == null) throw new InvalidOperationException("The executor must be initialized before execution");
        var input = this.Task.Definition.With == null ? [] : await this.Task.Workflow.Expressions.EvaluateAsync<EquatableDictionary<string, object>?>(this.Task.Definition.With, this.Task.Input, this.GetExpressionEvaluationArguments(), cancellationToken).ConfigureAwait(false) ?? [];
        var task = await this.Task.Workflow.CreateTaskAsync(this.Function, null, input, null, this.Task, false, cancellationToken).ConfigureAwait(false);
        var executor = await this.CreateTaskExecutorAsync(task, this.Function, this.Task.ContextData, this.Task.Arguments, cancellationToken).ConfigureAwait(false);
        await executor.ExecuteAsync(cancellationToken).ConfigureAwait(false);
        await this.SetResultAsync(executor.Task.Output, this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles error raised during a subtask's execution
    /// </summary>
    /// <param name="executor">The service used to execute sub tasks</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task OnSubTaskFaultAsync(ITaskExecutor executor, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(executor);
        var error = executor.Task.Instance.Error ?? throw new NullReferenceException();
        this.Executors.Remove(executor);
        await this.SetErrorAsync(error, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override ValueTask DisposeAsync(bool disposing)
    {
        this.HttpClient.Dispose();
        return base.DisposeAsync(disposing);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        this.HttpClient.Dispose();
        base.Dispose(disposing);
    }

}