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

        protected override IEnumerable<IAutomationEntity> DiscoverEntities()
        {
            m_Log.Verbose("Scanning for UI elements...");
            var buttons = FindObjectsOfType<Button>();
            foreach (var button in buttons)
            {
                yield return new UIButtonEntity(button);
            }
        }

        class UIButtonEntity : IAutomationEntity
        {
            public int Id { get; }
            public string Name { get; }
            public string Description => $"A button labelled '{Name}'";

            public UIButtonEntity(Button button)
            {
                Id = button.transform.GetInstanceID();
                Name = DiscoverName(button);

                var clickAction = new UIButtonAction(button);
                Actions = new Dictionary<string, IAutomationAction>
                {
                    { clickAction.Name, clickAction }
                };
            }

            public IReadOnlyDictionary<string, IAutomationAction> Actions { get; }

            static string DiscoverName(Component uiElement)
            {
                if (TMProHelper.TryGetTMProTextFromChild(uiElement) is { } tmProText)
                {
                    return tmProText;
                }

                // Check if there's a child text element, and if so, use that as the name.
                var text = uiElement.GetComponentInChildren<Text>();
                if (text != null)
                {
                    return text.text;
                }

                return uiElement.gameObject.name;
            }
        }

        class UIButtonAction : IAutomationAction
        {
            private readonly Button m_Button;
            private readonly Logger m_Log;

            public string Name => "Click";
            public string Description => "Clicks the button";

            public UIButtonAction(Button button)
            {
                m_Button = button;
                m_Log = Logger.For(typeof(UIButtonAction).FullName);
            }

            public bool CanActivateThisFrame() => m_Button.IsInteractable();

            public void Activate()
            {
                m_Log.Verbose($"Activating {Name}", m_Button);
                ExecuteEvents.Execute<IPointerClickHandler>(
                    m_Button.gameObject,
                    new PointerEventData(EventSystem.current),
                    (h, d) => h.OnPointerClick((PointerEventData)d));
            }

        }
    }
}
