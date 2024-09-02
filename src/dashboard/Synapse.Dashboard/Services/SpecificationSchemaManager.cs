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

using System.Net.Http.Json;

namespace Synapse.Dashboard.Services;

/// <summary>
/// The service used to download the specification schemas
/// </summary>
/// <param name="yamlSerializer">The service used to serialize/deserialize data to/from YAML</param>
/// <param name="httpClient">The service used to send HTTP requests and receive HTTP responses from a resource identified by a URI.</param>
public class SpecificationSchemaManager(IYamlSerializer yamlSerializer, HttpClient httpClient)
{

    Dictionary<string, string> _knownSchemas = new Dictionary<string, string>();

    string? _latestVersion = null;

    /// <summary>
    /// Gets the service used to serialize/deserialize data to/from YAML
    /// </summary>
    IYamlSerializer YamlSerializer { get; } = yamlSerializer;

    /// <summary>
    /// Gets the service used to send HTTP requests and receive HTTP responses from a resource identified by a URI.
    /// </summary>
    HttpClient HttpClient { get; } = httpClient;

    /// <summary>
    /// Gets the latest version of the specification
    /// </summary>
    /// <returns>A awaitable task</returns>
    public async Task<string> GetLatestVersion()
    {
        if (!string.IsNullOrEmpty(this._latestVersion)) return this._latestVersion;
        var tags = await this.HttpClient.GetFromJsonAsync<IEnumerable<GitHubTag>>("https://api.github.com/repos/serverlessworkflow/specification/tags");
        this._latestVersion = tags?.FirstOrDefault()?.Name;
        return this._latestVersion ?? string.Empty;
    }

    /// <summary>
    /// Gets the specification's JSON schema for the specificed version
    /// </summary>
    /// <param name="version">The version to get the schema for</param>
    /// <returns>A awaitable task</returns>
    public async Task<string> GetSchema(string version)
    {
        if (this._knownSchemas.ContainsKey(version)) return this._knownSchemas[version];
        var address = $"https://raw.githubusercontent.com/serverlessworkflow/specification/{version}/schema/workflow.yaml";
        var yamlSchema = await this.HttpClient.GetStringAsync(address);
        this._knownSchemas.Add(version, this.YamlSerializer.ConvertToJson(yamlSchema));
        return this._knownSchemas[version];
    }
}


/// <summary>
/// Represents a GitHub tag
/// </summary>
public class GitHubTag
{
    /// <summary>
    /// Gets/sets the tag name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Gets/sets the zip url
    /// </summary>
    public string ZipballUrl { get; set; } = string.Empty;
    /// <summary>
    /// Gets/sets the tar url
    /// </summary>
    public string TarballUrl { get; set; } = string.Empty;
    /// <summary>
    /// Gets/sets the commit info
    /// </summary>
    public object Commit { get; set; } = string.Empty;
    /// <summary>
    /// Gets/sets the node id
    /// </summary>
    public string NodeId { get; set; } = string.Empty;
}