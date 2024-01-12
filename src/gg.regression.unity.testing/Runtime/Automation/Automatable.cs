using UnityEngine;

namespace RegressionGames.Unity.Automation
{
    /// <summary>
    /// Behavior for configuring the automatability of a game object.
    /// </summary>
    /// <remarks>
    /// This component is NOT required for a game object to be automatable.
    /// By default, a game object is automatically automatable.
    /// However, this game object can be used to disable automatability for a game object and its children.
    /// </remarks>
    [AddComponentMenu("Regression Games/Automatable")]
    public class Automatable: MonoBehaviour
    {
        [Tooltip("Specifies whether this game object can be automated.")]
        public bool isAutomatable = true;

        [Tooltip("Specifies whether children of this game object can be automated.")]
        public bool childrenAreAutomatable = true;
    }
}
