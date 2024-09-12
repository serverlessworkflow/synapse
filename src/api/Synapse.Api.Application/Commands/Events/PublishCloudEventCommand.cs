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

using Neuroglia.Eventing.CloudEvents;
using Synapse.Api.Application.Services;

namespace Synapse.Api.Application.Commands.Events;

/// <summary>
/// Represents the <see cref="ICommand"/> used to publish a <see cref="Neuroglia.Eventing.CloudEvents.CloudEvent"/> to the configured sink
/// </summary>
/// <param name="e">The <see cref="Neuroglia.Eventing.CloudEvents.CloudEvent"/> to publish</param>
public class PublishCloudEventCommand(CloudEvent e)
    : Command
{

    /// <summary>
    /// Gets the <see cref="Neuroglia.Eventing.CloudEvents.CloudEvent"/> to publish
    /// </summary>
    public virtual CloudEvent CloudEvent { get; } = e;

}

/// <summary>
/// Represents the service used to handle <see cref="PublishCloudEventCommand"/>s
/// </summary>
/// <param name="cloudEventPublisher">The service used to publish <see cref="CloudEvent"/>s</param>
public class PublishCloudEventCommandHandler(ICloudEventPublisher cloudEventPublisher)
    : ICommandHandler<PublishCloudEventCommand>
{

    /// <inheritdoc/>
    public virtual async Task<IOperationResult> HandleAsync(PublishCloudEventCommand command, CancellationToken cancellationToken = default)
    {
        await cloudEventPublisher.PublishAsync(command.CloudEvent, cancellationToken).ConfigureAwait(false);
        return this.Ok();
    }

}