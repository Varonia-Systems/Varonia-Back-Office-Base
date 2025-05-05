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

        ////  [MenuItem("Varonia/Project Settings")]
        //  public static void ShowWindow()
        //  {
        //      EditorWindow.GetWindow(WindowType, false, "Project Settings");
        //  }

        void OnInspectorUpdate()
        {
            Repaint();
        }


        protected void OnDisable()
        {
     

        }


        public virtual void OnGUI()
        {


    
        }


  

    }

}