using System;
using System.IO;
using System.Linq;
using RegressionGames.Unity.Automation;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace RegressionGames.Unity
{
    static class Menus
    {
        const int MenuPriority = 10;

        private static readonly Logger m_Log = Logger.For(typeof(Menus).FullName);

        const string k_ThunkLabel = "gg.regression.unity.swap-script-on-load-thunk";

        [InitializeOnLoadMethod]
        static void OnLoad()
        {
            // Find all the swap script thunks in the assets folder and swap them out.
            var guids = AssetDatabase.FindAssets($"l:{k_ThunkLabel}");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var thunk = prefab.GetComponent<SwapScriptOnLoadThunk>();
                if (thunk == null)
                {
                    m_Log.Warning($"GameObject at {path} has no {nameof(SwapScriptOnLoadThunk)} component.");
                    continue;
                }

                var script = string.IsNullOrEmpty(thunk.scriptPath)
                    ? null
                    : AssetDatabase.LoadAssetAtPath<MonoScript>(thunk.scriptPath);
                if (script == null)
                {
                    m_Log.Error("GameObject at {path} has no script path set.");
                }
                else
                {
                    var type = script.GetClass();
                    if(type == null)
                    {
                        m_Log.Error($"Script at {thunk.scriptPath} has no class.");
                    }
                    else
                    {
                        prefab.AddComponent(type);
                    }
                }
                // DestroyImmediate is require in editor code because delayed destroy code doesn't run in editor mode.
                Object.DestroyImmediate(thunk, allowDestroyingAssets: true);
                var labels = AssetDatabase.GetLabels(prefab);
                var newLabels = labels.Where(l => l != k_ThunkLabel).ToArray();
                AssetDatabase.SetLabels(prefab, newLabels);
                PrefabUtility.SavePrefabAsset(prefab);
            }
        }

        [MenuItem("Assets/Create/Regression Games/Custom Bot...", priority = MenuPriority)]
        static void CreateCustomBot(MenuCommand context)
        {
            CreateScriptAndPrefabFromTemplate(
                "Create Custom Bot",
                "CustomBot.cs",
                "Please enter a name for the new Custom Bot script.",
                "Packages/gg.regression.unity.testing/Editor/Templates/CustomBotTemplate.cs.txt");
        }

        [MenuItem("Assets/Create/Regression Games/Custom Discoverer...", priority = MenuPriority)]
        static void CreateCustomEntityDiscoverer(MenuCommand context)
        {
            CreateScriptAndPrefabFromTemplate(
                "Create Custom Discoverer",
                "CustomDiscoverer.cs",
                "Please enter a name for the new Custom Discoverer script.",
                "Packages/gg.regression.unity.testing/Editor/Templates/CustomDiscovererTemplate.cs.txt");
        }

        private static void CreateScriptAndPrefabFromTemplate(
            string title, string defaultName, string message, string templatePath)
        {
            var folder = Selection.activeObject is DefaultAsset asset ? AssetDatabase.GetAssetPath(asset) : null;

            var scriptPath = EditorUtility.SaveFilePanelInProject(
                title,
                defaultName,
                "cs",
                message,
                folder);
            if (!scriptPath.EndsWith(".cs"))
            {
                scriptPath += ".cs";
            }

            var prefabPath = Path.ChangeExtension(scriptPath, ".prefab");

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(scriptPath);
            var safeClassName = fileNameWithoutExtension.Replace(" ", "");

            var template = AssetDatabase.LoadAssetAtPath<TextAsset>(templatePath);
            var rootNamespace = CompilationPipeline.GetAssemblyRootNamespaceFromScriptPath(scriptPath);
            var scriptContent = template.text
                .Replace("#NAMESPACE#", rootNamespace)
                .Replace("#SCRIPTNAME#", safeClassName);
            File.WriteAllText(Path.GetFullPath(scriptPath), scriptContent);
            AssetDatabase.ImportAsset(scriptPath);

            var go = new GameObject();
            var thunk = go.AddComponent<SwapScriptOnLoadThunk>();
            thunk.scriptPath = scriptPath;
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            AssetDatabase.SetLabels(prefab, new[]
            {
                k_ThunkLabel
            });

            // Destroy the original game object
            Object.DestroyImmediate(go, allowDestroyingAssets: true);

            m_Log.Info("Waiting for compilation to finish...");
        }

        [MenuItem("GameObject/Regression Games/Automation Controller", priority = MenuPriority)]
        static void CreateAutomationController(MenuCommand context) =>
            InstantiatePrefab(context, "AutomationController", "Automation Controller", false);

        [MenuItem("GameObject/Regression Games/Automation Recorder", priority = MenuPriority)]
        static void CreateAutomationRecorder(MenuCommand context) => InstantiatePrefab(context, "AutomationRecorder", "Automation Recorder", true);

        [MenuItem("GameObject/Regression Games/Discovery/UI Element Discoverer", priority = MenuPriority)]
        static void CreateUIElementDiscoverer(MenuCommand context) =>
            InstantiatePrefab(context, "UIElementDiscoverer", "UI Element Discoverer", true);

        [MenuItem("GameObject/Regression Games/Bots/Monkey Bot", priority = MenuPriority)]
        static void CreateMonkeyBot(MenuCommand context) => InstantiatePrefab(context, "MonkeyBot", "Monkey Bot", true);

        [MenuItem("GameObject/Regression Games/UI Overlay", priority = MenuPriority)]
        static void CreateUIOverlay(MenuCommand context) => InstantiatePrefab(context, "AutomationUIOverlay", "RegressionGames UI Overlay", false);

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
                    if (TryGetAutomationController(out parent))
                    {
                        return true;
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

        private static bool TryGetAutomationController(out GameObject parent)
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

            parent = null;
            return false;
        }
    }
}
