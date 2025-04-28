using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VaroniaBackOffice;

public class AppData_UI : EditorWindow
{
    [MenuItem("Varonia/Goto GlobalConfig folder",false,30)]
    public static void ShowWindow()
    {

        EditorUtility.RevealInFinder(Application.persistentDataPath);

    }


}
