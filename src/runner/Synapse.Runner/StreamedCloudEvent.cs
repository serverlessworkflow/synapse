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
/// Represents the default implementation of the <see cref="IStreamedCloudEvent"/> interface
/// </summary>
/// <param name="e">The streamed <see cref="CloudEvent"/></param>
/// <param name="offset">The position of the <see cref="CloudEvent"/> within its originating stream</param>
/// <param name="ackDelegate">A delegate used to ack that the <see cref="CloudEvent"/> has been successfully processed</param>
public class StreamedCloudEvent(CloudEvent e, uint offset, Func<uint, CancellationToken, Task> ackDelegate)
    : IStreamedCloudEvent
{

    /// <inheritdoc/>
    public virtual CloudEvent Event { get; } = e;

    /// <inheritdoc/>
    public virtual uint Offset { get; } = offset;

    /// <summary>
    /// Gets a delegate used to ack that the <see cref="CloudEvent"/> has been successfully processed
    /// </summary>
    protected Func<uint, CancellationToken, Task> AckDelegate { get; } = ackDelegate;

    /// <inheritdoc/>
    public virtual Task AckAsync(CancellationToken cancellationToken = default) => this.AckDelegate(this.Offset, cancellationToken);

}