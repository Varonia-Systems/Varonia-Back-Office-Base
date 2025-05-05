using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace VaroniaBackOffice
{
    [CustomEditor(typeof(VaroniaGlobal))]
    public class Varonia_Editor : Editor
    {
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

            //if (!Application.isPlaying)
            //{
            //    using (StreamWriter sw = new StreamWriter(Application.streamingAssetsPath + "/VBO_Version.txt", false))
            //    {
            //        var V = About_UI.GetVersion();
            //        sw.Write(V.Date + " " + V.VersionNumber);
            //    }
            //}
        }


        public override void OnInspectorGUI()
        {
            var rect = GUILayoutUtility.GetRect(Screen.width - 38, 300, GUI.skin.box);
            GUI.DrawTexture(rect, logo, ScaleMode.ScaleToFit);
            //GUILayout.Label("\nIt is important to comment out two lines of code in the steamVR script 'SteamVR_Render' \n"
            //                 + "in the 'OnInputFocus' function comment out 'Time.timeScale = 0.0f' \nAnd "
            //                 + "'SteamVR_Camera.sceneResolutionScale = 0.5f;'"
            //                 + "\n\nYour Main Camera have to 'MainCamera' Tag !  "
            //                 + "");

            var rect2 = GUILayoutUtility.GetRect(Screen.width - 38, 50, GUI.skin.box);
            if (GUI.Button(rect2, "Go Wiki For More Details"))
            { Application.OpenURL("https://varoniasystems.notion.site/backoffice?v=1d0ff92b37a181d58dbb000c443a2030"); }

            DrawDefaultInspector();

            var rect3 = GUILayoutUtility.GetRect(Screen.width - 38, 50, GUI.skin.box);
        }
    }
}
