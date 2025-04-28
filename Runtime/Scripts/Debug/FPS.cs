using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPS : MonoBehaviour
{
    public static int S_Fps;

    public float UpdateInterval = 1;
    private Text text;
    private int frames;
    // Use this for initialization
    void Start ()
    {
        Application.targetFrameRate = 1000;
        text = GetComponent<Text>();
		InvokeRepeating("UpdateFPS", UpdateInterval, UpdateInterval);
	}

    void UpdateFPS()
    {
        if (frames < 40) text.color = Color.red;
        if(frames >= 40 && frames <= 60) text.color = Color.yellow;
        else text.color = Color.green;
        text.text = "" + frames ;
        S_Fps = frames;
        frames = 0;
    }
	
	// Update is called once per frame
	void Update ()
	{
	    frames++;
	}
}
