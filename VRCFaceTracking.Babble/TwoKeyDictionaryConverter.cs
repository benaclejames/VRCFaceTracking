using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VRCFaceTracking.Babble;

public class TwoKeyDictionaryConverter<TKey1, TKey2, TValue> : JsonConverter<TwoKeyDictionary<TKey1, TKey2, TValue>>
{
    public override void WriteJson(JsonWriter writer, TwoKeyDictionary<TKey1, TKey2, TValue> tkd, JsonSerializer serializer)
    {
        var entries = new List<JObject>();

        for (int i = 0; i < tkd.Count; i++)
        {
            var elem = tkd.ElementAt(i);
            var entry = new JObject(
                new JProperty("unifiedExpression", elem.Item1!.ToString()),
                new JProperty("oscAddress", elem.Item2),
                new JProperty("weight", elem.Item3)
            );
            entries.Add(entry);
        }

        serializer.Serialize(writer, entries);
    }

    public override TwoKeyDictionary<TKey1, TKey2, TValue> ReadJson(JsonReader reader, Type objectType, TwoKeyDictionary<TKey1, TKey2, TValue> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var entries = serializer.Deserialize<List<JObject>>(reader);

        if (entries == null)
        {
            return null;
        }

        var dictionary = new TwoKeyDictionary<TKey1, TKey2, TValue>();

        foreach (var entry in entries)
        {
            var item1 = (TKey1)Enum.Parse(typeof(TKey1), entry["unifiedExpression"]!.Value<string>()!);
            var item2 = entry["oscAddress"]!.Value<TKey2>();
            var item3 = entry["weight"]!.Value<TValue>();

            dictionary.Add(item1!, item2!, item3!);
        }

        return dictionary;
    }

    public override bool CanRead => true;

    public override bool CanWrite => true;
}
