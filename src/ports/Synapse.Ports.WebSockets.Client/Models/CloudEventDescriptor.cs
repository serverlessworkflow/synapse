namespace Synapse.Ports.WebSockets.Client.Models
{

    public class CloudEventDescriptor
    {

        public virtual Uri Source { get; set; }

        public virtual string Type { get; set; }

        public virtual Uri? DataSchema { get; set; }

        public virtual object? Data { get; set; }

        public virtual IDictionary<string, object> Extensions { get; set; }

    }

}
