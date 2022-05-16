/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using k8s;
using k8s.Models;
using Synapse.Application.Services;
using Synapse.Infrastructure.Services;

namespace Synapse.Runtime.Kubernetes.Services
{

    /// <summary>
    /// Represents a Kubernetes-native <see cref="IWorkflowProcess"/>
    /// </summary>
    public class KubernetesProcess
        : WorkflowProcessBase
    {

        /// <summary>
        /// Initializes a new <see cref="KubernetesProcess"/>
        /// </summary>
        /// <param name="pod">The managed <see cref="V1Pod"/></param>
        /// <param name="kubernetes">The service used to interact with the Kubernetes API</param>
        public KubernetesProcess(V1Pod pod, IKubernetes kubernetes) 
        {
            this.Pod = pod;
            this.Kubernetes = kubernetes;
        }

        /// <inheritdoc/>
        public override string Id => $"{this.Pod.Name()}.{this.Pod.Namespace()}";

        /// <summary>
        /// Gets the managed <see cref="V1Pod"/>
        /// </summary>
        protected V1Pod Pod { get; set; }

        /// <summary>
        /// Gets the service used to interact with the Kubernetes API
        /// </summary>
        protected IKubernetes Kubernetes { get; }

        private IObservable<string> _Logs;
        /// <inheritdoc/>
        public override IObservable<string> Logs => this._Logs;

        private int? _ExitCode;
        /// <inheritdoc/>
        public override long? ExitCode => this._ExitCode;

        /// <inheritdoc/>
        public override async ValueTask StartAsync(CancellationToken cancellationToken = default)
        {
            this.Pod = await this.Kubernetes.CreateNamespacedPodAsync(this.Pod, this.Pod.Namespace(), cancellationToken: cancellationToken);
        }

        /// <inheritdoc/>
        public override async ValueTask TerminateAsync(CancellationToken cancellationToken = default)
        {
            await this.Kubernetes.DeleteNamespacedPodAsync(this.Pod.Name(), this.Pod.Namespace(), cancellationToken: cancellationToken);
        }

    }

}
