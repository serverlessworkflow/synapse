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
    /// Represents an <see cref="IBsonSerializer"/> implementation used to serialize and deserialize <see cref="DynamicObject"/>s
    /// </summary>
    public class DynamicObjectSerializer
        : SerializerBase<DynamicObject>
    {

        /// <inheritdoc/>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DynamicObject value)
        {
            var jsonDocument = Newtonsoft.Json.JsonConvert.SerializeObject(value, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            var bsonDocument = value == null ? null : BsonSerializer.Deserialize<BsonDocument>(jsonDocument);
            var serializer = new BsonValueCSharpNullSerializer<BsonDocument>(BsonSerializer.LookupSerializer<BsonDocument>());
            serializer.Serialize(context, bsonDocument!);
        }

        /// <inheritdoc/>
        public override DynamicObject Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var serializer = new BsonValueCSharpNullSerializer<BsonDocument>(BsonSerializer.LookupSerializer<BsonDocument>());
            var document = serializer.Deserialize(context, args);
            var bsonDocument = document.ToBsonDocument();
            if (bsonDocument == null)
                return null!;
            var result = bsonDocument.ToJson(new() { OutputMode = JsonOutputMode.RelaxedExtendedJson });
            var expando = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Dynamic.ExpandoObject>(result)!;
            return DynamicObject.FromObject(expando)!;
        }

    }

}
