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
    /// Defines extensions for <see cref="FileInfo"/>s
    /// </summary>
    public static class FileInfoExtensions
    {

        /// <summary>
        /// Determines whether or not the <see cref="FileInfo"/> is locked
        /// </summary>
        /// <param name="file">The <see cref="FileInfo"/> to check</param>
        /// <returns>A boolean indicating whether or not the <see cref="FileInfo"/> is locked</returns>
        /// <remarks>Code taken from <see href="https://stackoverflow.com/a/10982239/3637555"/></remarks>
        public static bool IsLocked(this FileInfo file)
        {
            var stream = null as FileStream;
            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return false;
        }

    }

}
