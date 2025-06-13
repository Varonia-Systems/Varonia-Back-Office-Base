using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System;

[CustomEditor(typeof(AddonsLoader))]
public class AddonsLoaderEditor : Editor
{
    SerializedProperty addons;

    private void OnEnable()
    {
        addons = serializedObject.FindProperty("addons");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Addons", EditorStyles.boldLabel);

        for (int i = 0; i < addons.arraySize; i++)
        {
            var element = addons.GetArrayElementAtIndex(i);
            var prefabProp = element.FindPropertyRelative("prefab");
            var configProp = element.FindPropertyRelative("config");

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(prefabProp);
            if (GUILayout.Button("-", GUILayout.Width(25)))
            {
                addons.DeleteArrayElementAtIndex(i);
                break;
            }
            EditorGUILayout.EndHorizontal();

            GameObject prefab = prefabProp.objectReferenceValue as GameObject;
            if (prefab != null)
            {
                AddonMarker marker = prefab.GetComponent<AddonMarker>();
                if (marker != null && marker.useCustomConfig)
                {
                    // Affiche les champs du ScriptableObject inline si existant
                    if (configProp.objectReferenceValue != null)
                    {
                        using (new EditorGUI.IndentLevelScope())
                        {
                            Editor configEditor = CreateEditor(configProp.objectReferenceValue);
                            if (configEditor != null)
                            {
                                configEditor.OnInspectorGUI();
                            }
                        }
                    }
                }
                else if (configProp.objectReferenceValue != null)
                {
                    // Nettoie si useCustomConfig est false
                    configProp.objectReferenceValue = null;
                }
            }

            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("Add All Addons"))
        {
            AddAllAddons((AddonsLoader)target);
        }

        if (GUILayout.Button("Add Addon"))
        {
            addons.InsertArrayElementAtIndex(addons.arraySize);
        }

        
        if (GUILayout.Button("🔁 Refresh"))
        {
            RefreshAddons((AddonsLoader)target);
        }
        
        serializedObject.ApplyModifiedProperties();
    }

    private void AddAllAddons(AddonsLoader loader)
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        var found = new HashSet<GameObject>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null) continue;

            var marker = prefab.GetComponent<AddonMarker>();
            if (marker == null) continue;

            if (loader.addons.Exists(a => a.prefab == prefab))
                continue;

            ScriptableObject config = null;
            if (marker.useCustomConfig)
            {
                config = CreateDefaultConfigFor(prefab);
            }

            var newAddon = new Addon
            {
                prefab = prefab,
                config = config
            };

            loader.addons.Add(newAddon);
            Debug.Log("✅ Addon ajouté : " + path, prefab);
            found.Add(prefab);
        }

        // Nettoyage des configs si useCustomConfig est false
        foreach (var addon in loader.addons)
        {
            if (addon.prefab == null) continue;
            var marker = addon.prefab.GetComponent<AddonMarker>();
            if (marker != null && !marker.useCustomConfig)
            {
                addon.config = null;
            }
        }

        if (found.Count == 0)
            Debug.Log("⚠️ Aucun nouvel addon trouvé.");

        EditorUtility.SetDirty(loader);
    }

    private static Dictionary<string, Type> _cachedScriptableTypes;

    private ScriptableObject CreateDefaultConfigFor(GameObject prefab)
    {
        if (_cachedScriptableTypes == null)
        {
            _cachedScriptableTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(ScriptableObject).IsAssignableFrom(t))
                .ToDictionary(t => t.FullName, t => t);
        }

        string cleanName = prefab.name
            .Replace("[", "")
            .Replace("]", "")
            .Trim();

        string className = ConvertToPascalCase(cleanName) + "Settings";

        if (!_cachedScriptableTypes.TryGetValue(className, out Type type))
        {
            Debug.LogWarning($"❌ Aucun ScriptableObject nommé {className} trouvé. Aucun fichier créé pour {prefab.name}.");
            return null;
        }

        ScriptableObject config = ScriptableObject.CreateInstance(type);

        string folder = "Assets/AddonConfigs/";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets", "AddonConfigs");

        string assetPath = $"{folder}{className}.asset";
        AssetDatabase.CreateAsset(config, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return config;
    }

    private string ConvertToPascalCase(string input)
    {
        string[] parts = input.Split(new[] { ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < parts.Length; i++)
        {
            parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1).ToLower();
        }
        return string.Join("", parts);
    }
    
    
    private void RefreshAddons(AddonsLoader loader)
    {
        bool modified = false;

        foreach (var addon in loader.addons)
        {
            if (addon.prefab == null) continue;

            var marker = addon.prefab.GetComponent<AddonMarker>();
            if (marker == null)
            {
                Debug.LogWarning($"❌ {addon.prefab.name} n'a plus de AddonMarker.");
                continue;
            }

            // Cas : useCustomConfig désactivé => on enlève la config
            if (!marker.useCustomConfig && addon.config != null)
            {
                addon.config = null;
                modified = true;
                Debug.Log($"🧹 Config supprimée pour {addon.prefab.name}");
            }

            // Cas : useCustomConfig activé mais pas de config => on peut en créer une
            if (marker.useCustomConfig && addon.config == null)
            {
                var config = CreateDefaultConfigFor(addon.prefab);
                if (config != null)
                {
                    addon.config = config;
                    modified = true;
                    Debug.Log($"⚙️ Config créée pour {addon.prefab.name}");
                }
            }
        }

        if (modified)
        {
            EditorUtility.SetDirty(loader);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("✅ Addons rafraîchis.");
        }
        else
        {
            Debug.Log("ℹ️ Aucun changement à appliquer.");
        }
    }

}
