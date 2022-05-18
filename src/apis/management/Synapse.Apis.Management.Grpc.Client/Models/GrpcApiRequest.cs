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

namespace Synapse.Apis.Management.Grpc.Models
{
    /// <summary>
    /// Represents a Grpc wrapper for an API request
    /// </summary>
    /// <typeparam name="T">The type of data wrapped by the request</typeparam>
    [ProtoContract]
    public class GrpcApiRequest<T>
    {

        /// <summary>
        /// Initializes a new <see cref="GrpcApiRequest{T}"/>
        /// </summary>
        /// <param name="data">The data wrapped by the <see cref="GrpcApiRequest{T}"/></param>
        public GrpcApiRequest(T data)
        {
            this.Data = data;
        }

        /// <summary>
        /// Gets the data wrapped by the <see cref="GrpcApiRequest{T}"/>
        /// </summary>
        [ProtoMember(1)]
        public virtual T Data { get; protected set; }

    }

}
