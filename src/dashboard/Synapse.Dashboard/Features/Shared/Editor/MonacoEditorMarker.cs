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

namespace Synapse.Dashboard
{
    public class MonacoEditorMarker : IMonacoEditorMarker
    {
        public string? Code { get; set; }
        public int EndColumn { get; set; }
        public int EndLineNumber { get; set; }
        public string Message { get; set; } = "";
        public string Owner { get; set; } = "";
        public IEnumerable<Object>? RelatedInformation { get; set; }
        public Object Resource { get; set; } = "";
        public int Severity { get; set; }
        public int StartColumn { get; set; }
        public int StartLineNumber { get; set; }
        public IEnumerable<Object>? Tags { get; set; }
    }
}
