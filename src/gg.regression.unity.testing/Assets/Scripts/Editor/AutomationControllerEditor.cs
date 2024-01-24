using RegressionGames.Unity.Automation;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RegressionGames.Unity
{
    [CustomEditor(typeof(AutomationController))]
    public class AutomationControllerEditor: Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var myInspector = new VisualElement();

            myInspector.Add(new PropertyField(serializedObject.FindProperty("dontDestroyOnLoad"),"Don't Destroy on Load"));

            return myInspector;
        }
    }
}
