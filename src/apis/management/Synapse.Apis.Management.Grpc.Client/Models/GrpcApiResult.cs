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
    /// Represents a Protobuf-compliant object used to describe the result of an operation on the Synapse Management API
    /// </summary>
    [ProtoContract]
    public class GrpcApiResult
    {

        /// <summary>
        /// Initializes a new <see cref="GrpcApiResult"/>
        /// </summary>
        protected GrpcApiResult()
        {
            Code = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="GrpcApiResult"/>
        /// </summary>
        /// <param name="code">The <see cref="GrpcApiResult"/>'s code</param>
        /// <param name="errors">An array of errors that have occured during the operation's execution</param>
        public GrpcApiResult(string code, params Error[] errors)
        {
            Code = code;
            Errors = errors == null ? new List<Error>() : errors.ToList();
        }

        /// <summary>
        /// Initializes a new <see cref="GrpcApiResult"/>
        /// </summary>
        /// <param name="code">The <see cref="GrpcApiResult"/>'s code</param>
        public GrpcApiResult(string code)
            : this(code, Array.Empty<Error>())
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
        public IEnumerable<Error>? Errors { get; internal set; }

        /// <summary>
        /// Gets a boolean indicating whether or not the operation was successfull
        /// </summary>
        [ProtoIgnore]
        [IgnoreDataMember]
        public virtual bool Succeeded => this.Errors == null || !this.Errors.Any();

        /// <summary>
        /// Creates a new <see cref="GrpcApiResult"/> for the specified <see cref="IOperationResult"/>
        /// </summary>
        /// <param name="result">The <see cref="IOperationResult"/> to create a new <see cref="GrpcApiResult"/> for</param>
        /// <returns>A new <see cref="GrpcApiResult"/></returns>
        public static GrpcApiResult CreateFor(IOperationResult result)
        {
            return new GrpcApiResult()
            {
                Code = result.Code,
                Errors = result.Errors?.Select(e => new Error() { Code = e.Code, Message = e.Message })!
            };
        }

        /// <summary>
        /// Creates a new <see cref="GrpcApiResult"/> for the specified <see cref="IOperationResult"/>
        /// </summary>
        /// <typeparam name="T">The type of data wrapped by the <see cref="GrpcApiResult"/></typeparam>
        /// <param name="result">The <see cref="IOperationResult"/> to create a new <see cref="GrpcApiResult"/> for</param>
        /// <returns>A new <see cref="GrpcApiResult"/></returns>
        public static GrpcApiResult<T> CreateFor<T>(IOperationResult result)
        {
            return new GrpcApiResult<T>()
            {
                Code = result.Code,
                Errors = result.Errors?.Select(e => new Error() { Code = e.Code, Message = e.Message })!
            };
        }

        /// <summary>
        /// Creates a new <see cref="GrpcApiResult{T}"/> for the specified <see cref="IOperationResult"/>
        /// </summary>
        /// <typeparam name="T">The type of data wrapped by the <see cref="GrpcApiResult{T}"/></typeparam>
        /// <param name="result">The <see cref="IOperationResult"/> to create a new <see cref="GrpcApiResult"/> for</param>
        /// <returns>A new <see cref="GrpcApiResult"/></returns>
        public static GrpcApiResult<T> CreateFor<T>(IOperationResult<T> result)
        {
            return new GrpcApiResult<T>()
            {
                Code = result.Code,
                Errors = result.Errors?.Select(e => new Error() { Code = e.Code, Message = e.Message })!,
                Data = result.Data
            };
        }

    }

    /// <summary>
    /// Represents a Protobuf-compliant object used to describe the result of an operation on the Synapse Management API
    /// </summary>
    /// <typeparam name="T">The type of wrapped result</typeparam>
    [ProtoContract]
    public class GrpcApiResult<T>
        : GrpcApiResult
    {

        /// <summary>
        /// Initializes a new <see cref="GrpcApiResult"/>
        /// </summary>
        protected internal GrpcApiResult()
        {
            Code = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="GrpcApiResult"/>
        /// </summary>
        /// <param name="code">The <see cref="GrpcApiResult"/>'s code</param>
        /// <param name="errors">An array of errors that have occured during the operation's execution</param>
        public GrpcApiResult(string code, params Error[] errors)
            : base(code, errors)
        {
            Code = code;
        }

        /// <summary>
        /// Initializes a new <see cref="GrpcApiResult"/>
        /// </summary>
        /// <param name="code">The <see cref="GrpcApiResult"/>'s code</param>
        /// <param name="data">The data returned by the operation</param>
        public GrpcApiResult(string code, T? data)
            : base(code)
        {
            Code = code;
            Data = data;
        }

        /// <summary>
        /// Initializes a new <see cref="GrpcApiResult"/>
        /// </summary>
        /// <param name="code">The <see cref="GrpcApiResult"/>'s code</param>
        public GrpcApiResult(string code)
            : this(code, default(T))
        {

        }

        /// <inheritdoc/>
        [ProtoMember(1)]
        public new string Code { get; internal set; }

        /// <inheritdoc/>
        [ProtoMember(2)]
        public new IEnumerable<Error>? Errors { get; internal set; }

        /// <summary>
        /// Gets the data returned by the operation
        /// </summary>
        [ProtoMember(3)]
        public T? Data { get; internal set; }

    }

}
