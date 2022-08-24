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
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Neuroglia.Serialization;
using Newtonsoft.Json;

namespace Synapse.Plugins.Persistence.MongoDB.Services
{
    /// <summary>
    /// Represents an <see cref="IBsonSerializer"/> implementation used to serialize and deserialize <see cref="DynamicArray"/>s
    /// </summary>
    public class DynamicArraySerializer
        : SerializerBase<DynamicArray>
    {

        /// <inheritdoc/>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DynamicArray value)
        {
            var jsonDocument = Newtonsoft.Json.JsonConvert.SerializeObject(value, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            var bsonDocument = value == null ? null : BsonSerializer.Deserialize<BsonArray>(jsonDocument);
            var serializer = new BsonValueCSharpNullSerializer<BsonArray>(BsonSerializer.LookupSerializer<BsonArray>());
            serializer.Serialize(context, bsonDocument!);
        }

        /// <inheritdoc/>
        public override DynamicArray Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var serializer = new BsonValueCSharpNullSerializer<BsonArray>(BsonSerializer.LookupSerializer<BsonArray>());
            var bsonArray = serializer.Deserialize(context, args);
            if (bsonArray == null)
                return null!;
            var result = bsonArray.ToJson(new() { OutputMode = JsonOutputMode.RelaxedExtendedJson });
            var array = Newtonsoft.Json.JsonConvert.DeserializeObject<List<object>>(result)!;
            return DynamicArray.FromObject(array)!;
        }

    }

}
