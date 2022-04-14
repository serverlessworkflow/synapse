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

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Synapse.Dashboard
{
    public class Dynamic
        : ComponentBase
    {

        [Parameter]
        public string Tag { get; set; }

        [Parameter]
        public Dictionary<string, object> AdditionalAttributes { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public string Class { get; set; }

        [Parameter]
        public EventCallback OnClick { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (string.IsNullOrWhiteSpace(Tag))
                throw new ArgumentNullException(nameof(Tag));
            builder.OpenElement(0, Tag);
            if (this.AdditionalAttributes == null)
                this.AdditionalAttributes = new();
            this.AdditionalAttributes["class"] = this.Class;
            this.AdditionalAttributes["onclick"] = this.OnClick;
            builder.AddMultipleAttributes(2, AdditionalAttributes);
            if (ChildContent != null)
                builder.AddContent(3, ChildContent);
            builder.CloseElement();
        }

    }
}
