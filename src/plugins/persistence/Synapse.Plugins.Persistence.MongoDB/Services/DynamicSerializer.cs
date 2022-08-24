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

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Neuroglia.Serialization;

namespace Synapse.Plugins.Persistence.MongoDB.Services
{
    /// <summary>
    /// Represents an <see cref="IBsonSerializer"/> implementation used to serialize and deserialize <see cref="Dynamic"/>s
    /// </summary>
    public class DynamicSerializer
        : SerializerBase<Dynamic>
    {

        /// <inheritdoc/>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Dynamic value)
        {
            IBsonSerializer serializer = null!;
            object toSerialize = value;
            if (value == null)
            {
                serializer = BsonSerializer.LookupSerializer<BsonNull>();
                toSerialize = BsonNull.Value;
            }
            else
            {
                switch (value)
                {
                    case DynamicArray:
                        serializer = BsonSerializer.LookupSerializer<DynamicArray>();
                        break;
                    case DynamicObject:
                        serializer = BsonSerializer.LookupSerializer<DynamicObject>();
                        break;
                    case DynamicValue val:
                        toSerialize = val.ToObject();
                        serializer = BsonSerializer.LookupSerializer(DynamicHelper.GetClrType(val.Type));
                        break;
                }
            }
            serializer!.Serialize(context, args, toSerialize);
        }

        /// <inheritdoc/>
        public override Dynamic Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            switch (context.Reader.CurrentBsonType)
            {
                case BsonType.Array:
                    var arraySerializer = BsonSerializer.LookupSerializer<DynamicArray>();
                    return arraySerializer.Deserialize(context, args);
                case BsonType.Document:
                    var documentSerializer = BsonSerializer.LookupSerializer<DynamicObject>();
                    return documentSerializer.Deserialize(context, args);
                default:
                    var deserializer = BsonSerializer.LookupSerializer(typeof(object));
                    var deserialized = deserializer.Deserialize(context, args);
                    return Dynamic.FromObject(deserialized)!;
            }
        }

    }

}
