using Synapse.Ports.Grpc.Models;

namespace Synapse.Ports.Grpc.Services
{

    /// <summary>
    /// Represents an exception originating from or concerning the Synapse API
    /// </summary>
    public class SynapseApiException
        : Exception
    {

        /// <summary>
        /// Initializes a new <see cref="SynapseApiException"/>
        /// </summary>
        /// <param name="result">The <see cref="V1GrpcApiResult"/> which is the cause of the <see cref="SynapseApiException"/></param>
        public SynapseApiException(V1GrpcApiResult result)
            : base($"The Synapse API responded with a non-success result code '{result.Code}'.{Environment.NewLine}Errors: {(result.Errors == null ? "" : string.Join(Environment.NewLine, result.Errors.Select(e => $"{e.Code}: {e.Message}")))}")
        {

        }

    }

}
