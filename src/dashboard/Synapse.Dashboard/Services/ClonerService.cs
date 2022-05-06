using Neuroglia.Serialization;

namespace Synapse.Dashboard.Services
{
    public class ClonerService
        : IClonerService
    {
        public ClonerService(IJsonSerializer serializer)
        {
            this.Serializer = serializer;
        }

        protected IJsonSerializer Serializer { get; set; }

        public T Clone<T>(T obj) => this.Serializer.Deserialize<T>(this.Serializer.Serialize(obj))!;
        public async Task<T> CloneAsync<T>(T obj) => await this.Serializer.DeserializeAsync<T>(await this.Serializer.SerializeAsync(obj))!;
    }
}
