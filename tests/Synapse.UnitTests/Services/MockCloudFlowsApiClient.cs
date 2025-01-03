﻿// Copyright © 2024-Present The Synapse Authors
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
using Synapse.Api.Client.Services;

namespace Synapse.UnitTests.Services;

internal class MockSynapseApiClient(IServiceProvider serviceProvider)
    : ISynapseApiClient
{

    public INamespacedResourceApiClient<Correlation> Correlations { get; } = ActivatorUtilities.CreateInstance<MockNamespacedResourceApiClient<Correlation>>(serviceProvider);

    public INamespacedResourceApiClient<Resources.Correlator> Correlators { get; } = ActivatorUtilities.CreateInstance<MockNamespacedResourceApiClient<Resources.Correlator>>(serviceProvider);

    public IClusterResourceApiClient<CustomFunction> CustomFunctions { get; } = ActivatorUtilities.CreateInstance<MockClusterResourceApiClient<CustomFunction>>(serviceProvider);

    public IClusterResourceApiClient<Namespace> Namespaces { get; } = ActivatorUtilities.CreateInstance<MockClusterResourceApiClient<Namespace>>(serviceProvider);

    public INamespacedResourceApiClient<Operator> Operators { get; } = ActivatorUtilities.CreateInstance<MockNamespacedResourceApiClient<Operator>>(serviceProvider);

    public INamespacedResourceApiClient<Workflow> Workflows { get; } = ActivatorUtilities.CreateInstance<MockNamespacedResourceApiClient<Workflow>>(serviceProvider);

    public IWorkflowInstanceApiClient WorkflowInstances { get; } = ActivatorUtilities.CreateInstance<MockWorkflowInstanceApiClient>(serviceProvider);

    public IDocumentApiClient Documents { get; } = ActivatorUtilities.CreateInstance<MockDocumentApiClient>(serviceProvider);

    public ICloudEventApiClient Events { get; } = ActivatorUtilities.CreateInstance<MockEventApiClient>(serviceProvider);

    public INamespacedResourceApiClient<ServiceAccount> ServiceAccounts => throw new NotImplementedException();

    public IUserApiClient Users => throw new NotImplementedException();

}
