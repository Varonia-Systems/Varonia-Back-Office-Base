using System.Collections;
using System.Collections.Generic;
using Tayx.Graphy.Fps;
using UnityEngine;
using UnityEngine.UI;

namespace VaroniaBackOffice
{
    public class FPS3D : MonoBehaviour
    {
        public G_FpsText g_FpsText;


        Text text;
        void Start()
        {
            text = transform.GetChild(0).GetComponent<Text>();
        }

        void Update()
        {
            text.text = g_FpsText.PublicFPS.ToString() + " Fps";
        }
    }
}