using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace VaroniaBackOffice
{

    public class VaroniaInfoUI : EditorWindow
    {

        public string GameId = "0";


        public static Type WindowType = typeof(VaroniaInfoUI);

        [MenuItem("Varonia/Project Settings")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(WindowType, false, "Project Settings");
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }



        protected void OnEnable()
        {
            var data = EditorPrefs.GetString(Application.productName + "config", JsonUtility.ToJson(this, false));
            JsonUtility.FromJsonOverwrite(data, this);

            if (!File.Exists(Application.streamingAssetsPath + "/GameID.txt"))
            {
                using (StreamWriter sw = new StreamWriter(Application.streamingAssetsPath + "/GameID.txt"))
                {
                    sw.Write("9999");
                    GameId = "9999";
                }
            }
            else
            {
                using (StreamReader sr = new StreamReader(Application.streamingAssetsPath + "/GameID.txt"))
                {
                    GameId = sr.ReadToEnd();
                }
            }
        }


        protected void OnDisable()
        {
            Save();

        }


     public virtual void OnGUI()
        {


            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.richText = true;
            GUILayout.BeginHorizontal("box");
            GUILayout.FlexibleSpace();
            GUILayout.Label("<color=red><b>~~ Project Settings ~~</b></color>", style);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Label("GameId : ");
            GameId = GUILayout.TextField(GameId);

            GUILayout.Space(25);
            if (GUILayout.Button("SAVE"))
            {
                Save();
            }
        }


      public void Save()
        {

            if (!Directory.Exists(Application.streamingAssetsPath))
                Directory.CreateDirectory(Application.streamingAssetsPath);

            try
            {

                using (StreamWriter sw = new StreamWriter(Application.streamingAssetsPath + "/GameID.txt"))
                {
                    sw.Write(GameId);
                }
            }
            catch (Exception)
            {

            }

            var data = JsonUtility.ToJson(this, false);
            EditorPrefs.SetString(Application.productName + "config", data);
        }

    }

}