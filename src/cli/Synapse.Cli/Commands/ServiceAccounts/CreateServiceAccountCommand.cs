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
using Synapse.Resources;
using System.Security.Cryptography;

namespace Synapse.Cli.Commands.ServiceAccounts;

/// <summary>
/// Represents the <see cref="Command"/> used to create a new <see cref="ServiceAccount"/>
/// </summary>
internal class CreateServiceAccountCommand
    : Command
{

    /// <summary>
    /// Gets the <see cref="CreateServiceAccountCommand"/>'s name
    /// </summary>
    public const string CommandName = "create";
    /// <summary>
    /// Gets the <see cref="CreateServiceAccountCommand"/>'s description
    /// </summary>
    public const string CommandDescription = "Creates a new service account.";

    /// <inheritdoc/>
    public CreateServiceAccountCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api, IJsonSerializer jsonSerializer)
        : base(serviceProvider, loggerFactory, api, CommandName, CommandDescription)
    {
        this.JsonSerializer = jsonSerializer;
        this.Add(new Argument<string>("name") { Description = "The name of the service account to create." });
        this.Add(CommandOptions.Namespace);
        this.Add(CommandOptions.Key);
        this.Add(CommandOptions.Claims);
        this.Handler = CommandHandler.Create<string, string, string, string>(this.HandleAsync);
    }

    /// <summary>
    /// Gets the service used to serialize/deserialize objects to/from JSON
    /// </summary>
    protected IJsonSerializer JsonSerializer { get; }

    /// <summary>
    /// Handles the <see cref="CreateServiceAccountCommand"/>
    /// </summary>
    /// <param name="name">The name of the service account to create</param>
    /// <param name="namespace">The namespace the service account to create belongs to</param>
    /// <param name="key"></param>
    /// <param name="claims">The JSON of a type/value mapping of the claims associated to the service account to create</param>
    /// <returns>A new awaitable <see cref="Task"/>The JSON of the claims associated to the service account to create</returns>
    public async Task HandleAsync(string name, string @namespace, string? key, string claims)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(@namespace);
        ArgumentException.ThrowIfNullOrWhiteSpace(claims);
        if (string.IsNullOrWhiteSpace(key)) 
        {
            var keyBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(keyBytes);
            key = Convert.ToBase64String(keyBytes);
        }
        var claimMap = this.JsonSerializer.Deserialize<IDictionary<string, string>>(claims);
        var serviceAccount = new ServiceAccount()
        {
            Metadata = new()
            {
                Namespace = @namespace,
                Name = name
            },
            Spec = new()
            {
                Key = key,
                Claims = claimMap ?? new Dictionary<string, string>()
            }
        };
        serviceAccount = await this.Api.ServiceAccounts.CreateAsync(serviceAccount);
        Console.WriteLine($"service-account/{serviceAccount.GetName()} created");
    }

    static class CommandOptions
    {

        public static Option<string> Namespace => new(["-n", "--namespace"], "The namespace the service account to create belongs to.");

        public static Option<string> Key => new(["-k", "--key"], () => string.Empty, "The symmetric key used by the related service for authentication purpose. If not set, one will be automatically generated.");

        public static Option<string> Claims => new(["-c", "--claims"], "The JSON of the claims associated to the service account to create");

    }

}