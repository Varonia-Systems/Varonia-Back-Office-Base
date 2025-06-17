using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VaroniaBackOffice
{
    public class FPSUi : MonoBehaviour
    {
        public bool showAverage = false;
        Text text;
        void Start()
        {
            text = transform.GetComponentInChildren<Text>();
        }

        void Update()
        {
            string currentFpsText = ColorText(FpsUtility.GetCurrentFps().ToString(), FpsUtility.GetCurrentFpsColor());
            
            if (showAverage)
            {
                string avgFpsText = ColorText(" ~" + FpsUtility.GetAverageFpsLastMinute().ToString("F0"), FpsUtility.GetAverageFpsColor());
                text.text = currentFpsText + avgFpsText;
            }
            else
            {
                text.text = currentFpsText;
            }
        }
        
        private string ColorText(string text, Color color)
        {
            string hexColor = ColorUtility.ToHtmlStringRGB(color);
            return $"<color=#{hexColor}>{text}</color>";
        }
    }


}