using UnityEngine;

namespace RegressionGames.Unity.Automation
{
    public interface IAutomationAction
    {
        /// <summary>
        /// Gets the name of this action.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a boolean indicating if this action can be activated on this frame.
        /// </summary>
        /// <returns></returns>
        bool CanActivateThisFrame();

        /// <summary>
        /// Activates this action.
        /// </summary>
        void Activate();
    }
}
