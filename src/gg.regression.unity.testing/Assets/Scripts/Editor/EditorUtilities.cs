using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEditor;

namespace RegressionGames.Unity
{
    internal static class EditorUtilities
    {
        // The Conditional attribute causes CALLSITES of this method to be compiled out if the specified symbol is not defined.
        [Conditional("UNITY_EDITOR")]
        public static void OpenFileBrowser(string path)
        {
            var quotedPath = $"\"{path}\"";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start("explorer.exe", quotedPath);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", quotedPath);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", quotedPath);
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Unsupported Platform",
                    $"Could not open '{path}' in your file browser.",
                    "OK");
            }
        }
    }
}
