using Synapse.Dashboard.Components;

namespace Synapse.Dashboard.Services
{

    public interface IToastBuilder
    {

        IToastBuilder WithLevel(LogLevel level);

        IToastBuilder WithHeader(string header);

        IToastBuilder WithoutHeader();

        IToastBuilder WithBody(string body);

        IToastBuilder WithLifetime(TimeSpan duration);

        IToastBuilder OnHide(Action callback);

        Toast Build();

    }

}
