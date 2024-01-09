using System;
using System.Reflection;
using UnityEngine;

namespace RegressionGames.Unity
{
    static class TMProHelper
    {
        private static readonly Type TMProTextType;
        private static readonly PropertyInfo TMProTextProperty;

        static TMProHelper()
        {
            TMProTextType = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro", false);
            TMProTextProperty = TMProTextType?.GetProperty("text");
        }

        // Helper class for working with TextMeshPro without a hard dependency on it.
        public static string TryGetTMProTextFromChild(Component parent)
        {
            if (TMProTextType == null || TMProTextProperty == null)
            {
                return null;
            }

            var text = parent.GetComponentInChildren(TMProTextType);
            if (text == null)
            {
                return null;
            }

            return TMProTextProperty.GetValue(text) as string;
        }
    }
}
