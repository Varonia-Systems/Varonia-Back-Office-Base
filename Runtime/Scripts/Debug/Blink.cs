using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Blink : MonoBehaviour
{
    public Image blink;
    private void FixedUpdate()
    {
        blink.enabled = !blink.enabled;
    }
}
