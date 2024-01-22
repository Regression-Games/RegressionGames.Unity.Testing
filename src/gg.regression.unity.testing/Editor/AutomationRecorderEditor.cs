using RegressionGames.Unity.Recording;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RegressionGames.Unity
{
    [CustomEditor(typeof(AutomationRecorder))]
    public class AutomationRecorderEditor: Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var myInspector = new VisualElement();

            myInspector.Add(new PropertyField(serializedObject.FindProperty("recordingDirectory"),"Recording Directory"));
            myInspector.Add(new PropertyField(serializedObject.FindProperty("snapshotRate"),"Snapshot Rate"));
            myInspector.Add(new PropertyField(serializedObject.FindProperty("saveSnapshotsOnlyWhenChanged"),
                "Only Save Changes"));
            myInspector.Add(new Button(() =>
            {
                var recorder = (AutomationRecorder) target;
                var recordingDir = recorder.GetRecordingDirectory();
                if(string.IsNullOrEmpty(recordingDir))
                {
                    EditorUtility.DisplayDialog(
                        "No Recording Directory",
                        "The AutomationRecorder component does not have a recording directory specified.",
                        "OK");
                    return;
                }

                // Open the directory
                EditorUtilities.OpenFileBrowser(recordingDir);
            })
            {
                text = "View recordings...",
                style =
                {
                    marginTop = 10,
                }
            });

            return myInspector;
        }
    }
}
