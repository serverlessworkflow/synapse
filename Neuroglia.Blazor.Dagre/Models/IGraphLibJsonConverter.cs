namespace Neuroglia.Blazor.Dagre.Models
{
    /// <summary>
    /// Used to serialize/deserialize a <see cref="IGraphLib"/> to/from JSON
    /// </summary>
    public interface IGraphLibJsonConverter
    {
        /// <summary>
        /// Serializes a <see cref="IGraphLib"/> to JSON (aka GraphLib json.write(g))
        /// </summary>
        /// <param name="graph"></param>
        /// <returns>The serialized <see cref="IGraphLib"/></returns>
        Task<string> SerializeAsync(IGraphLib graph);
        /// <summary>
        /// Deserializes a JSON to a 
        /// </summary>
        /// <param name="json"></param> <see cref="IGraphLib"/> (aka GraphLib json.read(json))
        /// <returns>The deserialied <see cref="IGraphLib"/></returns>
        Task<IGraphLib> DeserializeAsync(string json);
    }
}
