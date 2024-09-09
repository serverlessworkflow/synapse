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

using MimeKit;

namespace Synapse.Runner.Services;

/// <summary>
/// Represents a file-based implementation of the <see cref="ISecretsManager"/> interface
/// </summary>
/// <param name="logger">The service used to perform logging</param>
/// <param name="serializerProvider">The service used to provide <see cref="ISerializer"/>s</param>
/// <param name="options">The service used to access the current <see cref="RunnerOptions"/></param>
public class SecretsManager(ILogger<SecretsManager> logger, ISerializerProvider serializerProvider, IOptions<RunnerOptions> options)
    : BackgroundService, ISecretsManager
{

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger Logger { get; } = logger;

    /// <summary>
    /// Gets the service used to provide <see cref="ISerializer"/>s
    /// </summary>
    protected ISerializerProvider SerializerProvider { get; } = serializerProvider;

    /// <summary>
    /// Gets the current <see cref="RunnerOptions"/>
    /// </summary>
    protected RunnerOptions Options { get; } = options.Value;

    /// <summary>
    /// Gets an name/value mapping of all loaded secrets
    /// </summary>
    protected IDictionary<string, object> Secrets { get; } = new Dictionary<string, object>();

    /// <inheritdoc/>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var path = string.IsNullOrWhiteSpace(this.Options.Secrets.Directory)
            ? RunnerSecretsOptions.DefaultDirectory
            : this.Options.Secrets.Directory;
        var directory = new DirectoryInfo(path);
        if (!directory.Exists) directory.Create();
        foreach (var file in directory.GetFiles())
        {
            using var stream = file.OpenRead();
            var mediaTypeName = MimeTypes.GetMimeType(file.Name);
            var serializer = this.SerializerProvider.GetSerializersFor(mediaTypeName).FirstOrDefault();
            if (serializer == null)
            {
                this.Logger.LogWarning("Skipped loading secret '{secretFile}': failed to find a serializer for the specified media type '{mediaType}'", file.Name, mediaTypeName);
                continue;
            }
            try
            {
                var secret = serializer.Deserialize<object>(stream)!;
                this.Secrets.Add(file.Name, secret);
            }
            catch (Exception ex)
            {
                this.Logger.LogWarning("Skipped loading secret '{secretFile}': an exception occurred while deserializing the secret object: {ex}", file.Name, ex.ToString());
                continue;
            }
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual Task<IDictionary<string, object>> GetSecretsAsync(CancellationToken cancellationToken) => Task.FromResult(this.Secrets);

}