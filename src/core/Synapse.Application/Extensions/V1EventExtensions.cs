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
    /// Defines extensions for <see cref="V1Event"/>s
    /// </summary>
    public static class V1EventExtensions
    {

        /// <summary>
        /// Converts the <see cref="V1Event"/> into a new <see cref="CloudEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1Event"/> to convert</param>
        /// <returns>A new <see cref="CloudEvent"/></returns>
        public static CloudEvent AsCloudEvent(this V1Event e)
        {
            var specVersion = CloudEventsSpecVersion.FromVersionId(e.SpecVersion);
            if (specVersion == null) specVersion = CloudEventsSpecVersion.Default;
            var ce = new CloudEvent(specVersion);
            ce.Id = e.Id;
            ce.Time = e.Time;
            ce.Source = e.Source;
            ce.Type = e.Type;
            ce.Subject = e.Subject;
            ce.DataContentType = e.DataContentType;
            ce.DataSchema = e.DataSchema;
            ce.Data = e.Data;
            foreach(var extension in e.Extensions)
            {
                ce.SetAttributeFromString(extension.Key, extension.Value?.ToString()!);
            }
            return ce;
        }

    }

}
