using CloudNative.CloudEvents;
using Microsoft.Extensions.Hosting;
using System;

namespace Synapse.Correlator.Application.Services
{

    /// <summary>
    /// Defines the fundamentals of a service used to correlate incoming <see cref="CloudEvent"/>s
    /// </summary>
    public interface ICorrelationService
        : IHostedService, IDisposable
    {



    }

}
