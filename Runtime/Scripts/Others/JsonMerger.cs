using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

public static class JsonMerger
{
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
        return mergedObj.ToObject<T>();
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
