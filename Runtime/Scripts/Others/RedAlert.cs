using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RedAlert : MonoBehaviour
{
    public Image img;


    public void OnEnable()
    {
        StartCoroutine(Anim());
        img.color = new Color(img.color.r, img.color.g, img.color.b,0.4f);
    }

    IEnumerator Anim()
    {
        while (true)
        {


            for (int i = 0; i < 70; i++)
            {
                img.color -= new Color(0, 0, 0, (float)(1 / 255f));
                yield return new WaitForSeconds(0.01f);
            }

            for (int i = 0; i < 70; i++)
            {
                img.color += new Color(0, 0, 0, (float)(1 / 255f));
                yield return new WaitForSeconds(0.01f);
            }
        }

    }

}
