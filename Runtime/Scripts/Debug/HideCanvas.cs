using System;
using UnityEngine;
using VaroniaBackOffice;

public class HideCanvas : MonoBehaviour
{
    private Canvas canvas;


    void Awake()
    {
        canvas = GetComponent<Canvas>();
    }
    
 private void FixedUpdate()
 {
    if(Config.VaroniaConfig.Movie || VaroniaGlobal.HideDebugCanvas)
        canvas.enabled = false;
    else
        canvas.enabled = true;

 }
}
