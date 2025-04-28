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
using System.Reflection;
using Newtonsoft.Json;
using VaroniaBackOffice;


namespace VaroniaBackOffice
{

    public class VaroniaConfigUI : EditorWindow
    {

        public VaroniaConfig VC;
        public System.Object C;




        [MenuItem("Varonia/Config")]

        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(VaroniaConfigUI), false, "Varonia Config");

        }

        void OnInspectorUpdate()
        {
            Repaint();
        }








        protected void OnEnable()
        {

            if (File.Exists(Application.persistentDataPath.Replace(Application.companyName + "/" + Application.productName, "varonia") + "/GlobalConfig.json"))
            {
                using (StreamReader sr = new StreamReader(Application.persistentDataPath.Replace(Application.companyName + "/" + Application.productName, "varonia") + "/GlobalConfig.json"))
                {
                    VC = JsonConvert.DeserializeObject<VaroniaConfig>(sr.ReadToEnd());
                }
            }
            else
            {
                VC = new VaroniaConfig();
            }


#if Game_Config
            Assembly assem = typeof(VaroniaBackOffice.GameConfig).Assembly;
            C = assem.GetType("VaroniaBackOffice.GameConfig");


            if (File.Exists(Application.persistentDataPath + "/Config.json"))
            {
                using (StreamReader sr = new StreamReader(Application.persistentDataPath + "/Config.json"))
                {
                    C = JsonConvert.DeserializeObject(sr.ReadToEnd(), assem.GetType("VaroniaBackOffice.GameConfig"));
                }
            }
            else
            {
                C = new GameConfig();
            }
#endif


        }


        protected void OnDisable()
        {
            var data = JsonUtility.ToJson(this, false);
        }



        public Vector2 scrollPosition = Vector2.zero;
        void OnGUI()
        {
            var A = GetWindow<VaroniaConfigUI>().position;
            scrollPosition = GUI.BeginScrollView(new Rect(0, 0, A.width, A.height), scrollPosition, new Rect(0, 0, 500, 1800));

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.richText = true;

            GUILayout.BeginHorizontal("box");
            GUILayout.BeginVertical("box");
            GUILayout.Label("<color=red><b>~~ GlobalConfig ~~</b></color>", style); // GLOBALCONFIG
            GUILayout.Space(28);




            BindingFlags bindingFlags = BindingFlags.Public |
                           BindingFlags.NonPublic |
                           BindingFlags.Instance |
                           BindingFlags.Static;

            int index = 0;



            foreach (FieldInfo field in typeof(VaroniaConfig).GetFields(bindingFlags))
            {
                GUILayout.Label("<color=green><b>" + field.Name + "</b></color>", style);

                if (field.FieldType.ToString().Contains("String") || field.FieldType.ToString().Contains("Int") || field.FieldType.ToString().Contains("float") || field.FieldType.ToString().Contains("Single"))
                    TXT(field);
                else
                if (field.FieldType.ToString().Contains("Bool"))
                    TOOGLE(field);
                else if (!field.FieldType.ToString().Contains("List") && !field.FieldType.ToString().Contains("Vector"))
                    DrawToolStrip(index, field);
                else
                    GUILayout.Label("<color=orange>No Read</color>", style);

                index++;

            }



            //if (GUILayout.Button("Save GlobalConfig", EditorStyles.miniButton))
            //{
            //    using (StreamWriter sw = new StreamWriter(Application.persistentDataPath.Replace("/" + Application.productName, "") + "/GlobalConfig.json"))
            //    {

            //        sw.Write(JsonPrettify(JsonConvert.SerializeObject(VC)));

            //    }
            //}
            GUILayout.EndVertical();
            //GUILayout.EndHorizontal();


            // GUILayout.BeginHorizontal("box");
            GUILayout.BeginVertical("box");
#if Game_Config
            GUILayout.Label("<color=red><b>~~ Config ~~</b></color>", style); //CONFIG
            GUILayout.Space(28);
            int index_2 = 0;

            Assembly assem = typeof(VaroniaBackOffice.GameConfig).Assembly;
            var toto = assem.GetType("VaroniaBackOffice.GameConfig");



            foreach (FieldInfo field in toto.GetFields(bindingFlags))
            {
                GUILayout.Label("<color=green><b>" + field.Name + "</b></color>", style);

                if ((field.FieldType.ToString().Contains("String") || field.FieldType.ToString().Contains("Int") || field.FieldType.ToString().Contains("float") || field.FieldType.ToString().Contains("Single")) && !field.FieldType.ToString().Contains("List"))
                    TXT_2(field);
                else
                if (field.FieldType.ToString().Contains("Bool"))
                    TOOGLE_2(field);
                else if (!field.FieldType.ToString().Contains("List"))
                    DrawToolStrip_2(index_2, field);
                else
                    GUILayout.Label("<color=orange>No Read</color>", style);

                index_2++;


            }
#endif


            //if (GUILayout.Button("Save Config", EditorStyles.miniButton))
            //{
            //    using (StreamWriter sw = new StreamWriter(Application.persistentDataPath + "/Config.json"))
            //    {

            //        sw.Write(JsonPrettify(JsonConvert.SerializeObject(C)));

            //    }
            //}



            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(12);
            if (GUILayout.Button("Save", GUILayout.MinHeight(60)))
            {
                using (StreamWriter sw = new StreamWriter(Application.persistentDataPath.Replace(Application.companyName + "/" + Application.productName, "varonia") + "/GlobalConfig.json"))
                {
                    sw.Write(JsonPrettify(JsonConvert.SerializeObject(VC)));
                }


                using (StreamWriter sw = new StreamWriter(Application.persistentDataPath + "/Config.json"))
                {
                    sw.Write(JsonPrettify(JsonConvert.SerializeObject(C)));
                }
            }


            GUI.EndScrollView();







        }


        void TOOGLE(FieldInfo field)
        {
            try
            {
                var prop = VC.GetType().GetField(field.Name);
            prop.SetValue(VC, GUILayout.Toggle((bool)prop.GetValue(VC), ""));
            }
            catch (Exception)
            {


            }
        }

        void TOOGLE_2(FieldInfo field)
        {
            try
            {
                var prop = C.GetType().GetField(field.Name);
            prop.SetValue(C, GUILayout.Toggle((bool)prop.GetValue(C), ""));
            }
            catch (Exception)
            {


            }
        }


        void TXT(FieldInfo field)
        {
            try
            {
                string T = GUILayout.TextField(field.GetValue(VC).ToString());
                if (field.FieldType.ToString().Contains("String"))
                    field.SetValue(VC, T);
                if (field.FieldType.ToString().Contains("Int"))
                    field.SetValue(VC, int.Parse(T));
                if (field.FieldType.ToString().Contains("float"))
                    field.SetValue(VC, float.Parse(T));
                if (field.FieldType.ToString().Contains("Single"))
                    field.SetValue(VC, Single.Parse(T));
            }
            catch (Exception)
            {


            }
        }

        void TXT_2(FieldInfo field)
        {
            try
            {
                string T = GUILayout.TextField(field.GetValue(C).ToString());
            if (field.FieldType.ToString().Contains("String"))
                field.SetValue(C, T);
            if (field.FieldType.ToString().Contains("Int"))
                field.SetValue(C, int.Parse(T));
            if (field.FieldType.ToString().Contains("float"))
                field.SetValue(C, float.Parse(T));
            if (field.FieldType.ToString().Contains("Single"))
                field.SetValue(C, Single.Parse(T));
            }
            catch (Exception)
            {


            }
        }




        private FieldInfo tempfield;
        void DrawToolStrip(int index, FieldInfo field)
        {
            tempfield = field;
            if (GUILayout.Button(field.GetValue(VC).ToString() + " (" + (int)field.GetValue(VC) + ")", EditorStyles.toolbarDropDown))
            {
                GenericMenu toolsMenu = new GenericMenu();
                var tt = Enum.GetNames(field.FieldType);

                foreach (var item in tt)
                {
                    toolsMenu.AddItem(new GUIContent(item), false, Select, item);
                }
                toolsMenu.DropDown(new Rect(10, (36.2f * index) + 82.9f, 0, 16));
                EditorGUIUtility.ExitGUI();
            }
        }
        void Select(object obj)
        {

            var uu = tempfield.FieldType;
            tempfield.SetValue(VC, Enum.Parse(uu, (string)obj));
        }



        void DrawToolStrip_2(int index, FieldInfo field)
        {
            tempfield = field;
            if (GUILayout.Button(field.GetValue(C).ToString() + " (" + (int)field.GetValue(C) + ")", EditorStyles.toolbarDropDown))
            {
                GenericMenu toolsMenu = new GenericMenu();
                var tt = Enum.GetNames(field.FieldType);

                foreach (var item in tt)
                {
                    toolsMenu.AddItem(new GUIContent(item), false, Select_2, item);
                }
                toolsMenu.DropDown(new Rect(position.width - 200, (36.2f * index) + 82.9f, 0, 16));
                EditorGUIUtility.ExitGUI();
            }
        }
        void Select_2(object obj)
        {
            var uu = tempfield.FieldType;
            tempfield.SetValue(C, Enum.Parse(uu, (string)obj));
        }






        public static string JsonPrettify(string json)
        {
            using (var stringReader = new StringReader(json))
            using (var stringWriter = new StringWriter())
            {
                var jsonReader = new JsonTextReader(stringReader);
                var jsonWriter = new JsonTextWriter(stringWriter) { Formatting = Formatting.Indented };
                jsonWriter.WriteToken(jsonReader);
                return stringWriter.ToString();
            }
        }
    }
}