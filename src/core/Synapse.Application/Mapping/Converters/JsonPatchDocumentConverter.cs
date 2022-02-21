using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json.Linq;

namespace Synapse.Application.Mapping.Converters
{

    internal class JsonPatchDocumentConverter<T>
       : ITypeConverter<JsonPatchDocument, JsonPatchDocument<T>>
       where T : class
    {

        JsonPatchDocument<T> ITypeConverter<JsonPatchDocument, JsonPatchDocument<T>>.Convert(JsonPatchDocument source, JsonPatchDocument<T> destination, ResolutionContext context)
        {
            try
            {
                return JToken.FromObject(source)!.ToObject<JsonPatchDocument<T>>()!;
            }
            catch
            {
                throw;
            }

        }

    }

}
