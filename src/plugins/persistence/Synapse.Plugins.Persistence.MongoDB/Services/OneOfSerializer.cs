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
using Newtonsoft.Json;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Plugins.Persistence.MongoDB.Services
{

    /// <summary>
    /// Represents a <see cref="IBsonSerializer"/> implementation used to serialize and deserialize <see cref="OneOf{T1, T2}"/> instances
    /// </summary>
    /// <typeparam name="T1">The first type option</typeparam>
    /// <typeparam name="T2">The second type option</typeparam>
    public class OneOfSerializer<T1, T2>
        : SerializerBase<OneOf<T1, T2>>
    {

        /// <inheritdoc/>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, OneOf<T1, T2> value)
        {
            var jsonDocument = JsonConvert.SerializeObject(value == null ? null : new { T1Value = value.T1Value, T2Value = value.T2Value });
            var bsonDocument = value == null ? null : BsonSerializer.Deserialize<BsonDocument>(jsonDocument);
            var serializer = new BsonValueCSharpNullSerializer<BsonDocument>(BsonSerializer.LookupSerializer<BsonDocument>());
            serializer.Serialize(context, bsonDocument!);
        }

        /// <inheritdoc/>
        public override OneOf<T1, T2> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var serializer = new BsonValueCSharpNullSerializer<BsonDocument>(BsonSerializer.LookupSerializer<BsonDocument>());
            var document = serializer.Deserialize(context, args);
            var bsonDocument = document.ToBsonDocument();
            if (bsonDocument == null)
                return null!;
            var result = (OneOf<T1, T2>)Activator.CreateInstance(typeof(OneOf<T1, T2>), true)!;
            if (bsonDocument.TryGetValue("t1Value", out var bsonValue) && bsonValue != null && bsonValue is not BsonNull)
                result.T1Value = JsonConvert.DeserializeObject<T1>(bsonValue.ToJson());
            if (bsonDocument.TryGetValue("t2Value", out bsonValue) && bsonValue != null && bsonValue is not BsonNull)
                result.T2Value = JsonConvert.DeserializeObject<T2>(bsonValue.ToJson());
            return result;
        }

    }

}
