using System.Collections.Generic;
using RegressionGames.Unity.Automation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RegressionGames.Unity.Discovery
{
    [AddComponentMenu("Regression Games/Discovery/UI Element Discoverer")]
    public class UIElementDiscoverer : EntityDiscoverer
    {
        private readonly Logger<UIElementDiscoverer> m_Log;

        public UIElementDiscoverer()
        {
            m_Log = Logger.For(this);
        }

        protected override IEnumerable<AutomationEntity> DiscoverEntities()
        {
            m_Log.Verbose("Scanning for UI elements...");
            var uiBehaviours = FindAutomatableComponentsOfType<Selectable>();
            foreach (var uiBehaviour in uiBehaviours)
            {
                yield return new UISelectableEntity(uiBehaviour);
            }

            var canvasGroups = FindAutomatableComponentsOfType<CanvasGroup>();
            foreach (var canvasGroup in canvasGroups)
            {
                yield return new UIGroupEntity(canvasGroup);
            }
        }

        class UIGroupEntity : AutomationEntity<CanvasGroup>
        {
            public UIGroupEntity(CanvasGroup canvasGroup): base(canvasGroup, $"A canvas group named {canvasGroup.gameObject.name}")
            {
            }

            public override IEnumerable<KeyValuePair<string, object>> GetState()
            {
                yield return new("alpha", Component.alpha);
                yield return new("interactable", Component.interactable);
                yield return new("blocksRaycasts", Component.blocksRaycasts);
            }
        }

        class UISelectableEntity : AutomationEntity<Selectable>
        {
            public override IReadOnlyDictionary<string, AutomationAction> Actions { get; }

            public UISelectableEntity(Selectable selectable): base(selectable, UIUtils.DescribeSelectable(selectable))
            {
                var actions = new Dictionary<string, AutomationAction>();
                if (selectable is IPointerClickHandler)
                {
                    var clickAction = new UIClickAction(selectable, this);
                    actions.Add(clickAction.Name, clickAction);
                }
                Actions = actions;
            }
        }

        class UIClickAction : AutomationAction
        {
            private readonly Selectable m_Selectable;
            private readonly Logger m_Log;

            public UIClickAction(Selectable selectable, AutomationEntity entity): base("Click", "Clicks the element", entity)
            {
                m_Selectable = selectable;
                m_Log = Logger.For(typeof(UIClickAction).FullName);
            }

            public override bool CanActivateThisFrame() => m_Selectable.IsInteractable();

            protected override void Execute()
            {
                m_Log.Verbose($"Activating {Name}", m_Selectable);
                ExecuteEvents.Execute<IPointerClickHandler>(
                    m_Selectable.gameObject,
                    new PointerEventData(EventSystem.current),
                    (h, d) => h.OnPointerClick((PointerEventData)d));
            }
        }
    }
}
