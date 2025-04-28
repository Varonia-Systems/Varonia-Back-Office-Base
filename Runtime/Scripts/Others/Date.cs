using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Date : MonoBehaviour
{

     Text txt;

    void Awake()
    {
        txt = GetComponent<Text>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        txt.text = DateTime.Now.ToString("HH:mm:ss");
    }
}
