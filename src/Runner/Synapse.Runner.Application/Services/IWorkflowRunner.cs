using Microsoft.Extensions.Hosting;
using Synapse.Domain.Models;
using System;

namespace Synapse.Runner.Application.Services
{

    /// <summary>
    /// Defines the fundamentals of a service used to run <see cref="V1WorkflowInstance"/>s
    /// </summary>
    public interface IWorkflowRunner
        : IHostedService, IDisposable
    {



    }

}
