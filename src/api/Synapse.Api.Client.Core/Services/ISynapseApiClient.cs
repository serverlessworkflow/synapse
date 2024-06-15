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

namespace Synapse.Api.Client.Services;

/// <summary>
/// Defines the fundamentals of a Synapse API client
/// </summary>
public interface ISynapseApiClient
{

    /// <summary>
    /// Gets the Synapse API used to manage <see cref="Correlation"/>s
    /// </summary>
    INamespacedResourceApiClient<Correlation> Correlations { get; }

    /// <summary>
    /// Gets the Synapse API used to manage <see cref="Correlator"/>s
    /// </summary>
    INamespacedResourceApiClient<Correlator> Correlators { get; }

    /// <summary>
    /// Gets the Synapse API used to manage <see cref="Namespace"/>s
    /// </summary>
    IClusterResourceApiClient<Namespace> Namespaces { get; }

    /// <summary>
    /// Gets the Synapse API used to manage <see cref="Operator"/>s
    /// </summary>
    INamespacedResourceApiClient<Operator> Operators { get; }

    /// <summary>
    /// Gets the Synapse API used to manage <see cref="ServiceAccount"/>s
    /// </summary>
    INamespacedResourceApiClient<ServiceAccount> ServiceAccounts { get; }

    /// <summary>
    /// Gets the Synapse API used to manage users
    /// </summary>
    IUserApiClient Users { get; }

    /// <summary>
    /// Gets the Synapse API used to manage <see cref="Workflow"/>s
    /// </summary>
    INamespacedResourceApiClient<Workflow> Workflows { get; }

    /// <summary>
    /// Gets the Synapse API used to manage workflow data <see cref="Document"/>s
    /// </summary>
    IDocumentApiClient WorkflowData { get; }

    /// <summary>
    /// Gets the Synapse API used to manage <see cref="WorkflowInstance"/>s
    /// </summary>
    INamespacedResourceApiClient<WorkflowInstance> WorkflowInstances { get; }

}
