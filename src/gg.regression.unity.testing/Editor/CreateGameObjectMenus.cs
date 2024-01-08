using System;
using System.Net.Mime;
using RegressionGames.Unity.Automation;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace RegressionGames.Unity
{
    static class CreateGameObjectMenus
    {
        const int MenuPriority = 10;

        [MenuItem("GameObject/Regression Games/Automation Controller", priority = MenuPriority)]
        static void CreateAutomationController(MenuCommand context) =>
            InstantiatePrefab(context, "AutomationController", "Automation Controller", false);

        [MenuItem("GameObject/Regression Games/Discovery/UI Element Discoverer", priority = MenuPriority)]
        static void CreateUIElementDiscoverer(MenuCommand context) =>
            InstantiatePrefab(context, "UIElementDiscoverer", "UI Element Discoverer", true);

        [MenuItem("GameObject/Regression Games/Bots/Monkey Bot", priority = MenuPriority)]
        static void CreateMonkeyBot(MenuCommand context) => InstantiatePrefab(context, "MonkeyBot", "Monkey Bot", true);

        static void InstantiatePrefab(MenuCommand context, string prefabName, string name, bool parentToAutomationController)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                $"Packages/gg.regression.unity.testing/Assets/Prefabs/{prefabName}.prefab");
            if (prefab == null)
            {
                throw new InvalidOperationException($"{prefabName} prefab not found.");
            }

            // If we're parenting to an automation controller, we need to find/validate the parent
            if (!GetOrValidateParent(name, context.context as GameObject, parentToAutomationController, out var parent))
            {
                return;
            }

            var spawned = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            spawned.name = name;
            GameObjectUtility.SetParentAndAlign(spawned, parent);
            Undo.RegisterCreatedObjectUndo(spawned, $"Create {spawned.name}");
            Selection.activeObject = spawned;
        }

        private static bool GetOrValidateParent(
            string name, GameObject selection, bool parentToAutomationController, out GameObject parent)
        {
            if (parentToAutomationController)
            {
                if (selection != null)
                {
                    // If the user has selected a game object, use that as the parent.
                    // But we need to check if it's got an automation controller as a parent
                    if (selection.GetComponentInParent<AutomationController>() == null)
                    {
                        EditorUtility.DisplayDialog("Invalid parent",
                            $"A {name} must be placed in the scene as a descendant of an Automation Controller.", "OK");
                        parent = null;
                        return false;
                    }
                }
                else
                {
                    // Find the automation controller in the scene
                    var scene = SceneManager.GetActiveScene();
                    foreach (var root in scene.GetRootGameObjects())
                    {
                        var candidate = root.GetComponentInChildren<AutomationController>();
                        if (candidate != null)
                        {
                            parent = candidate.gameObject;
                            return true;
                        }
                    }

                    // If we get here, we didn't find an automation controller
                    EditorUtility.DisplayDialog("No automation controller",
                        $"There is no Automation Controller in the scene. Please add one before adding a {name}.",
                        "OK");
                    parent = null;
                    return false;
                }
            }

            parent = selection;
            return true;
        }
    }
}
