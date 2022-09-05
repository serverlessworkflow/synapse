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
    /// Defines extensions for <see cref="DirectoryInfo"/>s
    /// </summary>
    public static class DirectoryInfoExtensions
    {

        /// <summary>
        /// Copies the <see cref="Directory"/> to the specified target <see cref="Directory"/>
        /// </summary>
        /// <param name="sourceDirectory">The source <see cref="Directory"/></param>
        /// <param name="targetDirectory">The target <see cref="Directory"/></param>

        public static void CopyTo(this DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory)
        {
            foreach (var directory in sourceDirectory.GetDirectories("*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(directory.FullName.Replace(sourceDirectory.FullName, targetDirectory.FullName));
            }
            foreach (var file in sourceDirectory.GetFiles("*.*", SearchOption.AllDirectories))
            {
                File.Copy(file.FullName, file.FullName.Replace(sourceDirectory.FullName, targetDirectory.FullName), true);
            }
        }

    }

}
