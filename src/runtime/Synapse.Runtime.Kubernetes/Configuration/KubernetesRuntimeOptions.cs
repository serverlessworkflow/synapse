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

using k8s.Models;

namespace Synapse.Runtime.Kubernetes.Configuration
{

    /// <summary>
    /// Represents the options used to configure a Synapse Kubernetes-based runtime
    /// </summary>
    public class KubernetesRuntimeOptions
    {

        /// <summary>
        /// Gets the default worker <see cref="V1Pod"/>
        /// </summary>
        public static V1Pod DefaultWorkerPod = new()
        {
            Metadata = new V1ObjectMeta()
            {
                Annotations = new Dictionary<string, string>()
                {
                    { "traffic.sidecar.istio.io/excludeOutboundIPRanges", "10.96.0.1/32" }
                },
                Labels = new Dictionary<string, string>()
                {
                    { "app", "synapse-worker" },
                    { "version", typeof(KubernetesRuntimeOptions).Assembly.GetName().Version!.ToString(3) }
                }
            },
            Spec = new()
            {
                Containers = new List<V1Container>()
                {
                    new V1Container("synapse-worker")
                    {
                        Image = $"ghcr.io/serverlessworkflow/synapse-worker:{typeof(KubernetesRuntimeOptions).Assembly.GetName().Version!.ToString(3)}",
                        ImagePullPolicy = "Always",
                        Ports = new List<V1ContainerPort>()
                        {
                            new V1ContainerPort()
                            {
                                Name = "http",
                                ContainerPort = 42286,
                                Protocol = "TCP"
                            },
                            new V1ContainerPort()
                            {
                                Name = "http-2",
                                ContainerPort = 41387,
                                Protocol = "TCP"
                            }
                        }
                    }
                }
            }
        };

        /// <summary>
        /// Gets/sets the worker <see cref="V1Pod"/>
        /// </summary>
        public virtual V1Pod WorkerPod { get; set; } = DefaultWorkerPod;

    }

}
