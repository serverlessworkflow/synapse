using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Neuroglia.Serialization.Json;

namespace Synapse.Serialization.Bson;

/// <summary>
/// Represents the <see cref="SerializerBase{TValue}"/> implementation to serialize/deserialize untyped objects to/from BSON
/// </summary>
public class ObjectSerializer
    : SerializerBase<object>
{

    /// <inheritdoc/>
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
    {
        var json = value == null ? null : JsonSerializer.Default.SerializeToText(value);
        context.Writer.WriteJavaScript(json);
    }

    /// <inheritdoc/>
    public override object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var json = context.Reader.ReadJavaScript();
        return JsonSerializer.Default.Deserialize<object>(json)!;
    }

}
