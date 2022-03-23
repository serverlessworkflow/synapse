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

namespace Synapse
{

    /// <summary>
    /// Defines extensions for <see cref="IMediator"/>s
    /// </summary>
    public static class IMediatorExtensions
    {

        /// <summary>
        /// Executes and unwraps the result of the specified <see cref="ICommand"/>
        /// </summary>
        /// <typeparam name="TResult">The expected type of result returned by the <see cref="ICommand"/> to process</typeparam>
        /// <param name="mediator">The <see cref="IMediator"/> to use to execute the <see cref="ICommand"/></param>
        /// <param name="request">The <see cref="ICommand"/> to execute</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        /// <exception cref="OperationResultException">A new <see cref="OperationResultException"/> in case errors occured during the <see cref="IRequest"/>'s execution</exception>
        public static async Task ExecuteAndUnwrapAsync<TResult>(this IMediator mediator, ICommand<TResult> request, CancellationToken cancellationToken = default)
            where TResult : IOperationResult
        {
            var result = await mediator.ExecuteAsync(request, cancellationToken);
            if (!result.Succeeded)
                throw new OperationResultException(result);
        }

        /// <summary>
        /// Executes and unwraps the result of the specified <see cref="ICommand"/>
        /// </summary>
        /// <typeparam name="TResult">The expected type of result returned by the <see cref="ICommand"/> to process</typeparam>
        /// <typeparam name="T">The type of data wrapped by the result returned by the <see cref="ICommand"/> to process</typeparam>
        /// <param name="mediator">The <see cref="IMediator"/> to use to execute the <see cref="ICommand"/></param>
        /// <param name="request">The <see cref="ICommand"/> to execute</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        /// <exception cref="OperationResultException">A new <see cref="OperationResultException"/> in case errors occured during the <see cref="ICommand"/>'s execution</exception>
        public static async Task<T> ExecuteAndUnwrapAsync<TResult, T>(this IMediator mediator, ICommand<TResult, T> request, CancellationToken cancellationToken = default)
            where TResult : IOperationResult<T>
        {
            var result = await mediator.ExecuteAsync(request, cancellationToken);
            if (!result.Succeeded)
                throw new OperationResultException(result);
            return result.Data;
        }

        /// <summary>
        /// Executes and unwraps the result of the specified <see cref="IQuery"/>
        /// </summary>
        /// <typeparam name="TResult">The expected type of result returned by the <see cref="IQuery"/> to process</typeparam>
        /// <typeparam name="T">The type of data wrapped by the result returned by the <see cref="IQuery"/> to process</typeparam>
        /// <param name="mediator">The <see cref="IMediator"/> to use to execute the <see cref="IQuery"/></param>
        /// <param name="request">The <see cref="IQuery"/> to execute</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        /// <exception cref="OperationResultException">A new <see cref="OperationResultException"/> in case errors occured during the <see cref="IQuery"/>'s execution</exception>
        public static async Task<T> ExecuteAndUnwrapAsync<TResult, T>(this IMediator mediator, IQuery<TResult, T> request, CancellationToken cancellationToken = default)
            where TResult : IOperationResult<T>
        {
            var result = await mediator.ExecuteAsync(request, cancellationToken);
            if (!result.Succeeded)
                throw new OperationResultException(result);
            return result.Data;
        }

    }

}
