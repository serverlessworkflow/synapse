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

namespace Synapse.Operator.Services;

/// <summary>
/// Defines the fundamentals of the service used to access the current Synapse Operator
/// </summary>
public interface IOperatorController
    : IHostedService
{

    /// <summary>
    /// Gets the service used to monitor the current <see cref="Resources.Operator"/>
    /// </summary>
    IResourceMonitor<Resources.Operator> Operator { get; }

}