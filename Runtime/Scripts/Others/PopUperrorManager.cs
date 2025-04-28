using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ErrorType
{
    unknown = 404,
    JsonError = 1001,
}


public class Error_
{
    public ErrorType errorType;
    public string TrueError_Msg;
    public string Friendly_Msg;

}





public class PopUperrorManager : MonoBehaviour
{
    public PopUpError Prefab;

    public static PopUperrorManager Instance;


    private void Awake()
    {
        Instance = this;
    }


    public void ShowError(ErrorType Type, string FriendlyMsg, string TrueMsg)
    {

        var error = new Error_();

        error.errorType = Type;
        error.TrueError_Msg = TrueMsg;
        error.Friendly_Msg = FriendlyMsg;

        var A = StartCoroutine(Instantiate(Prefab, transform).Show(error));
    }



    [Button]
    public void FakeError()
    {
        ShowError(ErrorType.unknown, "Boob Cooney est un gros connard !", "");
    }


}
