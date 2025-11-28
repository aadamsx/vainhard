using UnityEngine;
using System.IO;

/// <summary>
/// Captures screenshots when F12 is pressed.
/// Screenshots are saved to the project's Screenshots folder.
/// </summary>
public class AutoScreenshot : MonoBehaviour
{
    private static string screenshotFolder;
    private static int screenshotCount = 0;

    void Awake()
    {
        // Create screenshots folder in project root (not Assets)
        screenshotFolder = Path.Combine(Application.dataPath, "..", "Screenshots");
        if (!Directory.Exists(screenshotFolder))
        {
            Directory.CreateDirectory(screenshotFolder);
        }
    }

    void Update()
    {
        // Press F12 to capture screenshot
        if (Input.GetKeyDown(KeyCode.F12))
        {
            CaptureScreenshot();
        }
    }

    public void CaptureScreenshot()
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string filename = $"screenshot_{timestamp}.png";
        string path = Path.Combine(screenshotFolder, filename);

        ScreenCapture.CaptureScreenshot(path);
        Debug.Log($"Screenshot saved to: {path}");
    }

    // Static method to capture from anywhere
    public static void CaptureNow()
    {
        if (string.IsNullOrEmpty(screenshotFolder))
        {
            screenshotFolder = Path.Combine(Application.dataPath, "..", "Screenshots");
            if (!Directory.Exists(screenshotFolder))
            {
                Directory.CreateDirectory(screenshotFolder);
            }
        }

        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string filename = $"screenshot_{timestamp}.png";
        string path = Path.Combine(screenshotFolder, filename);

        ScreenCapture.CaptureScreenshot(path);
        Debug.Log($"Screenshot saved to: {path}");
    }
}
