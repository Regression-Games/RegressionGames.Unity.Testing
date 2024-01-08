using System;
using UnityEngine;

namespace RegressionGames.Unity.Automation
{
    /// <summary>
    /// Represents a bot that can be spawned by an <see cref="AutomationController"/>
    /// </summary>
    public abstract class Bot: AutomationBehavior
    {
        AutomationController m_AutomationController;

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
