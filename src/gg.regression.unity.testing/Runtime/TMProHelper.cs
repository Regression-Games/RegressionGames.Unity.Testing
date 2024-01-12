using TMPro;
using UnityEngine;

namespace RegressionGames.Unity
{
    static class TMProHelper
    {
        public static string TryGetTMProTextFromChild(Component parent)
        {
            var text = parent.GetComponentInChildren<TextMeshPro>();
            if (text == null)
            {
                return null;
            }

            return text.text;
        }
    }
}
