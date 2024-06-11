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

using ServerlessWorkflow.Sdk.Models.Authentication;

namespace Synapse.Core.Infrastructure.Services;

/// <summary>
/// Defines the fundamentals of a service used to manage <see cref="OAuth2Token"/>s
/// </summary>
public interface IOAuth2TokenManager
{

    /// <summary>
    /// Gets an <see cref="OAuth2Token"/>
    /// </summary>
    /// <param name="configuration">The configuration that defines how to generate the <see cref="OAuth2Token"/> to get</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>An <see cref="OAuth2Token"/></returns>
    Task<OAuth2Token> GetTokenAsync(OAuth2AuthenticationSchemeDefinition configuration, CancellationToken cancellationToken = default);

}
