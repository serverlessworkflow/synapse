using CloudNative.CloudEvents;
using Synapse.Services;
using System;

namespace Synapse.Configuration
{

    /// <summary>
    /// Represents the options used to configure a <see cref="CloudEventBus"/>
    /// </summary>
    public class CloudEventBusOptions
    {

        /// <summary>
        /// Initializes a new <see cref="CloudEventBus"/>
        /// </summary>
        public CloudEventBusOptions()
        {
            string raw = SynapseConstants.EnvironmentVariables.CloudEvents.Sink.Value;
            if (!string.IsNullOrWhiteSpace(raw)
                && Uri.TryCreate(raw, UriKind.RelativeOrAbsolute, out Uri sinkUri))
                this.SinkUri = sinkUri;
        }

        /// <summary>
        /// Gets/sets the <see cref="Uri"/> to post produced <see cref="CloudEvent"/>s to
        /// </summary>
        public Uri SinkUri { get; set; }
        
    }

}
