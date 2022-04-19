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

namespace Synapse.Cli
{
    /// <summary>
    /// Defines extensions for <see cref="HttpClient"/>s
    /// </summary>
    public static class HttpClientExtensions
    {

        /// <summary>
        /// Download the file at the specified uri
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/> used to download</param>
        /// <param name="fileUri">The file uri</param>
        /// <param name="outputStream">The output <see cref="Stream"/></param>
        /// <param name="progress">The <see cref="ProgressTask"/> to use</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public static async Task DownloadAsync(this HttpClient client, string fileUri, Stream outputStream, ProgressTask progress, CancellationToken cancellationToken = default)
        {
            using (HttpResponseMessage response = await client.GetAsync(fileUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                response.EnsureSuccessStatusCode();
                progress.MaxValue(response.Content.Headers.ContentLength ?? 0);
                progress.StartTask();
                var filename = fileUri.Substring(fileUri.LastIndexOf('/') + 1);
                using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                var buffer = new byte[8192];
                while (true)
                {
                    var bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (bytesRead == 0)
                        break;
                    progress.Increment(bytesRead);
                    await outputStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                }
            }
        }

    }

}
