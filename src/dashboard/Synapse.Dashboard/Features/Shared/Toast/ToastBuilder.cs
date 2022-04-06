using Microsoft.AspNetCore.Components;

namespace Synapse.Dashboard
{
    public class ToastBuilder
        : IToastBuilder
    {

        protected Toast Toast { get; } = new();

        public virtual IToastBuilder WithLevel(LogLevel level)
        {
            this.Toast.Level = level;
            return this;
        }

        public virtual IToastBuilder WithLifetime(TimeSpan duration)
        {
            this.Toast.Duration = duration;
            return this;
        }

        public virtual IToastBuilder WithHeader(string header)
        {
            this.Toast.Header = (MarkupString)header;
            return this;
        }

        public virtual IToastBuilder WithoutHeader()
        {
            this.Toast.HasHeader = false;
            return this;
        }

        public virtual IToastBuilder WithBody(string body)
        {
            this.Toast.Body = (MarkupString)body;
            return this;
        }

        public virtual IToastBuilder OnHide(Action callback)
        {
            this.Toast.OnHideCallback = callback;
            return this;
        }

        public virtual Toast Build()
        {
            return this.Toast;
        }

    }

}
