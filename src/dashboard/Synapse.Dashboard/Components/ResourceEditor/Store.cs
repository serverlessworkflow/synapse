﻿// Copyright © 2024-Present The Synapse Authors
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

using JsonCons.Utilities;
using Neuroglia.Data;
using Neuroglia.Serialization.Yaml;
using Synapse.Api.Client.Services;
using Synapse.Dashboard.Components.DocumentDetailsStateManagement;

namespace Synapse.Dashboard.Components.ResourceEditorStateManagement;

/// <summary>
/// Represents a <see cref="ResourceEditor{TResource}"/>'s form <see cref="ComponentStore{TState}"/>
/// </summary>
/// <remarks>
/// Initializes a new <see cref="ResourceEditorStore{TResource}"/>
/// </remarks>
/// <param name="logger">The service used to perform logging</param>
/// <param name="apiClient">The service used to interact with a Synapse API</param>
/// <param name="monacoEditorHelper">The service used to facilitate the Monaco editor interactions</param>
/// <param name="jsonSerializer">The The service used to serialize/deserialize objects to/from JSON</param>
/// <param name="yamlSerializer">The service used to serialize/deserialize objects to/from YAML</param>
public class ResourceEditorStore<TResource>(ILogger<ResourceEditorStore<TResource>> logger, ISynapseApiClient apiClient, IMonacoEditorHelper monacoEditorHelper, IJsonSerializer jsonSerializer, IYamlSerializer yamlSerializer)
    : ComponentStore<ResourceEditorState<TResource>>(new())
    where TResource : Resource, new()
{

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger<ResourceEditorStore<TResource>> Logger { get; } = logger;

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="ResourceEditorState{TResource}.Resource"/> changes
    /// </summary>
    public IObservable<TResource?> Resource => this.Select(state => state.Resource).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="ResourceEditorState{TResource}.Definition"/> changes
    /// </summary>
    public IObservable<ResourceDefinition?> Definition => this.Select(state => state.Definition).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="ResourceEditorState{TResource}.TextEditorValue"/> changes
    /// </summary>
    public IObservable<string> TextEditorValue => this.Select(state => state.TextEditorValue).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="ResourceEditorState{TResource}.IsCluster"/> changes
    /// </summary>
    public IObservable<bool> IsCluster => this.Select(state => state.IsCluster).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="ResourceEditorState{TResource}.IsUpdating"/> changes
    /// </summary>
    public IObservable<bool> IsUpdating => this.Select(state => state.IsUpdating).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="ResourceEditorState{TResource}.IsSaving"/> changes
    /// </summary>
    public IObservable<bool> IsSaving => this.Select(state => state.IsSaving).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="ResourceEditorState{TResource}.ProblemType"/> changes
    /// </summary>
    public IObservable<Uri?> ProblemType => this.Select(state => state.ProblemType).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="ResourceEditorState{TResource}.ProblemTitle"/> changes
    /// </summary>
    public IObservable<string> ProblemTitle => this.Select(state => state.ProblemTitle).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="ResourceEditorState{TResource}.ProblemDetail"/> changes
    /// </summary>
    public IObservable<string> ProblemDetail => this.Select(state => state.ProblemDetail).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="ResourceEditorState{TResource}.ProblemStatus"/> changes
    /// </summary>
    public IObservable<int> ProblemStatus => this.Select(state => state.ProblemStatus).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="ResourceEditorState{TResource}.ProblemErrors"/> changes
    /// </summary>
    public IObservable<IDictionary<string, string[]>> ProblemErrors => this.Select(state => state.ProblemErrors).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe computed <see cref="Neuroglia.ProblemDetails"/>
    /// </summary>
    public IObservable<ProblemDetails?> ProblemDetails => Observable.CombineLatest(
        this.ProblemType,
        this.ProblemTitle,
        this.ProblemStatus,
        this.ProblemDetail,
        this.ProblemErrors,
        (type, title, status, details, errors) =>
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }
            return new ProblemDetails(type ?? new Uri("unknown://"), title, status, details, null, errors, null);
        }
    );

    /// <inheritdoc/>
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync().ConfigureAwait(false);
        this.Resource.Subscribe(resource =>
        {
            if (monacoEditorHelper.PreferredLanguage == PreferredLanguage.YAML) this.SetEditorValue(yamlSerializer.ConvertFromJson(jsonSerializer.SerializeToText(resource)));
            else this.SetEditorValue(jsonSerializer.SerializeToText(resource));
        }, token: this.CancellationTokenSource.Token);
    }

    /// <summary>
    /// Sets the state's <see cref="ResourceEditorState{TResource}.Resource"/>
    /// </summary>
    /// <param name="resource">The new <see cref="ResourceEditorState{TResource}.Resource"/> value</param>
    public void SetResource(TResource? resource)
    {
        if (resource != null)
        {
            this.Reduce(state => state with
            {
                Resource = resource
            });
            return;
        }
        this.Reduce(state => state with { 
            Resource = new() { Metadata = new() { Name = "new-" + typeof(TResource).Name.ToLower() } }
        });
    }

    /// <summary>
    /// Sets the state's <see cref="ResourceEditorState{TResource}.Definition"/>
    /// </summary>
    /// <param name="definition">The new <see cref="ResourceEditorState{TResource}.Definition"/> value</param>
    public void SetDefinition(ResourceDefinition? definition)
    {
        this.Reduce(state => state with
        {
            Definition = definition
        });
    }

    /// <summary>
    /// Sets the state's <see cref="ResourceEditorState{TResource}.TextEditorValue"/>
    /// </summary>
    /// <param name="textEditorValue">The new <see cref="ResourceEditorState{TResource}.TextEditorValue"/> value</param>
    public void SetEditorValue(string textEditorValue)
    {
        this.SetUpdating(true);
        this.Reduce(state => state with
        {
            TextEditorValue = textEditorValue
        });
        this.SetUpdating(false);
    }

    /// <summary>
    /// Sets the state's <see cref="ResourceEditorState{TResource}.IsUpdating"/>
    /// </summary>
    /// <param name="isUpdating">The new <see cref="ResourceEditorState{TResource}.IsUpdating"/> value</param>
    public void SetUpdating(bool isUpdating)
    {
        this.Reduce(state => state with
        {
            IsUpdating = isUpdating
        });
    }

    /// <summary>
    /// Sets the state's <see cref="ResourceEditorState{TResource}.IsCluster"/>
    /// </summary>
    /// <param name="isCluster">The new <see cref="ResourceEditorState{TResource}.IsCluster"/> value</param>
    public void SetIsCluster(bool isCluster)
    {
        this.Reduce(state => state with
        {
            IsCluster = isCluster
        });
    }

    /// <summary>
    /// Sets the state's <see cref="ResourceEditorState{TResource}.IsSaving"/>
    /// </summary>
    /// <param name="isSaving">The new <see cref="ResourceEditorState{TResource}.IsSaving"/> value</param>
    public void SetSaving(bool isSaving)
    {
        this.Reduce(state => state with
        {
            IsSaving = isSaving
        });
    }

    /// <summary>
    /// Sets the state's <see cref="ResourceEditorState{TResource}" /> <see cref="ProblemDetails"/>'s related data
    /// </summary>
    /// <param name="problem">The <see cref="ProblemDetails"/> to populate the data with</param>
    public void SetProblemDetails(ProblemDetails? problem)
    {
        this.Reduce(state => state with
        {
            ProblemType = problem?.Type,
            ProblemTitle = problem?.Title ?? string.Empty,
            ProblemStatus = problem?.Status ?? 0,
            ProblemDetail = problem?.Detail ?? string.Empty,
            ProblemErrors = problem?.Errors?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? []
        });
    }

    /// <summary>
    /// Changes the editor language
    /// </summary>
    /// <param name="language">The new editor's language</param>
    /// <returns></returns>
    public async Task ChangeTextEditorLanguageAsync(string language)
    {
        var textEditorValue = this.Get(state => state.TextEditorValue);
        try
        {
            var text = language == PreferredLanguage.YAML ?
                yamlSerializer.ConvertFromJson(textEditorValue) :
                yamlSerializer.ConvertToJson(textEditorValue);
            this.SetEditorValue(text);
        }
        catch (Exception ex)
        {
            this.Logger.LogError("Unable to change text editor language: {exception}", ex.ToString());
            await monacoEditorHelper.ChangePreferredLanguageAsync(language == PreferredLanguage.YAML ? PreferredLanguage.JSON : PreferredLanguage.YAML);
        }
    }

    /// <summary>
    /// Creates or updates the current resource
    /// </summary>
    /// <returns></returns>
    public async Task SubmitResourceAsync()
    {
        TResource? resource = this.Get(state => state.Resource);
        if (resource?.Metadata?.Generation == null || resource?.Metadata?.Generation == 0)
        {
            await this.CreateResourceAsync();
        }
        else
        {
            await this.UpdateResourceAsync();
        }
    }

    /// <summary>
    /// Creates the current resource using the text editor value
    /// </summary>
    /// <returns></returns>
    public async Task CreateResourceAsync()
    {
        this.SetProblemDetails(null);
        this.SetSaving(true);
        var textEditorValue = this.Get(state => state.TextEditorValue);
        if (monacoEditorHelper.PreferredLanguage == PreferredLanguage.YAML) textEditorValue = yamlSerializer.ConvertToJson(textEditorValue);
        TResource? resource;
        try
        {
            resource = jsonSerializer.Deserialize<TResource>(textEditorValue);
            var isCluster = this.Get(state => state.IsCluster);
            resource = await (!isCluster ? 
                 apiClient.ManageNamespaced<TResource>().CreateAsync(resource!, this.CancellationTokenSource.Token) :
                 apiClient.ManageCluster<TResource>().CreateAsync(resource!, this.CancellationTokenSource.Token)
            );
            this.SetResource(resource);
        }
        catch (ProblemDetailsException ex)
        {
            this.SetProblemDetails(ex.Problem);
        }
        catch (Exception ex)
        {
            this.Logger.LogError("Unable to create resource: {exception}", ex.ToString());
        }
        this.SetSaving(false);
    }

    /// <summary>
    /// Updates the current resource using the text editor value
    /// </summary>
    /// <returns></returns>
    public async Task UpdateResourceAsync()
    {
        this.SetProblemDetails(null);
        this.SetSaving(true);
        var resource = this.Get(state => state.Resource);
        if (resource == null) return;
        var textEditorValue = this.Get(state => state.TextEditorValue);
        if (monacoEditorHelper.PreferredLanguage == PreferredLanguage.YAML) textEditorValue = yamlSerializer.ConvertToJson(textEditorValue);
        var jsonPatch = JsonPatch.FromDiff(jsonSerializer.SerializeToElement(resource)!.Value, jsonSerializer.SerializeToElement(jsonSerializer.Deserialize<TResource>(textEditorValue))!.Value);
        var patch = jsonSerializer.Deserialize<Json.Patch.JsonPatch>(jsonPatch.RootElement);
        if (patch != null)
        {
            var resourcePatch = new Patch(PatchType.JsonPatch, jsonPatch);
            try
            {
                var isCluster = this.Get(state => state.IsCluster);
                resource = await (!isCluster ? 
                     apiClient.ManageNamespaced<TResource>().PatchAsync(resource.GetName(), resource.GetNamespace()!, resourcePatch, null, this.CancellationTokenSource.Token) :
                     apiClient.ManageCluster<TResource>().PatchAsync(resource.GetName(), resourcePatch, null, this.CancellationTokenSource.Token)
                );
                this.SetResource(resource);
            }
            catch(ProblemDetailsException ex)
            {
                this.SetProblemDetails(ex.Problem);
            }
            catch (Exception ex)
            {
                this.Logger.LogError("Unable to update resource: {exception}", ex.ToString());
            }
        }
        this.SetSaving(false);
    }

}
