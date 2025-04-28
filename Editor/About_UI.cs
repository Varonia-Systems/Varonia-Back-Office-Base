using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VaroniaBackOffice;


public class Version_
{
    public string Date;
    public string VersionNumber;
}


public class About_UI : EditorWindow
{


    public static Version_ GetVersion()
    {
        Version_ V = new Version_();

        var ms = MonoScript.FromScriptableObject(new About_UI());
        var path = AssetDatabase.GetAssetPath(ms);
        path = Path.GetDirectoryName(path);
        path = path.Substring(0, path.Length - "Editor".Length) + "";

        if (File.Exists(path + "/version"))
        {
            using (StreamReader sr = new StreamReader(path + "/version"))
            {
                V = JsonConvert.DeserializeObject<Version_>(sr.ReadToEnd());
            }
        }

        return V;
    }





    public static Version_ version;

    [MenuItem("Varonia/About Varonia Back Office", false, 20)]
    public static void ShowWindow()
    {


       version = GetVersion();

        var B = EditorWindow.GetWindow(typeof(About_UI), false, "About ...");


        B.position = new Rect(B.position.x, B.position.y, 500, 300);

    }





    Texture logo;





    string GetResourcePath()
    {
        var ms = MonoScript.FromScriptableObject(this);
        var path = AssetDatabase.GetAssetPath(ms);
        path = Path.GetDirectoryName(path);
        return path.Substring(0, path.Length - "Editor".Length) + "";
    }


    void OnEnable()
    {
        logo = AssetDatabase.LoadAssetAtPath<Texture2D>(GetResourcePath() + "BackOfficeLogo.png");
    }







    private void OnGUI()
    {


        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.richText = true;

        Rect r = new Rect(0, 0, 100, 20);  // size
        r.center = new Vector2((position.width / 2) - 100, r.height / 2);



        GUI.Label(r, "<b>Varonia Back Office Version Number : </b><color=Orange><b>" + version.VersionNumber + "</b></color>", style);
        GUI.Label(r, "\n<b>Varonia Back Office Version Date : </b><color=yellow><b>" + version.Date + "</b></color>", style);
        GUI.Label(r, "", style);

        var rect = GUILayoutUtility.GetRect(500, 300, GUI.skin.box);
        GUI.DrawTexture(rect, logo, ScaleMode.ScaleToFit);
    }


}
