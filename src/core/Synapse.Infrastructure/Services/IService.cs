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

namespace Synapse.Infrastructure.Services
{
    /// <summary>
    /// Defines the fundamentals of a service
    /// </summary>
    public interface IService
    {

        /// <summary>
        /// Waits for the <see cref="IService"/> to start
        /// </summary>
        /// <param name="stoppingToken">A <see cref="CancellationToken"/> used to control the <see cref="IService"/>'s lifetime</param>
        /// <returns>A new awaitable <see cref="ValueTask"/></returns>
        ValueTask WaitForStartupAsync(CancellationToken stoppingToken);

    }

}
