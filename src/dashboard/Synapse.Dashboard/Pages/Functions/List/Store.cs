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

using Synapse.Api.Client.Services;
using Synapse.Resources;

namespace Synapse.Dashboard.Pages.Functions.List;

/// <summary>
/// Represents the <see cref="View"/>'s store
/// </summary>
/// <param name="logger">The service used to perform logging</param>
/// <param name="apiClient">The service used to interact with the Synapse API</param>
/// <param name="resourceEventHub">The hub used to watch resource events</param>
public class FunctionListComponentStore(ILogger<FunctionListComponentStore> logger, ISynapseApiClient apiClient, ResourceWatchEventHubClient resourceEventHub)
    : ClusterResourceManagementComponentStore<CustomFunction>(logger, apiClient, resourceEventHub)
{



}
