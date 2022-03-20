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
