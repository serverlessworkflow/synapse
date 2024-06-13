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

namespace Synapse.Cli.Commands.Operators;

/// <summary>
/// Represents the <see cref="Command"/> used to get a specific <see cref="Operator"/>
/// </summary>
internal class GetOperatorCommand
    : Command
{

    /// <summary>
    /// Gets the <see cref="GetOperatorCommand"/>'s name
    /// </summary>
    public const string CommandName = "get";
    /// <summary>
    /// Gets the <see cref="GetOperatorCommand"/>'s description
    /// </summary>
    public const string CommandDescription = "Get the specified operator";

    /// <inheritdoc/>
    public GetOperatorCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api, IJsonSerializer jsonSerializer, IYamlSerializer yamlSerializer)
        : base(serviceProvider, loggerFactory, api, CommandName, CommandDescription)
    {
        this.JsonSerializer = jsonSerializer;
        this.YamlSerializer = yamlSerializer;
        this.AddAlias("get");
        this.Add(new Argument<string>("name") { Description = "The name of the operator to get" });
        this.Add(CommandOptions.Namespace);
        this.Add(CommandOptions.Output);
        this.Handler = CommandHandler.Create<string, string, string>(this.HandleAsync);
    }

    /// <summary>
    /// Gets the service used to serialize/deserialize to/from JSON
    /// </summary>
    protected IJsonSerializer JsonSerializer { get; }

    /// <summary>
    /// Gets the service used to serialize/deserialize to/from YAML
    /// </summary>
    protected IYamlSerializer YamlSerializer { get; }

    /// <summary>
    /// Handles the <see cref="GetOperatorCommand"/>
    /// </summary>
    /// <param name="namespace">The namespace of the operator to get</param>
    /// <param name="name">The name of the operator to get</param>
    /// <param name="output">The desired output format</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task HandleAsync(string name, string @namespace, string output)
    {
        var @operator = await this.Api.Operators.GetAsync(name, @namespace);
        string outputText = output.ToLowerInvariant() switch
        {
            "json" => this.JsonSerializer.SerializeToText(@operator),
            "yaml" => this.YamlSerializer.SerializeToText(@operator),
            _ => throw new NotSupportedException($"The specified output format '{output}' is not supported"),
        };
        AnsiConsole.Markup($"[gray]{outputText.EscapeMarkup()}[/]");
    }

    static class CommandOptions
    {

        public static Option<string> Namespace => new(["-n", "--namespace"], () => "default", "The namespace the operator to get belongs to.");

        public static Option<string> Output => new(["-o", "--output"], () => "yaml", "The output format.");

    }

}
