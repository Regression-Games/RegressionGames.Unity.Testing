using System;
using RegressionGames.Unity.Automation;
using UnityEngine;

namespace RegressionGames.Unity.Discovery
{
    public abstract class EntityDiscoverer: MonoBehaviour
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
