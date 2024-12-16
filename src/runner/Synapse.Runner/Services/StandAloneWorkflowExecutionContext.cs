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

using Json.Pointer;
using Moq;
using Neuroglia;
using Neuroglia.Data.Expressions;
using Neuroglia.Data.Infrastructure.ResourceOriented;
using Synapse.Events.Tasks;
using Synapse.Events.Workflows;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace Synapse.Runner.Services;

/// <summary>
/// Represents a stand-alone, in-memory implementation of the <see cref="IWorkflowExecutionContext"/> interface
/// </summary>
/// <param name="services">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="expressionEvaluator">The service used to evaluate runtime expressions</param>
/// <param name="jsonSerializer">The service used to serialize/deserialize data to/from JSON</param>
/// <param name="yamlSerializer">The service used to serialize/deserialize data to/from YAML</param>
/// <param name="options">The service used to access the current <see cref="RunnerOptions"/></param>
/// <param name="httpClient">The service used to perform HTTP requests</param>
/// <param name="definition">The <see cref="WorkflowDefinition"/> of the <see cref="WorkflowInstance"/> to execute</param>
/// <param name="instance">The <see cref="WorkflowInstance"/> to execute</param>
public class StandAloneWorkflowExecutionContext(IServiceProvider services, ILogger<IWorkflowExecutionContext> logger, IExpressionEvaluator expressionEvaluator, IJsonSerializer jsonSerializer, IYamlSerializer yamlSerializer, IOptions<RunnerOptions> options, HttpClient httpClient, WorkflowDefinition definition, WorkflowInstance instance)
    : IWorkflowExecutionContext
{

    readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    /// <inheritdoc/>
    public IServiceProvider Services { get; } = services;

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger Logger { get; } = logger;

    /// <inheritdoc/>
    public IExpressionEvaluator Expressions { get; } = expressionEvaluator;

    /// <summary>
    /// Gets the service used to serialize/deserialize data to/from JSON
    /// </summary>
    protected IJsonSerializer JsonSerializer { get; } = jsonSerializer;

    /// <summary>
    /// Gets the service used to serialize/deserialize data to/from YAML
    /// </summary>
    protected IYamlSerializer YamlSerializer { get; } = yamlSerializer;

    /// <summary>
    /// Gets the service used to perform HTTP requests
    /// </summary>
    protected HttpClient HttpClient { get; } = httpClient;

    /// <summary>
    /// Gets the current <see cref="RunnerOptions"/>
    /// </summary>
    protected RunnerOptions Options { get; } = options.Value;

    /// <inheritdoc/>
    public WorkflowDefinition Definition { get; } = definition;

    /// <inheritdoc/>
    public WorkflowInstance Instance { get; set; } = instance;

    /// <inheritdoc/>
    public IDocumentApiClient Documents { get; } = new MemoryDocumentManager();

    /// <inheritdoc/>
    public IClusterResourceApiClient<CustomFunction> CustomFunctions => Mock.Of<IClusterResourceApiClient<CustomFunction>>();

    /// <summary>
    /// Gets the object used to asynchronously lock the <see cref="TaskExecutor{TDefinition}"/>
    /// </summary>
    protected AsyncLock Lock { get; } = new();

    /// <inheritdoc/>
    public IDictionary<string, object> ContextData { get; protected set; } = new Dictionary<string, object>();

    /// <inheritdoc/>
    public IDictionary<string, object> Arguments { get; protected set; } = new Dictionary<string, object>();

    /// <inheritdoc/>
    public object? Output { get; protected set; }

    /// <inheritdoc/>
    public virtual Task ContinueWithAsync(TaskDefinition task, CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <inheritdoc/>
    public virtual async Task<TaskInstance> CreateTaskAsync(TaskDefinition definition, string? path, object input, IDictionary<string, object>? context = null, ITaskExecutionContext? parent = null, bool isExtension = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);
        using var @lock = await this.Lock.LockAsync(cancellationToken).ConfigureAwait(false);
        input ??= new();
        var name = string.IsNullOrWhiteSpace(path)
            ? parent?.Instance.Reference?.OriginalString.Split('/', StringSplitOptions.RemoveEmptyEntries).Last()
            : path.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
        var reference = this.Definition.BuildReferenceTo(definition, path, parent?.Instance.Reference);
        var contextReference = string.Empty;
        if (context == null)
        {
            if (parent?.ContextData != null)
            {
                contextReference = parent.Instance.ContextReference;
                context = parent.ContextData;
            }
            else if (!string.IsNullOrWhiteSpace(this.Instance.Status?.ContextReference))
            {
                contextReference = this.Instance.Status?.ContextReference;
                var contextDocument = await this.Documents.GetAsync(this.Instance.Status!.ContextReference, cancellationToken).ConfigureAwait(false);
                context = contextDocument?.Content.ConvertTo<IDictionary<string, object>>() ?? new Dictionary<string, object>();
            }
            else throw new NullReferenceException($"Failed to find the data document with id '{this.Instance.Status!.ContextReference}'");
        }
        else
        {
            var contextDocument = await this.Documents.CreateAsync($"{reference}/input", context, cancellationToken).ConfigureAwait(false);
            contextReference = contextDocument.Id;
        }
        var filteredInput = input;
        var evaluationArguments = new Dictionary<string, object>()
        {
            { RuntimeExpressions.Arguments.Runtime, RuntimeDescriptor.Current },
            { RuntimeExpressions.Arguments.Workflow, this.GetDescriptor() },
            { RuntimeExpressions.Arguments.Context, context }
        };
        if (definition.Input?.From is string fromExpression) filteredInput = (await this.Expressions.EvaluateAsync<object>(fromExpression, input, evaluationArguments, cancellationToken).ConfigureAwait(false))!;
        else if (definition.Input?.From != null) filteredInput = (await this.Expressions.EvaluateAsync<object>(definition.Input.From, input, evaluationArguments, cancellationToken).ConfigureAwait(false))!;
        var inputDocument = await this.Documents.CreateAsync($"{reference}/input", filteredInput, cancellationToken).ConfigureAwait(false);
        var task = new TaskInstance()
        {
            Name = name,
            Reference = reference,
            IsExtension = isExtension,
            ParentId = parent?.Instance.Id,
            InputReference = inputDocument.Id,
            ContextReference = contextReference
        };
        var pointer = JsonPointer.Create<WorkflowInstance>(w => w.Status!.Tasks!).ToCamelCase();
        if (this.Instance.Status?.Tasks != null) pointer = JsonPointer.Parse($"{pointer}/-");
        this.Instance.Status ??= new();
        this.Instance.Status.Tasks ??= [];
        this.Instance.Status.Tasks.Add(task);
        if (this.Options.CloudEvents.PublishLifecycleEvents) await this.PublishAsync(new CloudEvent()
        {
            SpecVersion = CloudEventSpecVersion.V1.Version,
            Id = Guid.NewGuid().ToString(),
            Time = DateTimeOffset.Now,
            Source = this.Options.Api.BaseAddress,
            Type = SynapseDefaults.CloudEvents.Task.Created.v1,
            Subject = this.Instance.GetQualifiedName(),
            DataContentType = MediaTypeNames.Application.Json,
            Data = new TaskCreatedEventV1()
            {
                Workflow = this.Instance.GetQualifiedName(),
                Task = task.Reference,
                CreatedAt = task.CreatedAt
            }
        }, cancellationToken).ConfigureAwait(false);
        this.Logger.LogInformation("Task '{reference}' created.", reference);
        return task;
    }

    /// <inheritdoc/>
    public virtual async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var document = await this.Documents.CreateAsync(this.Instance.GetQualifiedName(), this.Instance.Spec.Input ?? [], cancellationToken).ConfigureAwait(false);
        this.Instance.Status = new()
        {
            ContextReference = document.Id
        };
        this.Logger.LogInformation("Workflow initialized.");
    }

    /// <inheritdoc/>
    public virtual Task<TaskInstance> InitializeAsync(TaskInstance task, CancellationToken cancellationToken = default)
    {
        //task = this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
        //task.Status = TaskInstanceStatus.Initializing;
        this.Logger.LogInformation("Task '{reference}' initialized.", task.Reference);
        return Task.FromResult(task);
    }

    /// <inheritdoc/>
    public virtual async Task StartAsync(CancellationToken cancellationToken = default)
    {
        using var @lock = await this.Lock.LockAsync(cancellationToken).ConfigureAwait(false);
        this.Instance.Status ??= new();
        this.Instance.Status.Phase = WorkflowInstanceStatusPhase.Running;
        this.Instance.Status.StartedAt ??= DateTimeOffset.Now;
        this.Instance.Status.Runs ??= [];
        this.Instance.Status.Runs.Add(new() { StartedAt = DateTimeOffset.Now });
        this.Instance.Status.ContextReference ??= (await this.Documents.CreateAsync(this.Instance.GetQualifiedName(), this.ContextData, cancellationToken).ConfigureAwait(false)).Id;
        if (this.Options.CloudEvents.PublishLifecycleEvents) await this.PublishAsync(new CloudEvent()
        {
            SpecVersion = CloudEventSpecVersion.V1.Version,
            Id = Guid.NewGuid().ToString(),
            Time = DateTimeOffset.Now,
            Source = this.Options.Api.BaseAddress,
            Type = SynapseDefaults.CloudEvents.Workflow.Started.v1,
            Subject = this.Instance.GetQualifiedName(),
            DataContentType = MediaTypeNames.Application.Json,
            Data = new WorkflowStartedEventV1()
            {
                Name = this.Instance.GetQualifiedName(),
                Definition = this.Instance.Spec.Definition,
                StartedAt = this.Instance.Status?.StartedAt ?? DateTimeOffset.Now
            }
        }, cancellationToken).ConfigureAwait(false);
        this.Logger.LogInformation("Workflow started.");
    }

    /// <inheritdoc/>
    public virtual async Task<TaskInstance> StartAsync(TaskInstance task, CancellationToken cancellationToken = default)
    {
        using var @lock = await this.Lock.LockAsync(cancellationToken).ConfigureAwait(false);
        task = this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
        task.Status = TaskInstanceStatus.Running;
        task.StartedAt ??= DateTimeOffset.Now;
        task.Runs ??= [];
        task.Runs.Add(new() { StartedAt = DateTimeOffset.Now });
        if (this.Options.CloudEvents.PublishLifecycleEvents) await this.PublishAsync(new CloudEvent()
        {
            SpecVersion = CloudEventSpecVersion.V1.Version,
            Id = Guid.NewGuid().ToString(),
            Time = DateTimeOffset.Now,
            Source = this.Options.Api.BaseAddress,
            Type = SynapseDefaults.CloudEvents.Task.Started.v1,
            Subject = this.Instance.GetQualifiedName(),
            DataContentType = MediaTypeNames.Application.Json,
            Data = new TaskStartedEventV1()
            {
                Workflow = this.Instance.GetQualifiedName(),
                Task = task.Reference,
                StartedAt = task.StartedAt ?? DateTimeOffset.Now
            }
        }, cancellationToken).ConfigureAwait(false);
        this.Logger.LogInformation("Task '{reference}' started.", task.Reference);
        return this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
    }

    /// <inheritdoc/>
    public virtual async Task ResumeAsync(CancellationToken cancellationToken = default)
    {
        using var @lock = await this.Lock.LockAsync(cancellationToken).ConfigureAwait(false);
        this.Instance.Status ??= new();
        this.Instance.Status.Phase = WorkflowInstanceStatusPhase.Running;
        this.Instance.Status.StartedAt ??= DateTimeOffset.Now;
        this.Instance.Status.Runs ??= [];
        this.Instance.Status.Runs.Add(new() { StartedAt = DateTimeOffset.Now });
        this.ContextData = (await this.Documents.GetAsync(this.Instance.Status!.ContextReference, cancellationToken).ConfigureAwait(false)).Content.ConvertTo<IDictionary<string, object>>()!;
        if (this.Options.CloudEvents.PublishLifecycleEvents) await this.PublishAsync(new CloudEvent()
        {
            SpecVersion = CloudEventSpecVersion.V1.Version,
            Id = Guid.NewGuid().ToString(),
            Time = DateTimeOffset.Now,
            Source = this.Options.Api.BaseAddress,
            Type = SynapseDefaults.CloudEvents.Workflow.Resumed.v1,
            Subject = this.Instance.GetQualifiedName(),
            DataContentType = MediaTypeNames.Application.Json,
            Data = new WorkflowResumedEventV1()
            {
                Name = this.Instance.GetQualifiedName(),
                ResumedAt = this.Instance.Status?.Runs?.LastOrDefault()?.StartedAt ?? DateTimeOffset.Now
            }
        }, cancellationToken).ConfigureAwait(false);
        this.Logger.LogInformation("The workflow's execution has been resumed.");
    }

    /// <inheritdoc/>
    public virtual Task<CorrelationContext> CorrelateAsync(ITaskExecutionContext task, CancellationToken cancellationToken = default) => throw new NotSupportedException("Event correlation is not supported in stand-alone execution mode");

    /// <inheritdoc/>
    public virtual async Task PublishAsync(CloudEvent e, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(e);
        if (this.Options.CloudEvents.Sink == null) return;
        try
        {
            var json = this.JsonSerializer.SerializeToText(e);
            using var request = new HttpRequestMessage(HttpMethod.Post, this.Options.CloudEvents.Sink)
            {
                Content = new StringContent(json, Encoding.UTF8, CloudEventContentType.Json)
            };
            using var response = await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            this.Logger.LogError("An error occurred while publishing a cloud event to the configured sink: {ex}", ex);
        }
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TaskInstance> GetTasksAsync(CancellationToken cancellationToken = default) => (this.Instance.Status?.Tasks ?? []).ToAsyncEnumerable();

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TaskInstance> GetTasksAsync(TaskInstance task, CancellationToken cancellationToken = default) => this.GetTasksAsync(cancellationToken).Where(t => t.ParentId == task.Id);

    /// <inheritdoc/>
    public virtual async Task<TaskInstance> RetryAsync(TaskInstance task, Error cause, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(cause);
        using var @lock = await this.Lock.LockAsync(cancellationToken).ConfigureAwait(false);
        this.Logger.LogInformation("Retrying task '{reference}'...", task.Reference);
        task = this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
        task.Retries ??= [];
        task.Retries.Add(new RetryAttempt()
        {
            Number = (uint)task.Retries.Count + 1,
            Cause = cause
        });
        task.Status = TaskInstanceStatus.Running;
        task.StartedAt ??= DateTimeOffset.Now;
        task.Runs ??= [];
        task.Runs.Add(new() { StartedAt = DateTimeOffset.Now });
        if (this.Options.CloudEvents.PublishLifecycleEvents) await this.PublishAsync(new CloudEvent()
        {
            SpecVersion = CloudEventSpecVersion.V1.Version,
            Id = Guid.NewGuid().ToString(),
            Time = DateTimeOffset.Now,
            Source = this.Options.Api.BaseAddress,
            Type = SynapseDefaults.CloudEvents.Task.Retrying.v1,
            Subject = this.Instance.GetQualifiedName(),
            DataContentType = MediaTypeNames.Application.Json,
            Data = new RetryingTaskEventV1()
            {
                Workflow = this.Instance.GetQualifiedName(),
                Task = task.Reference,
                RetryingAt = task.Runs?.LastOrDefault()?.StartedAt ?? DateTimeOffset.Now
            }
        }, cancellationToken).ConfigureAwait(false);
        return this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
    }

    /// <inheritdoc/>
    public virtual async Task SetErrorAsync(Error error, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(error);
        using var @lock = await this.Lock.LockAsync(cancellationToken).ConfigureAwait(false);
        this.Instance.Status ??= new();
        this.Instance.Status.Phase = WorkflowInstanceStatusPhase.Faulted;
        this.Instance.Status.EndedAt = DateTimeOffset.Now;
        this.Instance.Status.Error = error;
        var run = this.Instance.Status.Runs?.LastOrDefault();
        if (run != null) run.EndedAt = DateTimeOffset.Now;
        if (this.Options.CloudEvents.PublishLifecycleEvents)
        {
            await this.PublishAsync(new CloudEvent()
            {
                SpecVersion = CloudEventSpecVersion.V1.Version,
                Id = Guid.NewGuid().ToString(),
                Time = DateTimeOffset.Now,
                Source = this.Options.Api.BaseAddress,
                Type = SynapseDefaults.CloudEvents.Workflow.Faulted.v1,
                Subject = this.Instance.GetQualifiedName(),
                DataContentType = MediaTypeNames.Application.Json,
                Data = new WorkflowFaultedEventV1()
                {
                    Name = this.Instance.GetQualifiedName(),
                    Error = error,
                    FaultedAt = run?.EndedAt ?? DateTimeOffset.Now
                }
            }, cancellationToken).ConfigureAwait(false);
            await this.PublishAsync(new CloudEvent()
            {
                SpecVersion = CloudEventSpecVersion.V1.Version,
                Id = Guid.NewGuid().ToString(),
                Time = DateTimeOffset.Now,
                Source = this.Options.Api.BaseAddress,
                Type = SynapseDefaults.CloudEvents.Workflow.Ended.v1,
                Subject = this.Instance.GetQualifiedName(),
                DataContentType = MediaTypeNames.Application.Json,
                Data = new WorkflowEndedEventV1()
                {
                    Name = this.Instance.GetQualifiedName(),
                    Status = this.Instance.Status!.Phase!,
                    EndedAt = run?.EndedAt ?? DateTimeOffset.Now
                }
            }, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public virtual async Task<TaskInstance> SetErrorAsync(TaskInstance task, Error error, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(error);
        using var @lock = await this.Lock.LockAsync(cancellationToken).ConfigureAwait(false);
        task = this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
        task.Status = TaskInstanceStatus.Faulted;
        task.EndedAt = DateTimeOffset.Now;
        task.Error = error;
        var run = task.Runs?.LastOrDefault();
        if (run != null)
        {
            run.EndedAt = DateTimeOffset.Now;
            run.Outcome = task.Status;
        }
        if (this.Options.CloudEvents.PublishLifecycleEvents)
        {
            await this.PublishAsync(new CloudEvent()
            {
                SpecVersion = CloudEventSpecVersion.V1.Version,
                Id = Guid.NewGuid().ToString(),
                Time = DateTimeOffset.Now,
                Source = this.Options.Api.BaseAddress,
                Type = SynapseDefaults.CloudEvents.Task.Faulted.v1,
                Subject = this.Instance.GetQualifiedName(),
                DataContentType = MediaTypeNames.Application.Json,
                Data = new TaskFaultedEventV1()
                {
                    Workflow = this.Instance.GetQualifiedName(),
                    Task = task.Reference,
                    Error = error,
                    FaultedAt = run?.EndedAt ?? DateTimeOffset.Now
                }
            }, cancellationToken).ConfigureAwait(false);
            await this.PublishAsync(new CloudEvent()
            {
                SpecVersion = CloudEventSpecVersion.V1.Version,
                Id = Guid.NewGuid().ToString(),
                Time = DateTimeOffset.Now,
                Source = this.Options.Api.BaseAddress,
                Type = SynapseDefaults.CloudEvents.Task.Ended.v1,
                Subject = this.Instance.GetQualifiedName(),
                DataContentType = MediaTypeNames.Application.Json,
                Data = new TaskEndedEventV1()
                {
                    Workflow = this.Instance.GetQualifiedName(),
                    Task = task.Reference,
                    Status = task.Status!,
                    EndedAt = run?.EndedAt ?? DateTimeOffset.Now
                }
            }, cancellationToken).ConfigureAwait(false);
        }
        return this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
    }

    /// <inheritdoc/>
    public virtual async Task SetResultAsync(object? result, CancellationToken cancellationToken = default)
    {
        using var @lock = await this.Lock.LockAsync(cancellationToken).ConfigureAwait(false);
        result ??= new();
        this.Instance.Status ??= new();
        this.Instance.Status.Phase = WorkflowInstanceStatusPhase.Completed;
        this.Instance.Status.EndedAt = DateTimeOffset.Now;
        var document = await this.Documents.CreateAsync($"{this.Instance.GetQualifiedName()}/output", result, cancellationToken).ConfigureAwait(false);
        this.Instance.Status.OutputReference = document.Id;
        this.Output = result;
        var run = this.Instance.Status.Runs?.LastOrDefault();
        if (run != null) run.EndedAt = DateTimeOffset.Now;
        if (this.Options.CloudEvents.PublishLifecycleEvents)
        {
            await this.PublishAsync(new CloudEvent()
            {
                SpecVersion = CloudEventSpecVersion.V1.Version,
                Id = Guid.NewGuid().ToString(),
                Time = DateTimeOffset.Now,
                Source = this.Options.Api.BaseAddress,
                Type = SynapseDefaults.CloudEvents.Workflow.Completed.v1,
                Subject = this.Instance.GetQualifiedName(),
                DataContentType = MediaTypeNames.Application.Json,
                Data = new WorkflowCompletedEventV1()
                {
                    Name = this.Instance.GetQualifiedName(),
                    CompletedAt = run?.EndedAt ?? DateTimeOffset.Now,
                    Output = this.Output
                }
            }, cancellationToken).ConfigureAwait(false);
            await this.PublishAsync(new CloudEvent()
            {
                SpecVersion = CloudEventSpecVersion.V1.Version,
                Id = Guid.NewGuid().ToString(),
                Time = DateTimeOffset.Now,
                Source = this.Options.Api.BaseAddress,
                Type = SynapseDefaults.CloudEvents.Workflow.Ended.v1,
                Subject = this.Instance.GetQualifiedName(),
                DataContentType = MediaTypeNames.Application.Json,
                Data = new WorkflowEndedEventV1()
                {
                    Name = this.Instance.GetQualifiedName(),
                    Status = this.Instance.Status!.Phase!,
                    EndedAt = run?.EndedAt ?? DateTimeOffset.Now
                }
            }, cancellationToken).ConfigureAwait(false);
        }
        var output = this.Options.Workflow.OutputFormat switch
        {
            WorkflowOutputFormat.Json => System.Text.Json.JsonSerializer.Serialize(this.Output, this._jsonSerializerOptions),
            WorkflowOutputFormat.Yaml => this.YamlSerializer.SerializeToText(this.Output),
            _ => throw new NotSupportedException($"The specified workflow output format '{this.Options.Workflow.OutputFormat}' is not supported"),
        };
        if (string.IsNullOrWhiteSpace(this.Options.Workflow.OutputFilePath)) Console.WriteLine(output);
        else await File.WriteAllTextAsync(this.Options.Workflow.OutputFilePath, output, cancellationToken).ConfigureAwait(false);
        this.Logger.LogInformation("Workflow successfully executed.");
    }

    /// <inheritdoc/>
    public virtual async Task<TaskInstance> SetResultAsync(TaskInstance task, object? result, string? then = FlowDirective.Continue, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        using var @lock = await this.Lock.LockAsync(cancellationToken).ConfigureAwait(false);
        result ??= new();
        task = this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
        task.Status = TaskInstanceStatus.Completed;
        task.EndedAt = DateTimeOffset.Now;
        var document = await this.Documents.CreateAsync($"{this.Instance.GetQualifiedName()}/{task.Reference}/output", result, cancellationToken).ConfigureAwait(false);
        task.OutputReference = document.Id;
        task.Next = then;
        var run = task.Runs!.Last();
        run.EndedAt = DateTimeOffset.Now;
        run.Outcome = task.Status;
        if (this.Options.CloudEvents.PublishLifecycleEvents)
        {
            await this.PublishAsync(new CloudEvent()
            {
                SpecVersion = CloudEventSpecVersion.V1.Version,
                Id = Guid.NewGuid().ToString(),
                Time = DateTimeOffset.Now,
                Source = this.Options.Api.BaseAddress,
                Type = SynapseDefaults.CloudEvents.Task.Completed.v1,
                Subject = this.Instance.GetQualifiedName(),
                DataContentType = MediaTypeNames.Application.Json,
                Data = new TaskCompletedEventV1()
                {
                    Workflow = this.Instance.GetQualifiedName(),
                    Task = task.Reference,
                    CompletedAt = run?.EndedAt ?? DateTimeOffset.Now
                }
            }, cancellationToken).ConfigureAwait(false);
            await this.PublishAsync(new CloudEvent()
            {
                SpecVersion = CloudEventSpecVersion.V1.Version,
                Id = Guid.NewGuid().ToString(),
                Time = DateTimeOffset.Now,
                Source = this.Options.Api.BaseAddress,
                Type = SynapseDefaults.CloudEvents.Task.Ended.v1,
                Subject = this.Instance.GetQualifiedName(),
                DataContentType = MediaTypeNames.Application.Json,
                Data = new TaskEndedEventV1()
                {
                    Workflow = this.Instance.GetQualifiedName(),
                    Task = task.Reference,
                    Status = this.Instance.Status!.Phase!,
                    EndedAt = run?.EndedAt ?? DateTimeOffset.Now
                }
            }, cancellationToken).ConfigureAwait(false);
        }
        var output = this.Options.Workflow.OutputFormat switch
        {
            WorkflowOutputFormat.Json => System.Text.Json.JsonSerializer.Serialize(result, this._jsonSerializerOptions),
            WorkflowOutputFormat.Yaml => this.YamlSerializer.SerializeToText(result),
            _ => throw new NotSupportedException($"The specified workflow output format '{this.Options.Workflow.OutputFormat}' is not supported"),
        };
        this.Logger.LogInformation("Task '{reference}' successfully executed.", task.Reference);
        return this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
    }

    /// <inheritdoc/>
    public virtual async Task<TaskInstance> SkipAsync(TaskInstance task, object? result, string? then = FlowDirective.Continue, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        using var @lock = await this.Lock.LockAsync(cancellationToken).ConfigureAwait(false);
        var document = await this.Documents.CreateAsync($"{this.Instance.GetQualifiedName()}/{task.Reference}/output", result ?? new(), cancellationToken).ConfigureAwait(false);
        result ??= new();
        task = this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
        task.Status = TaskInstanceStatus.Skipped;
        task.EndedAt = DateTimeOffset.Now;
        task.OutputReference = document.Id;
        task.Next = then;
        if (this.Options.CloudEvents.PublishLifecycleEvents) await this.PublishAsync(new CloudEvent()
        {
            SpecVersion = CloudEventSpecVersion.V1.Version,
            Id = Guid.NewGuid().ToString(),
            Time = DateTimeOffset.Now,
            Source = this.Options.Api.BaseAddress,
            Type = SynapseDefaults.CloudEvents.Task.Skipped.v1,
            Subject = this.Instance.GetQualifiedName(),
            DataContentType = MediaTypeNames.Application.Json,
            Data = new TaskSkippedEventV1()
            {
                Workflow = this.Instance.GetQualifiedName(),
                Task = task.Reference,
                SkippedAt = task.EndedAt ?? DateTimeOffset.Now
            }
        }, cancellationToken).ConfigureAwait(false);
        this.Logger.LogInformation("The execution of task '{reference}' has been skipped.", task.Reference);
        return this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
    }

    /// <inheritdoc/>
    public virtual async Task SuspendAsync(CancellationToken cancellationToken = default)
    {
        if (this.Instance.Status?.Phase == WorkflowInstanceStatusPhase.Suspended) return;
        using var @lock = await this.Lock.LockAsync(cancellationToken).ConfigureAwait(false);
        this.Instance.Status ??= new();
        this.Instance.Status.Phase = WorkflowInstanceStatusPhase.Suspended;
        var run = this.Instance.Status.Runs?.LastOrDefault();
        if (run != null) run.EndedAt = DateTimeOffset.Now;
        if (this.Options.CloudEvents.PublishLifecycleEvents) await this.PublishAsync(new CloudEvent()
        {
            SpecVersion = CloudEventSpecVersion.V1.Version,
            Id = Guid.NewGuid().ToString(),
            Time = DateTimeOffset.Now,
            Source = this.Options.Api.BaseAddress,
            Type = SynapseDefaults.CloudEvents.Workflow.Suspended.v1,
            Subject = this.Instance.GetQualifiedName(),
            DataContentType = MediaTypeNames.Application.Json,
            Data = new WorkflowSuspendedEventV1()
            {
                Name = this.Instance.GetQualifiedName(),
                SuspendedAt = run?.EndedAt ?? DateTimeOffset.Now
            }
        }, cancellationToken).ConfigureAwait(false);
        this.Logger.LogInformation("The workflow's execution has been suspended.");
    }

    /// <inheritdoc/>
    public virtual async Task<TaskInstance> SuspendAsync(TaskInstance task, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        using var @lock = await this.Lock.LockAsync(cancellationToken).ConfigureAwait(false);
        task = this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
        task.Status = TaskInstanceStatus.Suspended;
        var run = task.Runs!.Last();
        run.EndedAt = DateTimeOffset.Now;
        run.Outcome = task.Status;
        if (this.Options.CloudEvents.PublishLifecycleEvents) await this.PublishAsync(new CloudEvent()
        {
            SpecVersion = CloudEventSpecVersion.V1.Version,
            Id = Guid.NewGuid().ToString(),
            Time = DateTimeOffset.Now,
            Source = this.Options.Api.BaseAddress,
            Type = SynapseDefaults.CloudEvents.Task.Suspended.v1,
            Subject = this.Instance.GetQualifiedName(),
            DataContentType = MediaTypeNames.Application.Json,
            Data = new TaskSuspendedEventV1()
            {
                Workflow = this.Instance.GetQualifiedName(),
                Task = task.Reference,
                SuspendedAt = run?.EndedAt ?? DateTimeOffset.Now
            }
        }, cancellationToken).ConfigureAwait(false);
        this.Logger.LogInformation("The execution of task '{reference}' has been suspended.", task.Reference);
        return this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
    }

    /// <inheritdoc/>
    public virtual async Task CancelAsync(CancellationToken cancellationToken = default)
    {
        if (this.Instance.Status?.Phase == WorkflowInstanceStatusPhase.Cancelled) return;
        using var @lock = await this.Lock.LockAsync(cancellationToken).ConfigureAwait(false);
        this.Instance.Status ??= new();
        this.Instance.Status.Phase = WorkflowInstanceStatusPhase.Cancelled;
        this.Instance.Status.EndedAt = DateTimeOffset.Now;
        var run = this.Instance.Status.Runs?.LastOrDefault();
        if (run != null) run.EndedAt = DateTimeOffset.Now;
        if (this.Options.CloudEvents.PublishLifecycleEvents)
        {
            await this.PublishAsync(new CloudEvent()
            {
                SpecVersion = CloudEventSpecVersion.V1.Version,
                Id = Guid.NewGuid().ToString(),
                Time = DateTimeOffset.Now,
                Source = this.Options.Api.BaseAddress,
                Type = SynapseDefaults.CloudEvents.Workflow.Cancelled.v1,
                Subject = this.Instance.GetQualifiedName(),
                DataContentType = MediaTypeNames.Application.Json,
                Data = new WorkflowCancelledEventV1()
                {
                    Name = this.Instance.GetQualifiedName(),
                    CancelledAt = run?.EndedAt ?? DateTimeOffset.Now
                }
            }, cancellationToken).ConfigureAwait(false);
            await this.PublishAsync(new CloudEvent()
            {
                SpecVersion = CloudEventSpecVersion.V1.Version,
                Id = Guid.NewGuid().ToString(),
                Time = DateTimeOffset.Now,
                Source = this.Options.Api.BaseAddress,
                Type = SynapseDefaults.CloudEvents.Workflow.Ended.v1,
                Subject = this.Instance.GetQualifiedName(),
                DataContentType = MediaTypeNames.Application.Json,
                Data = new WorkflowEndedEventV1()
                {
                    Name = this.Instance.GetQualifiedName(),
                    Status = this.Instance.Status!.Phase!,
                    EndedAt = run?.EndedAt ?? DateTimeOffset.Now
                }
            }, cancellationToken).ConfigureAwait(false);
        }
        this.Logger.LogInformation("The workflow's execution cancelled.");
    }

    /// <inheritdoc/>
    public virtual async Task<TaskInstance> CancelAsync(TaskInstance task, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        using var @lock = await this.Lock.LockAsync(cancellationToken).ConfigureAwait(false);
        task = this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
        task.Status = TaskInstanceStatus.Cancelled;
        task.EndedAt = DateTimeOffset.Now;
        var run = task.Runs!.Last();
        run.EndedAt = DateTimeOffset.Now;
        run.Outcome = task.Status;
        if (this.Options.CloudEvents.PublishLifecycleEvents)
        {
            await this.PublishAsync(new CloudEvent()
            {
                SpecVersion = CloudEventSpecVersion.V1.Version,
                Id = Guid.NewGuid().ToString(),
                Time = DateTimeOffset.Now,
                Source = this.Options.Api.BaseAddress,
                Type = SynapseDefaults.CloudEvents.Task.Cancelled.v1,
                Subject = this.Instance.GetQualifiedName(),
                DataContentType = MediaTypeNames.Application.Json,
                Data = new TaskCancelledEventV1()
                {
                    Workflow = this.Instance.GetQualifiedName(),
                    Task = task.Reference,
                    CancelledAt = run?.EndedAt ?? DateTimeOffset.Now
                }
            }, cancellationToken).ConfigureAwait(false);
            await this.PublishAsync(new CloudEvent()
            {
                SpecVersion = CloudEventSpecVersion.V1.Version,
                Id = Guid.NewGuid().ToString(),
                Time = DateTimeOffset.Now,
                Source = this.Options.Api.BaseAddress,
                Type = SynapseDefaults.CloudEvents.Task.Ended.v1,
                Subject = this.Instance.GetQualifiedName(),
                DataContentType = MediaTypeNames.Application.Json,
                Data = new TaskEndedEventV1()
                {
                    Workflow = this.Instance.GetQualifiedName(),
                    Task = task.Reference,
                    Status = this.Instance.Status!.Phase!,
                    EndedAt = run?.EndedAt ?? DateTimeOffset.Now
                }
            }, cancellationToken).ConfigureAwait(false);
        }
        this.Logger.LogInformation("The execution of task '{reference}' has been cancelled.", task.Reference);
        return this.Instance.Status?.Tasks?.FirstOrDefault(t => t.Id == task.Id) ?? throw new NullReferenceException($"Failed to find the task instance with the specified id '{task.Id}'. Make sure the task instance resource has been created using the workflow context.");
    }

}
