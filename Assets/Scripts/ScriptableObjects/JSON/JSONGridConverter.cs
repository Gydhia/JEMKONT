using DownBelow.Managers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class JSONGridConverter : JsonConverter<Dictionary<GridPosition, Guid>>
{
    public override Dictionary<GridPosition, Guid> ReadJson(JsonReader reader, Type objectType, Dictionary<GridPosition, Guid> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        JArray jArray = JArray.Load(reader);

        List<string> keys = new List<string>();
        List<string> values = new List<string>();

        foreach (JObject obj in jArray) {
            string key = obj.Properties().Select(p => p.Name).ElementAt(0);
            keys.Add(key);
            values.Add(obj.GetValue(key).ToString());
        }
            

        Dictionary<GridPosition, Guid> result = new Dictionary<GridPosition, Guid>();

        Regex posRx = new Regex(@"/^\d+,\d+$/");
        for (int i = 0; i < keys.Count; i++)
        {
            //posRx.IsMatch(keys[i]);
            string[] positions = keys[i].Split(',');
            if (positions.Length >= 2)
            {
                GridPosition pos = new GridPosition(int.Parse(positions[0]), int.Parse(positions[1]));

                result.Add(pos, Guid.Parse(values[i]));
            }
        }

        return result;
    }

    public override void WriteJson(JsonWriter writer, Dictionary<GridPosition, Guid> value, JsonSerializer serializer)
    {
        writer.WriteStartArray();
        
        foreach (var kvp in value)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(kvp.Key.longitude.ToString() + "," + kvp.Key.latitude.ToString());
            writer.WriteValue(kvp.Value);

            writer.WriteEndObject();
        }
        
        writer.WriteEndArray();
    }
}
