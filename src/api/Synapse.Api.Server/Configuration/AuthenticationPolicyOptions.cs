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

using Neuroglia.Serialization.Yaml;

namespace Synapse.Api.Server.Configuration;

/// <summary>
/// Represents the options used to configure the authentication policy of a Synapse API server
/// </summary>
public class AuthenticationPolicyOptions
{

    /// <summary>
    /// Initializes a new <see cref="AuthenticationPolicyOptions"/>
    /// </summary>
    public AuthenticationPolicyOptions()
    {
        var env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Api.Authentication.File);
        if (!string.IsNullOrWhiteSpace(env))
        {
            if (!File.Exists(env)) throw new FileNotFoundException($"The specified file '{env}' does not exist or cannot be found", env);
            var yaml = File.ReadAllText(env);
            this.Tokens = YamlSerializer.Default.Deserialize<Dictionary<string, IDictionary<string, string>>>(yaml) ?? [];
        }
        env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Api.Authentication.Jwt.Authority);
        if(!string.IsNullOrWhiteSpace(env))
        {
            this.Jwt ??= new();
            this.Jwt.Authority = new(env);
        }
        env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Api.Authentication.Jwt.Audience);
        if (!string.IsNullOrWhiteSpace(env))
        {
            this.Jwt ??= new();
            this.Jwt.Audience = env;
        }

        env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Api.Authentication.Oidc.Authority);
        if (!string.IsNullOrWhiteSpace(env))
        {
            this.Oidc ??= new();
            this.Oidc.Authority = new(env);
        }
        env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Api.Authentication.Oidc.ClientId);
        if (!string.IsNullOrWhiteSpace(env))
        {
            this.Oidc ??= new();
            this.Oidc.ClientId = env;
        }
        env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Api.Authentication.Oidc.ClientSecret);
        if (!string.IsNullOrWhiteSpace(env))
        {
            this.Oidc ??= new();
            this.Oidc.ClientSecret = env;
        }
        env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Api.Authentication.Oidc.Scope);
        if (!string.IsNullOrWhiteSpace(env))
        {
            this.Oidc ??= new();
            if(!string.IsNullOrWhiteSpace(env)) this.Oidc.Scope = [.. env.Split(',', StringSplitOptions.RemoveEmptyEntries)];
        }
        env = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Api.Authentication.Oidc.SigningKey);
        if (!string.IsNullOrWhiteSpace(env))
        {
            this.Oidc ??= new();
            this.Oidc.SigningKey = env;
        }
    }

    /// <summary>
    /// Gets/sets a token/claims mappings of the static tokens to use
    /// </summary>
    public virtual Dictionary<string, IDictionary<string, string>> Tokens { get; set; } = [];

    /// <summary>
    /// Gets/sets the options used to configure the Jwt Bearer authentication to use, if any
    /// </summary>
    public virtual JwtBearerAuthenticationOptions? Jwt { get; set; }

    /// <summary>
    /// Gets/sets the options used to configure the Jwt Bearer authentication to use, if any
    /// </summary>
    public virtual OidcAuthenticationOptions? Oidc { get; set; }

}
