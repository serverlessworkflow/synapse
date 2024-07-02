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

using Synapse.Cli.Configuration;

namespace Synapse.Cli.Services;

/// <summary>
/// Defines the service used to manage the application's options
/// </summary>
public interface IOptionsManager
{

    /// <summary>
    /// Updates the application's options
    /// </summary>
    /// <param name="options">The updated <see cref="ApplicationOptions"/> to persist</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task UpdateOptionsAsync(ApplicationOptions options, CancellationToken cancellationToken = default);

}

/// <summary>
/// Represents the default implementation of the <see cref="IOptionsManager"/> interface
/// </summary>
/// <param name="serializer">The service used to serialize/deserialize data to/from YAML</param>
public class OptionsManager(IYamlSerializer serializer)
    : IOptionsManager
{

    /// <summary>
    /// Gets the service used to serialize/deserialize data to/from YAML
    /// </summary>
    protected IYamlSerializer Serializer { get; } = serializer;

    /// <inheritdoc/>
    public virtual async Task UpdateOptionsAsync(ApplicationOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var yaml = this.Serializer.SerializeToText(options);
        await File.WriteAllTextAsync(CliConstants.ConfigurationFileName, yaml, cancellationToken);
    }
}
