// Copyright © 2024-Present Neuroglia SRL. All rights reserved.
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

namespace Synapse.Correlator.Commands.CloudEvents;

/// <summary>
/// Represents the command used to ingest a cloud event
/// </summary>
[DataContract]
public class IngestCloudEventCommand
    : Command
{

    /// <summary>
    /// Initializes a new <see cref="IngestCloudEventCommand"/>
    /// </summary>
    protected IngestCloudEventCommand() { }

    /// <summary>
    /// Initializes a new <see cref="IngestCloudEventCommand"/>
    /// </summary>
    /// <param name="e">The <see cref="CloudEvent"/> to publish</param>
    public IngestCloudEventCommand(CloudEvent e)
    {
        this.Event = e;
    }

    /// <summary>
    /// Gets the <see cref="CloudEvent"/> to publish
    /// </summary>
    public virtual CloudEvent Event { get; protected set; } = null!;

}


/// <summary>
/// Represents the service used to handle <see cref="IngestCloudEventCommand"/>s
/// </summary>
/// <param name="cloudEventBus">The service used to stream input and output <see cref="CloudEvent"/>s</param>
public class IngestCloudEventCommandHandler(ICloudEventBus cloudEventBus)
    : ICommandHandler<IngestCloudEventCommand>
{

    /// <summary>
    /// Gets the service used to stream input and output <see cref="CloudEvent"/>s
    /// </summary>
    protected ICloudEventBus CloudEventBus { get; } = cloudEventBus;

    /// <inheritdoc/>
    public virtual async Task<IOperationResult> HandleAsync(IngestCloudEventCommand command, CancellationToken cancellationToken = default)
    {
        this.CloudEventBus.InputStream.OnNext(command.Event);
        return await Task.FromResult(this.Ok());
    }

}