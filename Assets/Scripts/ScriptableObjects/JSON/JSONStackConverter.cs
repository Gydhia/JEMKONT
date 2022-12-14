using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class JSONStackConverter : JsonConverter<Dictionary<Guid, int>>
{
    public override Dictionary<Guid, int> ReadJson(JsonReader reader, Type objectType, Dictionary<Guid, int> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        JArray jArray = JArray.Load(reader);

        List<string> keys = new List<string>();
        List<string> values = new List<string>();

        foreach (JObject obj in jArray)
        {
            string key = obj.Properties().Select(p => p.Name).ElementAt(0);
            keys.Add(key);
            values.Add(obj.GetValue(key).ToString());
        }


        Dictionary<Guid, int> result = new Dictionary<Guid, int>();

        Regex posRx = new Regex(@"/^\d+,\d+$/");
        for (int i = 0; i < keys.Count; i++)
        {
            //posRx.IsMatch(keys[i]);
            string[] positions = keys[i].Split(',');
            if (positions.Length >= 2)
            {
                
                Guid id = Guid.Parse(positions[0]);

                result.Add(id, int.Parse(values[i]));
            }
        }

        return result;
    }

    public override void WriteJson(JsonWriter writer, Dictionary<Guid, int> value, JsonSerializer serializer)
    {
        writer.WriteStartArray();

        foreach (var kvp in value)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(kvp.Value.ToString());
            writer.WriteValue(kvp.Key.ToString());

            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }
}
