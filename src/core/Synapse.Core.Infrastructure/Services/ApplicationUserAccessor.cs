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

using Neuroglia.Security.Services;
using System.Security.Claims;

namespace Synapse.Core.Infrastructure.Services;

/// <summary>
/// Represents an <see cref="IUserAccessor"/> implementation used to access the user that represents the executing application
/// </summary>
public class ApplicationUserAccessor
    : IUserAccessor
{

    /// <inheritdoc/>
    public ClaimsPrincipal? User => new(new ClaimsIdentity());

}
