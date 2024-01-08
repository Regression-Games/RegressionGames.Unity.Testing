using System.Collections.Generic;
using UnityEngine;

namespace RegressionGames.Unity.Automation
{
    /// <summary>
    /// A component that can be used to control automation of the game.
    /// Any scene that can be automated by Regression Games must have this component somewhere in it.
    /// </summary>
    [AddComponentMenu("Regression Games/Automation Controller")]
    public class AutomationController: MonoBehaviour
    {
        // TODO: We can "index" entities by other values, like their ID, name or type, to make it easier to find them.
        // For now though, we'll implement those finders by iterating the list, until we have a need to optimize.
        private readonly List<IAutomationEntity> m_Entities = new();

        public IReadOnlyList<IAutomationEntity> Entities => m_Entities;

        private readonly Logger<AutomationController> m_Log;

        public AutomationController()
        {
            m_Log = Logger.For(this);
        }

        public void RegisterEntity(IAutomationEntity entity)
        {
            m_Entities.Add(entity);
        }

        public void UnregisterEntity(IAutomationEntity entity)
        {
            m_Entities.Remove(entity);
        }
    }
}
