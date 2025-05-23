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

namespace Synapse.Runner;

/// <summary>
/// Defines the fundamentals of an object used to wrap a streamed <see cref="CloudEvent"/>
/// </summary>
public interface IStreamedCloudEvent
{

    /// <summary>
    /// Gets the streamed <see cref="CloudEvent"/>
    /// </summary>
    CloudEvent Event { get; }

    /// <summary>
    /// Gets the position of the <see cref="CloudEvent"/> within its originating stream
    /// </summary>
    uint Offset { get; }

    /// <summary>
    /// Acknowledges that the <see cref="CloudEvent"/> has been successfully processed
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task AckAsync(CancellationToken cancellationToken = default);

}
