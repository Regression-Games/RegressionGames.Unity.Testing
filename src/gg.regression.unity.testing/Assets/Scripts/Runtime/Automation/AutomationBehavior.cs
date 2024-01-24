using System;
using RegressionGames.Unity.Recording;
using UnityEngine;

namespace RegressionGames.Unity.Automation
{
    /// <summary>
    /// Base class for behaviors that expect to be placed under an <see cref="AutomationController"/>.
    /// </summary>
    public abstract class AutomationBehavior: MonoBehaviour
    {
        private AutomationController m_AutomationController;

        protected AutomationController AutomationController => m_AutomationController;

        protected virtual void Awake()
        {
            m_AutomationController = GetComponentInParent<AutomationController>();
            if (m_AutomationController == null)
            {
                throw new InvalidOperationException("EntityDiscoverer must be a child of an AutomationController.");
            }
        }
    }
}
