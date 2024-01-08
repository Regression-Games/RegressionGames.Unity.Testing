using System;
using System.Collections.Generic;
using System.Linq;
using RegressionGames.Unity.Automation;

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
    }
}
