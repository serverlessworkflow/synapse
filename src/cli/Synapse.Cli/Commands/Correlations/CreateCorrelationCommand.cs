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

using Neuroglia.Data.Infrastructure.ResourceOriented;

namespace Synapse.Cli.Commands.Correlations;

/// <summary>
/// Represents the <see cref="Command"/> used to create a new <see cref="Correlation"/>
/// </summary>
internal class CreateCorrelationCommand
    : Command
{

    /// <summary>
    /// Gets the <see cref="CreateCorrelationCommand"/>'s name
    /// </summary>
    public const string CommandName = "create";
    /// <summary>
    /// Gets the <see cref="CreateCorrelationCommand"/>'s description
    /// </summary>
    public const string CommandDescription = "Creates a new correlation.";

    /// <inheritdoc/>
    public CreateCorrelationCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api, IYamlSerializer yamlSerializer)
        : base(serviceProvider, loggerFactory, api, CommandName, CommandDescription)
    {
        this.YamlSerializer = yamlSerializer;
        this.Add(CommandOptions.File);
        this.Handler = CommandHandler.Create<string>(this.HandleAsync);
    }

    /// <summary>
    /// Gets the service used to serialize/deserialize objects to/from YAML
    /// </summary>
    protected IYamlSerializer YamlSerializer { get; }

    /// <summary>
    /// Handles the <see cref="CreateCorrelationCommand"/>
    /// </summary>
    /// <param name="file">The file that defines the correlation to create</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task HandleAsync(string file)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(file);
        var yaml = await File.ReadAllTextAsync(file);
        var correlation = this.YamlSerializer.Deserialize<Correlation>(yaml) ?? throw new NullReferenceException("Failed to read a correlation resource from the specified file.");
        correlation = await this.Api.Correlations.CreateAsync(correlation);
        Console.WriteLine($"correlation/{correlation.GetName()} created");
    }

    static class CommandOptions
    {

        public static Option<string> File
        {
            get
            {
                var option = new Option<string>("--file")
                {
                    Description = "The file that contains the definition of the correlation to create."
                };
                option.AddAlias("-f");
                return option;
            }
        }

    }

}
