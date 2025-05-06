using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;

class BuildProcessor_VBO : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(BuildReport report)
    {
        ListRequest listRequest;
        Debug.Log("Build Begin...");

        listRequest = Client.List();

        string V_ = "null", D_ = "null";

        while (!listRequest.IsCompleted)
        { }


        foreach (var package in listRequest.Result)
        {
            if (package.name.StartsWith("com.varonia"))
            {
                if (package.name == "com.varonia.vbobase")
                {
                    V_ = package.version;
                    D_ = System.IO.File.GetLastWriteTime(package.resolvedPath + "/package.json").ToString("dd/MM/yyyy");
                }
            }
        }

        using (StreamWriter sw = new StreamWriter(Application.streamingAssetsPath + "/VBO_Version.txt", false))
        {
            sw.Write(V_ + " " + D_);
        }

    }
}

public class VaroniaAboutWindow : EditorWindow
{
    private List<UnityEditor.PackageManager.PackageInfo> varoniaPackages = new();
    private ListRequest listRequest;
    private string version = "inconnue";
    private string versionDate = "inconnue";

    private Texture2D logo;

    [MenuItem("Varonia/About")]
    public static void ShowWindow()
    {
        var window = GetWindow<VaroniaAboutWindow>("Varonia Back Office");
        window.minSize = new Vector2(450, 400);
        window.FetchPackages();
    }

    private void FetchPackages()
    {
        listRequest = Client.List();
        EditorApplication.update += Progress;
    }

    private void Progress()
    {
        if (listRequest.IsCompleted)
        {
            if (listRequest.Status == StatusCode.Success)
            {
                foreach (var package in listRequest.Result)
                {
                    if (package.name.StartsWith("com.varonia"))
                    {
                        if (package.name == "com.varonia.vbobase")
                        {
                            version = package.version;
                            versionDate = System.IO.File.GetLastWriteTime(package.resolvedPath + "/package.json").ToString("dd/MM/yyyy");
                        }
                        else
                        {
                            varoniaPackages.Add(package); // uniquement les add-ons //
                        }
                    }
                }
            }

            logo = AssetDatabase.LoadAssetAtPath<Texture2D>(GetResourcePath() + "BackOfficeLogo.png"); // charge le logo si présent
            EditorApplication.update -= Progress;
            Repaint();
        }
    }

    private void OnGUI()
    {
        GUILayout.Space(10);

        // Logo
        if (logo != null)
        {
            float logoWidth = Mathf.Min(position.width - 40, logo.width);
            float logoHeight = logo.height * (logoWidth / logo.width);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(logo, GUILayout.Width(logoWidth), GUILayout.Height(logoHeight));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(15);
        GUILayout.Label("<b>Varonia Back Office</b>", new GUIStyle(EditorStyles.label) { richText = true, fontSize = 14, alignment = TextAnchor.MiddleCenter });
        GUILayout.Label($"Version : <b>{version}</b>", new GUIStyle(EditorStyles.label) { richText = true, alignment = TextAnchor.MiddleCenter });
        GUILayout.Label($"Date : <b>{versionDate}</b>", new GUIStyle(EditorStyles.label) { richText = true, alignment = TextAnchor.MiddleCenter });

        GUILayout.Space(20);
        GUILayout.Label("📦 Add-ons détectés :", EditorStyles.boldLabel);

        if (varoniaPackages.Count > 0)
        {
            foreach (var pkg in varoniaPackages)
            {
                GUILayout.Label($"• {pkg.displayName} ({pkg.version})");
            }
        }
        else
        {
            GUILayout.Label("Aucun add-on Varonia détecté.");
        }

        GUILayout.FlexibleSpace();
        GUILayout.Label("Varonia Systems © 2025", EditorStyles.centeredGreyMiniLabel);
    }


    string GetResourcePath()
    {
        var ms = MonoScript.FromScriptableObject(this);
        var path = AssetDatabase.GetAssetPath(ms);
        path = Path.GetDirectoryName(path);
        return path.Substring(0, path.Length - "Editor".Length) + "";
    }



}
