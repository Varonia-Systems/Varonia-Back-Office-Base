using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;




public class GameTime : MonoBehaviour
{

    public Text txt;


    private void FixedUpdate()
    {
        txt.text = " Game Started Since : " + Math.Round(Time.time, 1) + " Sec ";
    }

}
