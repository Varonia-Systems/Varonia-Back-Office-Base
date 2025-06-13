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
        }


        public override void OnInspectorGUI()
        {
            
           
            
            var rect = GUILayoutUtility.GetRect(Screen.width - 38, 300, GUI.skin.box);
            GUI.DrawTexture(rect, logo, ScaleMode.ScaleToFit);

            GUIStyle warningStyle = new GUIStyle(GUI.skin.box);
            warningStyle.normal.textColor = Color.white;
            warningStyle.normal.background = MakeTex(2, 2, new Color(0.8f, 0.2f, 0.2f));
            warningStyle.fontSize = 14;
            warningStyle.fontStyle = FontStyle.Bold;
            warningStyle.alignment = TextAnchor.MiddleCenter;
            warningStyle.wordWrap = true;

            GUILayout.Space(10);
            EditorGUILayout.TextArea("⚠️ WARNING: Please read the Back Office documentation carefully before starting the integration process!", warningStyle);
            GUILayout.Space(10);

            GUIStyle sexyButtonStyle = new GUIStyle(GUI.skin.button);
            sexyButtonStyle.fontSize = 18;
            sexyButtonStyle.fontStyle = FontStyle.Bold;
            sexyButtonStyle.normal.textColor = Color.white;
            
            
            
            sexyButtonStyle.normal.background = MakeTex(2, 2, new Color(0.1f, 0.6f, 1f)); // Bleu clair
            sexyButtonStyle.hover.background = MakeTex(2, 2, new Color(0.3f, 0.7f, 1f));  // Survol

            
            GUILayout.Space(15);
            
            var rect2 = GUILayoutUtility.GetRect(Screen.width - 38, 50, GUI.skin.box);
            if (GUI.Button(rect2,"🤔 Go Wiki For More Details",sexyButtonStyle))
            { Application.OpenURL("https://varoniasystems.notion.site/backoffice?v=1d0ff92b37a181d58dbb000c443a2030"); }

            
            GUILayout.Space(15);
            
            serializedObject.Update();
            
            SerializedProperty property = serializedObject.GetIterator();
            bool expanded = true;

            property.NextVisible(expanded); 

            while (property.NextVisible(false))
            {
                EditorGUILayout.PropertyField(property, true);
            }

            serializedObject.ApplyModifiedProperties();

        }
        
        
        Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
                pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
    
  
}
