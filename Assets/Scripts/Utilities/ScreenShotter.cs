#if (UNITY_EDITOR)
using System.IO;
using UnityEngine;

namespace Utilities
{
    public class ScreenShotter : MonoBehaviour
    {
        private void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F12))
            {
                DirectoryInfo di = new DirectoryInfo("Itch");

                int index = di.GetFiles().Length + 1;
                string path = $"Itch/screen_{index}.png";

                ScreenCapture.CaptureScreenshot(path);

                Debug.Log($"Screenshot {index} saved to \"{path}\".");
            }
        }
    }
}
#endif
