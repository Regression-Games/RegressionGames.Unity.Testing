using System.Collections.Generic;
using RegressionGames.Unity.Automation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#nullable enable

namespace RegressionGames.Unity.Discovery
{
    [AddComponentMenu("Regression Games/Discovery/UI Element Discoverer")]
    public class UIElementDiscoverer : ActionDiscoverer
    {
        private readonly Logger<UIElementDiscoverer> m_Log;

        public UIElementDiscoverer()
        {
            m_Log = Logger.For(this);
        }

        public override IEnumerable<IAutomationAction> DiscoverActions()
        {
            m_Log.Verbose("Scanning for UI elements...");
            var buttons = FindObjectsOfType<Button>();
            foreach (var button in buttons)
            {
                yield return new UIButtonAction(button);
            }
        }
    }

    public class UIButtonAction : IAutomationAction
    {
        private readonly Button m_Button;

        public UIButtonAction(Button button)
        {
            m_Button = button;
        }

        public bool CanActivateThisFrame() => m_Button.IsInteractable();

        public void Activate()
        {
            ExecuteEvents.Execute<IPointerClickHandler>(
                m_Button.gameObject,
                new PointerEventData(EventSystem.current),
                (h, d) => h.OnPointerClick((PointerEventData)d));
        }
    }
}
