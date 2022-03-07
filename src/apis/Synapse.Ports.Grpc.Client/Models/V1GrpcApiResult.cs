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
using Synapse.Integration.Models;
using System.Runtime.Serialization;

namespace Synapse.Ports.Grpc.Models
{

    /// <summary>
    /// Represents a Protobuf-compliant object used to describe the result of an operation on the Synapse API
    /// </summary>
    [ProtoContract]
    public class V1GrpcApiResult
    {

        /// <summary>
        /// Initializes a new <see cref="V1GrpcApiResult"/>
        /// </summary>
        protected V1GrpcApiResult()
        {
            Code = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1GrpcApiResult"/>
        /// </summary>
        /// <param name="code">The <see cref="V1GrpcApiResult"/>'s code</param>
        /// <param name="errors">An array of errors that have occured during the operation's execution</param>
        public V1GrpcApiResult(string code, params ErrorDto[] errors)
        {
            Code = code;
            Errors = errors == null ? new List<ErrorDto>() : errors.ToList();
        }

        /// <summary>
        /// Initializes a new <see cref="V1GrpcApiResult"/>
        /// </summary>
        /// <param name="code">The <see cref="V1GrpcApiResult"/>'s code</param>
        public V1GrpcApiResult(string code)
            : this(code, Array.Empty<ErrorDto>())
        {

        }

        /// <summary>
        /// Gets the described result code
        /// </summary>
        [ProtoMember(1)]
        public string Code { get; internal set; }

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> containing the errors that have occured during the application's execution
        /// </summary>
        [ProtoMember(2)]
        public IEnumerable<ErrorDto>? Errors { get; internal set; }

        [ProtoIgnore]
        [IgnoreDataMember]
        public virtual bool Succeeded => this.Errors == null || !this.Errors.Any();

        /// <summary>
        /// Creates a new <see cref="V1GrpcApiResult"/> for the specified <see cref="IOperationResult"/>
        /// </summary>
        /// <param name="result">The <see cref="IOperationResult"/> to create a new <see cref="V1GrpcApiResult"/> for</param>
        /// <returns>A new <see cref="V1GrpcApiResult"/></returns>
        public static V1GrpcApiResult CreateFor(IOperationResult result)
        {
            return new V1GrpcApiResult()
            {
                Code = result.Code,
                Errors = result.Errors?.Select(e => new ErrorDto() { Code = e.Code, Message = e.Message })!
            };
        }

        /// <summary>
        /// Creates a new <see cref="V1GrpcApiResult"/> for the specified <see cref="IOperationResult"/>
        /// </summary>
        /// <typeparam name="T">The type of data wrapped by the <see cref="V1GrpcApiResult"/></typeparam>
        /// <param name="result">The <see cref="IOperationResult"/> to create a new <see cref="V1GrpcApiResult"/> for</param>
        /// <returns>A new <see cref="V1GrpcApiResult"/></returns>
        public static V1GrpcApiResult<T> CreateFor<T>(IOperationResult result)
        {
            return new V1GrpcApiResult<T>()
            {
                Code = result.Code,
                Errors = result.Errors?.Select(e => new ErrorDto() { Code = e.Code, Message = e.Message })!
            };
        }

        /// <summary>
        /// Creates a new <see cref="V1GrpcApiResult{T}"/> for the specified <see cref="IOperationResult"/>
        /// </summary>
        /// <typeparam name="T">The type of data wrapped by the <see cref="V1GrpcApiResult{T}"/></typeparam>
        /// <param name="result">The <see cref="IOperationResult"/> to create a new <see cref="V1GrpcApiResult"/> for</param>
        /// <returns>A new <see cref="V1GrpcApiResult"/></returns>
        public static V1GrpcApiResult<T> CreateFor<T>(IOperationResult<T> result)
        {
            return new V1GrpcApiResult<T>()
            {
                Code = result.Code,
                Errors = result.Errors?.Select(e => new ErrorDto() { Code = e.Code, Message = e.Message })!,
                Data = result.Data
            };
        }

    }

    /// <summary>
    /// Represents a Protobuf-compliant object used to describe the result of an operation on the Synapse API
    /// </summary>
    /// <typeparam name="T">The type of wrapped result</typeparam>
    [ProtoContract]
    public class V1GrpcApiResult<T>
        : V1GrpcApiResult
    {

        /// <summary>
        /// Initializes a new <see cref="V1GrpcApiResult"/>
        /// </summary>
        protected internal V1GrpcApiResult()
        {
            Code = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1GrpcApiResult"/>
        /// </summary>
        /// <param name="code">The <see cref="V1GrpcApiResult"/>'s code</param>
        /// <param name="errors">An array of errors that have occured during the operation's execution</param>
        public V1GrpcApiResult(string code, params ErrorDto[] errors)
            : base(code, errors)
        {
            Code = code;
        }

        /// <summary>
        /// Initializes a new <see cref="V1GrpcApiResult"/>
        /// </summary>
        /// <param name="code">The <see cref="V1GrpcApiResult"/>'s code</param>
        /// <param name="data">The data returned by the operation</param>
        public V1GrpcApiResult(string code, T? data)
            : base(code)
        {
            Code = code;
            Data = data;
        }

        /// <summary>
        /// Initializes a new <see cref="V1GrpcApiResult"/>
        /// </summary>
        /// <param name="code">The <see cref="V1GrpcApiResult"/>'s code</param>
        public V1GrpcApiResult(string code)
            : this(code, default(T))
        {

        }

        /// <inheritdoc/>
        [ProtoMember(1)]
        public new string Code { get; internal set; }

        /// <inheritdoc/>
        [ProtoMember(2)]
        public new IEnumerable<ErrorDto>? Errors { get; internal set; }

        /// <summary>
        /// Gets the data returned by the operation
        /// </summary>
        [ProtoMember(3)]
        public T? Data { get; internal set; }

    }

}
