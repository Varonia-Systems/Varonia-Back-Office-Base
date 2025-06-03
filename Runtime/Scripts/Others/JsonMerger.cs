using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

// Converter personnalisé pour gérer les erreurs d'enum
public class SafeEnumConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType.IsEnum || (Nullable.GetUnderlyingType(objectType)?.IsEnum == true);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            if (Nullable.GetUnderlyingType(objectType) != null)
                return null;
            return -1; // ou Enum.ToObject(objectType, -1) si vous préférez
        }

        Type enumType = Nullable.GetUnderlyingType(objectType) ?? objectType;

        try
        {
            if (reader.TokenType == JsonToken.String)
            {
                string enumText = reader.Value.ToString();
                if (Enum.IsDefined(enumType, enumText))
                {
                    return Enum.Parse(enumType, enumText, true);
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"Enum value '{enumText}' not found in {enumType.Name}. Using -1 instead.");
                    return Enum.ToObject(enumType, -1);
                }
            }
            else if (reader.TokenType == JsonToken.Integer)
            {
                int enumValue = Convert.ToInt32(reader.Value);
                if (Enum.IsDefined(enumType, enumValue))
                {
                    return Enum.ToObject(enumType, enumValue);
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"Enum value '{enumValue}' not found in {enumType.Name}. Using -1 instead.");
                    return Enum.ToObject(enumType, -1);
                }
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"Error parsing enum for {enumType.Name}: {ex.Message}. Using -1 instead.");
        }

        return Enum.ToObject(enumType, -1);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
        }
        else
        {
            writer.WriteValue(value.ToString());
        }
    }
}

public static class JsonMerger
{
    public static readonly JsonSerializerSettings SafeSettings = new JsonSerializerSettings
    {
        Converters = { new SafeEnumConverter() },
        Error = (sender, args) =>
        {
            UnityEngine.Debug.LogWarning($"JSON deserialization error: {args.ErrorContext.Error.Message}");
            args.ErrorContext.Handled = true;
        }
    };

    public static T MergeJson<T>(string baseJson, string overrideJson)
    {
        if (string.IsNullOrWhiteSpace(baseJson))
            throw new ArgumentException("Base JSON cannot be null or empty");

        JObject baseObj = JObject.Parse(baseJson);
        JObject overrideObj = null;

        if (!string.IsNullOrWhiteSpace(overrideJson))
        {
            try
            {
                overrideObj = JObject.Parse(overrideJson);
            }
            catch (JsonReaderException ex)
            {
                UnityEngine.Debug.LogWarning("Override JSON is invalid. Using base config only. " + ex.Message);
            }
        }

        JObject mergedObj = overrideObj != null ? MergeJObjects(baseObj, overrideObj) : baseObj;

        // Utilisation des settings sécurisés pour la désérialisation
        return JsonConvert.DeserializeObject<T>(mergedObj.ToString(), SafeSettings);
    }

    private static JObject MergeJObjects(JObject baseObj, JObject overrideObj)
    {
        JObject result = new JObject(baseObj); // start with base

        foreach (var prop in overrideObj.Properties())
        {
            if (prop.Value.Type == JTokenType.Object)
            {
                var baseSub = result[prop.Name] as JObject ?? new JObject();
                var overrideSub = prop.Value as JObject;
                result[prop.Name] = MergeJObjects(baseSub, overrideSub);
            }
            else if (prop.Value.Type != JTokenType.Null && prop.Value.Type != JTokenType.Undefined)
            {
                result[prop.Name] = prop.Value;
            }
        }

        return result;
    }
}