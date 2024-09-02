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

namespace Synapse.Runner;

/// <summary>
/// Represents the <see cref="Exception"/> thrown when an <see cref="Synapse.Error"/> has been raised during the execution of a workflow
/// </summary>
/// <param name="error">The <see cref="Synapse.Error"/> that has been raised</param>
public class ErrorRaisedException(Error error)
    : Exception
{

    /// <summary>
    /// Gets the <see cref="Synapse.Error"/> that has been raised
    /// </summary>
    public virtual Error Error { get; } = error;

}