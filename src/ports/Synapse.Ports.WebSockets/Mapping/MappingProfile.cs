using AutoMapper;
using CloudNative.CloudEvents;
using Synapse.Ports.WebSockets.Client.Models;

namespace Synapse.Ports.WebSockets.Mapping
{

    /// <summary>
    /// Represents the Synapse Websocket API mapping profile
    /// </summary>
    public class MappingProfile
        : Profile
    {

        /// <summary>
        /// Initializes a new <see cref="MappingProfile"/>
        /// </summary>
        public MappingProfile()
        {
            this.CreateMap<CloudEvent, CloudEventDescriptor>();
        }

    }

}
