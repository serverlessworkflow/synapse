﻿using Microsoft.Extensions.Hosting;

namespace Synapse.Worker.Services
{

    /// <summary>
    /// Defines the fundamentals of a service used to run a <see cref="V1WorkflowInstance"/>
    /// </summary>
    public interface IWorkflowRuntime
        : IHostedService, IDisposable
    {


    }

}
