using System;
using System.Collections.Generic;
using RegressionGames.Unity.Discovery;
using UnityEngine;
using UnityEngine.Serialization;

#nullable enable

namespace RegressionGames.Unity.Automation
{
    /// <summary>
    /// A component that can be used to control automation of the game.
    /// Any scene that can be automated by Regression Games must have this component somewhere in it.
    /// </summary>
    [AddComponentMenu("Regression Games/Automation Controller")]
    public class AutomationController: MonoBehaviour
    {
        [FormerlySerializedAs("ActionDiscoverers")]
        [Tooltip("The components used to discover actions the bot can take in this scene.")]
        public ActionDiscoverer[]? actionDiscoverers;

        private List<IAutomationAction>? m_AvailableActions = null;

        private readonly Logger<AutomationController> m_Log;

        public AutomationController()
        {
            m_Log = Logger.For(this);
        }

        private void Awake()
        {
            // TODO: Should we only do this on Awake? What about objects spawned after the scene starts?
            // I'd prefer not to do it every Update, but we may need to.

            // Discover available actions
            var actions = new List<IAutomationAction>();
            foreach(var actionDiscoverer in actionDiscoverers ?? Array.Empty<ActionDiscoverer>())
            {
                m_Log.Verbose($"Discovering actions from {actionDiscoverer.GetType().FullName}");
                actions.AddRange(actionDiscoverer.DiscoverActions());
            }
            m_AvailableActions = actions;
        }

        private void Update()
        {
        }
    }
}
