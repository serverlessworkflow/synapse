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

using Neuroglia.Security;
using Neuroglia.Security.Services;

namespace Synapse.Api.Application.Queries.Users;

/// <summary>
/// Represents the query used to get the current user's profile
/// </summary>
public class GetUserProfileQuery
    : Query<UserInfo>
{


}

/// <summary>
/// Represents the service used to handle <see cref="GetUserProfileQuery"/> instances
/// </summary>
/// <param name="userInfoProvider">The service used to describe the current user</param>
public class GetUserProfileQueryHandler(IUserInfoProvider userInfoProvider)
    : IQueryHandler<GetUserProfileQuery, UserInfo>
{

    /// <summary>
    /// Gets the service used to describe the current user
    /// </summary>
    protected IUserInfoProvider UserInfoProvider { get; } = userInfoProvider;

    /// <inheritdoc/>
    public virtual Task<IOperationResult<UserInfo>> HandleAsync(GetUserProfileQuery query, CancellationToken cancellationToken = default)
    {
        var userInfo = this.UserInfoProvider.GetCurrentUser();
        if (userInfo == null) return Task.FromResult(this.Forbidden());
        else return Task.FromResult(this.Ok(userInfo));
    }

}
