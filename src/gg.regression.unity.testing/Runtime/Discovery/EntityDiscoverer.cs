using System.Collections.Generic;
using System.Linq;
using RegressionGames.Unity.Automation;
using UnityEngine;

namespace RegressionGames.Unity.Discovery
{
    public abstract class EntityDiscoverer: AutomationBehavior
    {
        private readonly List<AutomationEntity> m_Entities = new();

        protected override void Awake()
        {
            base.Awake();

            foreach (var entity in DiscoverEntities())
            {
                RegisterEntity(entity);
            }
        }

        protected virtual void OnDestroy()
        {
            foreach (var entity in m_Entities)
            {
                AutomationController.UnregisterEntity(entity);
            }
            m_Entities.Clear();
        }

        protected void RegisterEntity(AutomationEntity entity)
        {
            m_Entities.Add(entity);
            AutomationController.RegisterEntity(entity);
        }

        protected virtual IEnumerable<AutomationEntity> DiscoverEntities() => Enumerable.Empty<AutomationEntity>();

        protected IEnumerable<T> FindAutomatableComponentsOfType<T>() where T : Component
        {
            var components = FindObjectsOfType<T>();
            return components.Where(IsAutomatable);
        }

        protected bool IsAutomatable(Component component)
        {
            // First off, check for an Automatable component on the game object itself
            var automatable = component.GetComponent<Automatable>();
            if (automatable != null)
            {
                // There is one, so it's isAutomatable property determines whether this component is automatable.
                return automatable.isAutomatable;
            }

            // Now, scan for the first parent that has an Automatable component.
            var parentAutomatable = component.GetComponentInParent<Automatable>();
            if (parentAutomatable != null)
            {
                // There is one, so it's childrenAreAutomatable property determines whether this component is automatable.
                return parentAutomatable.childrenAreAutomatable;
            }

            // By default, we assume that the component is automatable.
            return true;
        }
    }
}
