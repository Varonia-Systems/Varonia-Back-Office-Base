using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public static class KeyboardHook
{
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    private static readonly Dictionary<KeyCode, int> keyMap = new Dictionary<KeyCode, int>
    {
        { KeyCode.M, 0x4D },
        { KeyCode.F1, 0x70 },
        { KeyCode.F2, 0x71 },
        { KeyCode.F3, 0x72 },
        { KeyCode.F7, 0x76 },
        { KeyCode.F8, 0x77 },
        { KeyCode.F12, 0x7B },
        { KeyCode.KeypadPlus, 0x6B },
        { KeyCode.KeypadMinus, 0x6D },
        { KeyCode.KeypadEnter, 0x0D },
        { KeyCode.UpArrow, 0x26 },
        { KeyCode.DownArrow, 0x28 },
        { KeyCode.LeftArrow, 0x25 },
        { KeyCode.RightArrow, 0x27 },
        { KeyCode.LeftShift, 0xA0 },
        { KeyCode.RightShift, 0xA1 },
        { KeyCode.RightAlt, 0xA5 }
    };

    private static Dictionary<KeyCode, bool> keyDownStates = new Dictionary<KeyCode, bool>();
    private static Dictionary<KeyCode, bool> keyPressedLastFrame = new Dictionary<KeyCode, bool>();

    // Appeler au tout début de chaque Update() dans ton MonoBehaviour
    public static void Update()
    {
        foreach (var key in keyMap.Keys)
        {
            bool pressed = (GetAsyncKeyState(keyMap[key]) & 0x8000) != 0;

            if (!keyPressedLastFrame.ContainsKey(key))
                keyPressedLastFrame[key] = false;

            keyDownStates[key] = pressed && !keyPressedLastFrame[key];

            keyPressedLastFrame[key] = pressed;
        }
    }

    public static bool GetKeyDown(KeyCode key)
    {
        return keyDownStates.ContainsKey(key) && keyDownStates[key];
    }

    public static bool GetKey(KeyCode key)
    {
        return keyPressedLastFrame.ContainsKey(key) && keyPressedLastFrame[key];
    }
}
