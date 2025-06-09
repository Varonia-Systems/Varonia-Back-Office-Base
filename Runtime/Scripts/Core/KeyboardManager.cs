using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class KeyboardManager : MonoBehaviour
{
    private static KeyboardManager instance;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        KeyboardHook.Update();
    }
}