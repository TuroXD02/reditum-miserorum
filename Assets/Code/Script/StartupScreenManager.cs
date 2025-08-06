using UnityEngine;

public class StartupScreenManager : MonoBehaviour
{
    void Start()
    {
        // Ensure we're using Display 1 (main display)
        if (Display.displays.Length > 0)
        {
            Display.displays[0].Activate(); // Ensure main display is active
        }

        // Force window to open centered and full screen on primary monitor
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        Screen.fullScreen = true;
    }
}