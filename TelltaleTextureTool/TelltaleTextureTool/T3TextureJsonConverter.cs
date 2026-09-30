using System.Collections.Generic;
using System.Text.Json;
using TelltaleToolKit.Serialization.Binary;
using TelltaleToolKit.T3Types.Textures;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace TelltaleTextureTool;

public static class T3TextureJsonConverter
{
    public static string ToJson(T3Texture tex, MetaStreamConfiguration configuration)
    {
        List<object> jsonObjects = [configuration, tex];

        return JsonSerializer.Serialize(jsonObjects, JsonSerializerOptions.Default);
    }
    
    public static (T3Texture tex, MetaStreamConfiguration configuration) FromJson(string json)
    {
        // Deserialize as a list of JsonElements, then convert to the correct types
        var doc = JsonSerializer.Deserialize<List<JsonElement>>(json, JsonSerializerOptions.Default);

        if (doc is not { Count: 2 })
            throw new JsonException("Expected a list with two elements: MetaStreamConfiguration and T3Texture.");

        var configuration = doc[0].Deserialize<MetaStreamConfiguration>(JsonSerializerOptions.Default);
        var tex = doc[1].Deserialize<T3Texture>(JsonSerializerOptions.Default);

        if (configuration == null || tex == null)
            throw new JsonException("Failed to deserialize MetaStreamConfiguration or T3Texture.");

        return (tex, configuration);
    }
}