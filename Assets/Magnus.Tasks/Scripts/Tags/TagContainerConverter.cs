using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    public class TagContainerConverter : JsonConverter<TagContainer>
    {
        public override void WriteJson(JsonWriter writer, TagContainer value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            foreach (var tag in value.Tags)
                writer.WriteValue(tag);
            writer.WriteEndArray();
        }

        public override TagContainer ReadJson(JsonReader reader, Type objectType, TagContainer existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartArray) return null;
            
            reader.Read(); // read StartArray

            List<string> arr = new List<string>();
            while (reader.TokenType != JsonToken.EndArray)
            {
                arr.Add(reader.Value as string);
                reader.Read();
            }
            
            // reader.Read(); // read EndArray

            return new TagContainer(arr);
        }
    }
}

