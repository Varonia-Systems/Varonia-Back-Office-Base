using System.Runtime.InteropServices;
using UnityEngine;

public static class MouseHook
{
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    // Mouse button virtual-key codes
    private const int VK_LBUTTON = 0x01;
    private const int VK_RBUTTON = 0x02;
    private const int VK_MBUTTON = 0x04;

    private static bool leftDownLast = false;
    private static bool rightDownLast = false;
    private static bool middleDownLast = false;

    private static bool leftDown = false;
    private static bool rightDown = false;
    private static bool middleDown = false;

    // Doit être appelé en début d’Update
    public static void Update()
    {
        leftDown = (GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0;
        rightDown = (GetAsyncKeyState(VK_RBUTTON) & 0x8000) != 0;
        middleDown = (GetAsyncKeyState(VK_MBUTTON) & 0x8000) != 0;
    }

    public static bool GetMouseButtonDown(int button)
    {
        switch (button)
        {
            case 0: return leftDown && !leftDownLast;
            case 1: return rightDown && !rightDownLast;
            case 2: return middleDown && !middleDownLast;
            default: return false;
        }
    }

    public static bool GetMouseButtonUp(int button)
    {
        switch (button)
        {
            case 0: return !leftDown && leftDownLast;
            case 1: return !rightDown && rightDownLast;
            case 2: return !middleDown && middleDownLast;
            default: return false;
        }
    }

    // À appeler après la logique pour mémoriser l’état pour la prochaine frame
    public static void LateUpdate()
    {
        leftDownLast = leftDown;
        rightDownLast = rightDown;
        middleDownLast = middleDown;
    }
}
