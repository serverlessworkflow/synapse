using Microsoft.AspNetCore.SignalR.Client;
using Neuroglia.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Synapse.Dashboard.Components;
using Synapse.Ports.WebSockets.Client.Models;
using Synapse.Ports.WebSockets.Client.Services;
using System.Reactive.Subjects;

namespace Synapse.Dashboard.Services
{
    /// <summary>
    /// Represents the default implementation of the <see cref="IIntegrationEventStream"/> interface
    /// </summary>
    public class IntegrationEventStream
        : IIntegrationEventStream
    {

        /// <summary>
        /// Initializes a new <see cref="IntegrationEventStream"/>
        /// </summary>
        /// <param name="hubConnection">The current <see cref="Microsoft.AspNetCore.SignalR.Client.HubConnection"/></param>
        /// <param name="toastManager">The service used to manage <see cref="Toast"/>s</param>
        /// <param name="jsonSerializer">The service used to serialize/deserialize to/from JSON</param>
        public IntegrationEventStream(ILogger<IntegrationEventStream> logger, HubConnection hubConnection, IToastManager toastManager, IJsonSerializer jsonSerializer)
        {
            this.Logger = logger;
            this.HubConnection = hubConnection;
            //this.Subscription = this.HubConnection.On<CloudEventDescriptor>(nameof(ISynapseWebSocketApiClient.PublishIntegrationEvent), On); //We probably don't want to create a toast for each single event. If have to notify, do it in a more subtle way
            this.ToastManager = toastManager;
            this.JsonSerializer = jsonSerializer;
        }

        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the <see cref="IntegrationEventStream"/>'s subscription
        /// </summary>
        protected IDisposable Subscription { get; }

        /// <summary>
        /// Gets the current <see cref="Microsoft.AspNetCore.SignalR.Client.HubConnection"/>
        /// </summary>
        protected HubConnection HubConnection { get; }

        /// <summary>
        /// Gets the service used to manage <see cref="Toast"/>s
        /// </summary>
        protected IToastManager ToastManager { get; }

        /// <summary>
        /// Gets the service used to serialize/deserialize to/from JSON
        /// </summary>
        protected IJsonSerializer JsonSerializer { get; }

        /// <summary>
        /// Gets the <see cref="Subject{T}"/> used to observe <see cref=""/>s consumed by the <see cref="IntegrationEventStream"/>
        /// </summary>
        protected Subject<CloudEventDescriptor> Stream { get; } = new();

        /// <inheritdoc/>
        public virtual IDisposable Subscribe(IObserver<CloudEventDescriptor> observer)
        {
            return this.Stream.Subscribe(observer);
        }

        /// <summary>
        /// Handles the specified <see cref="CloudEventDescriptor"/>
        /// </summary>
        /// <param name="e">The <see cref="CloudEventDescriptor"/> to handle</param>
        protected virtual async void On(CloudEventDescriptor e)
        {
            try
            {
                var body = string.Empty;
                if (e.Data is JObject jobject)
                    body = $"<pre><code>{jobject.ToString(Formatting.Indented)}</code></pre>";
                this.ToastManager.ShowToast(toast => toast
                    .WithLevel(LogLevel.Information)
                    .WithHeader(e.Type)
                    .WithBody(body));
                this.Stream.OnNext(e);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex.ToString());
            }

        }

    }

}
