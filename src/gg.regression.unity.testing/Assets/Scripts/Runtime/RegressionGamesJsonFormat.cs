using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RegressionGames.Unity.Recording;

namespace RegressionGames.Unity
{
    public class RegressionGamesJsonFormat
    {
        public static JsonSerializerSettings SerializerSettings = CreateJsonSerializerSettings();

        private static JsonSerializerSettings CreateJsonSerializerSettings()
        {
            var settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };
            settings.Converters.Add(new ListOfKeyValuePairsConverter<StateSnapshot>());
            return settings;
        }

        public static string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, SerializerSettings);
        }

        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, SerializerSettings);
        }
    }

    /// <summary>
    /// Serializes a <see cref="List{KeyValuePair{string, TValue}}"/> as a JSON object.
    /// </summary>
    class ListOfKeyValuePairsConverter<TValue> : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var list = (List<KeyValuePair<string, TValue>>) value;
            writer.WriteStartObject();
            foreach (var (key, val) in list)
            {
                writer.WritePropertyName(key);
                serializer.Serialize(writer, val);
            }

            writer.WriteEndObject();
        }

        public override object ReadJson(
            JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(List<KeyValuePair<string, TValue>>);

        public override bool CanRead => false;
    }
}
