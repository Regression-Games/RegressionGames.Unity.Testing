using UnityEngine;
using UnityEngine.UI;

namespace RegressionGames.Unity
{
    static class UIUtils
    {
        public static string DescribeSelectable(Selectable selectable)
        {
            var label = GetLabel(selectable);
            if (selectable is Button)
            {
                return $"A button labelled '{label}'";
            }

            return $"A UI element labelled '{label}'";
        }

        public static string GetLabel(Component uiElement)
        {
            if (TMProHelper.TryGetTMProTextFromChild(uiElement) is { } tmProText)
            {
                return tmProText;
            }

            // Check if there's a child text element, and if so, use that as the name.
            var text = uiElement.GetComponentInChildren<Text>();
            if (text != null)
            {
                return text.text;
            }

            return uiElement.gameObject.name;
        }
    }
}
