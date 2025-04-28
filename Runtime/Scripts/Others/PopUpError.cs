using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class PopUpError : MonoBehaviour
{

    public IEnumerator Show(Error_ error)
    {
        var A = GetComponent<CanvasGroup>();
        var Txt = GetComponentInChildren<Text>();

        Txt.text = "Error " + (int)error.errorType + " - " + error.errorType + "\n" + error.Friendly_Msg;

        yield return new WaitForSeconds(8f);

        while (A.alpha > 0)
        {
            yield return new WaitForSeconds(0.01f);
            A.alpha -= 0.025f;
        }

        Destroy(gameObject);



    }


}
