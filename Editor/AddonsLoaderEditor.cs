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
        if(Application.isPlaying)
        return;
        
        serializedObject.Update();
        
      
        
        GUI.backgroundColor = new Color(0.7f, 0.7f, 1f); 
        EditorGUILayout.LabelField("Addons", EditorStyles.boldLabel);

        for (int i = 0; i < addons.arraySize; i++)
        {
            var element = addons.GetArrayElementAtIndex(i);
            var prefabProp = element.FindPropertyRelative("prefab");
            var configProp = element.FindPropertyRelative("config");

            GameObject prefab_ = prefabProp.objectReferenceValue as GameObject;
            bool isVariant = prefab_ != null && PrefabUtility.GetPrefabAssetType(prefab_) == PrefabAssetType.Variant;
            
            Color originalColor = GUI.backgroundColor;
            if (isVariant)
            {
                GUI.backgroundColor = new Color(1f, 0.5f, 0.1f); 
               
            }
            
            
            EditorGUILayout.BeginVertical("box");
            if (isVariant) EditorGUILayout.LabelField("[VARIANT]", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
        
            EditorGUILayout.PropertyField(prefabProp);
            
            GameObject selected = prefabProp.objectReferenceValue as GameObject;
            if (selected != null && selected.GetComponent<AddonMarker>() == null)
            {
                EditorGUILayout.HelpBox("Missing AddonMarker", MessageType.Error);
            }
            
            
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button("-", GUILayout.Width(25)))
            {
                addons.DeleteArrayElementAtIndex(i);
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
                break;
            }

            EditorGUILayout.EndHorizontal();

            GameObject prefab = prefabProp.objectReferenceValue as GameObject;
            if (prefab != null)
            {
                AddonMarker marker = prefab.GetComponent<AddonMarker>();
                if (marker != null && marker.useCustomConfig)
                {
                    if (configProp.objectReferenceValue != null)
                    {
                        using (new EditorGUI.IndentLevelScope())
                        {
                            Editor configEditor = CreateEditor(configProp.objectReferenceValue);
                            configEditor?.OnInspectorGUI();
                        }
                    }
                }
                else
                {
                    if (configProp.objectReferenceValue != null)
                    {
                        configProp.objectReferenceValue = null;
                    }
                }
            }
            GUI.backgroundColor = originalColor;
            EditorGUILayout.EndVertical();
        }

        GUILayout.Space(40);
        
        if (!Application.isPlaying)
        {
            GUI.backgroundColor = new Color(0.3f, 1f, 0.5f); 
            if (GUILayout.Button("Add All Addons"))
            {
                AddAllAddons((AddonsLoader)target);
            }

            GUI.backgroundColor = new Color(1f, 0.7f, 0.5f); 
            if (GUILayout.Button("Add Addon"))
            {
                serializedObject.Update();

                addons.arraySize++;
                SerializedProperty newElement = addons.GetArrayElementAtIndex(addons.arraySize - 1);

                newElement.FindPropertyRelative("prefab").objectReferenceValue = null;
                newElement.FindPropertyRelative("config").objectReferenceValue = null;

                serializedObject.ApplyModifiedProperties();

                // Marque bien le script comme modifié
                EditorUtility.SetDirty(target);
            }

            GUI.backgroundColor = new Color(0f, 0.5f, 0.5f); 
            if (GUILayout.Button("🔁 Refresh"))
            {
                RefreshAddons((AddonsLoader)target);
            }
        }
        else
        {
            GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f); 
            if (GUILayout.Button("Force Load Addon"))
            {
                AddonsLoader myScript = (AddonsLoader)target;

                // Appelle la méthode avec un paramètre
                myScript.LoadAddons();

            }
        }

        GUILayout.Space(15);
            
        serializedObject.Update();
            
        GUI.backgroundColor = new Color(1f, 1f, 1f); 
        
        SerializedProperty loadOnStartProperty = serializedObject.FindProperty("loadOnStart");
        EditorGUILayout.PropertyField(loadOnStartProperty, true);


        serializedObject.ApplyModifiedProperties();

        
        
    }

   private void AddAllAddons(AddonsLoader loader)
{
    string[] guids = AssetDatabase.FindAssets("t:Prefab");
    var found = new HashSet<GameObject>();

    // Étape 1 : récupérer tous les variants
    var variantSources = new HashSet<GameObject>();

    foreach (string guid in guids)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) continue;

        if (PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.Variant)
        {
            GameObject basePrefab = PrefabUtility.GetCorrespondingObjectFromSource(prefab);
            if (basePrefab != null)
                variantSources.Add(basePrefab);
        }
    }

    // Étape 2 : parcourir tous les prefabs à ajouter
    foreach (string guid in guids)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) continue;

        var marker = prefab.GetComponent<AddonMarker>();
        if (marker == null) continue;

        // ⛔️ Skip si c'est un original et qu’un variant existe
        bool isVariant = PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.Variant;
        if (!isVariant && variantSources.Contains(prefab))
        {
            Debug.Log($"❌ Ignoré : {prefab.name} (un variant existe)");
            continue;
        }

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

    // Nettoyage config si non nécessaire
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
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); } catch { return Array.Empty<Type>(); }
                })
                .Where(t => typeof(ScriptableObject).IsAssignableFrom(t))
                .GroupBy(t => t.FullName)
                .ToDictionary(g => g.Key, g => g.First());
        }

        string cleanName = prefab.name
            .Split(']')[0]
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

        // Étape 1 : trouver les prefabs variants déjà présents
        var variantSources = new HashSet<GameObject>();
        foreach (var addon in loader.addons)
        {
            if (addon.prefab == null) continue;

            if (PrefabUtility.GetPrefabAssetType(addon.prefab) == PrefabAssetType.Variant)
            {
                GameObject basePrefab = PrefabUtility.GetCorrespondingObjectFromSource(addon.prefab);
                if (basePrefab != null)
                    variantSources.Add(basePrefab);
            }
        }

        // Étape 2 : supprimer les originaux si un variant existe déjà
        for (int i = loader.addons.Count - 1; i >= 0; i--)
        {
            var addon = loader.addons[i];
            if (addon.prefab == null) continue;

            bool isVariant = PrefabUtility.GetPrefabAssetType(addon.prefab) == PrefabAssetType.Variant;
            if (!isVariant && variantSources.Contains(addon.prefab))
            {
                Debug.Log($"🗑️ Supprimé : {addon.prefab.name} (variant présent)");
                loader.addons.RemoveAt(i);
                modified = true;
                continue;
            }

            // Étape 3 : gérer la config
            var marker = addon.prefab.GetComponent<AddonMarker>();
            if (marker == null)
            {
                Debug.LogWarning($"❌ {addon.prefab.name} n'a plus de AddonMarker.");
                continue;
            }

            if (!marker.useCustomConfig && addon.config != null)
            {
                addon.config = null;
                modified = true;
                Debug.Log($"🧹 Config supprimée pour {addon.prefab.name}");
            }

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
