using System;
using System.Collections.Generic;
using System.Linq;
using RegressionGames.Unity.Discovery;
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
        [Tooltip("The components used to discover actions the bot can take in this scene.")]
        public ActionDiscoverer[]? actionDiscoverers;

        [Tooltip("By default, action discovery happens once when the controller is spawned. If this is true, actions will be discovered every frame, during Update")]
        public bool discoverEveryFrame;

        [Tooltip("Bots that should be spawned as soon as the Automation Controller awakens in the scene.")]
        public Bot[]? initialBots;

        private List<IAutomationAction>? m_DiscoveredActions = null;
        private List<Bot> m_ActiveBots = new();

        private readonly Logger<AutomationController> m_Log;

        public AutomationController()
        {
            m_Log = Logger.For(this);
        }

        private void Awake()
        {
            DiscoverActions();

            foreach (var bot in initialBots ?? Array.Empty<Bot>())
            {
                var spawned = Instantiate(bot, transform);
                m_ActiveBots.Add(spawned);
            }
        }

        private void OnDestroy()
        {
            // Bots we spawned belong to us. Destroy them when we're destroyed.
            foreach (var bot in m_ActiveBots)
            {
                Destroy(bot);
            }
            m_ActiveBots.Clear();
        }

        private void Update()
        {
            if (discoverEveryFrame)
            {
                DiscoverActions();
            }

            var availableActions = m_DiscoveredActions == null
                ? Array.Empty<IAutomationAction>()
                : m_DiscoveredActions.Where(a => a.CanActivateThisFrame()).ToArray();
            m_Log.Verbose($"Found {availableActions.Length} actions available this frame");

            // Run through all the bots and give them the actions they can perform
            var botContext = new BotContext(availableActions, m_DiscoveredActions);
            foreach (var bot in m_ActiveBots)
            {
                m_Log.Verbose($"Executing bot {bot.GetType().FullName}");

                // Don't allow one bot to stop another by throwing
                try
                {
                    bot.Execute(botContext);
                }
                catch (Exception ex)
                {
                    m_Log.Exception(ex, bot);
                }
            }
        }

        private void DiscoverActions()
        {
            // Discover available actions
            var actions = new List<IAutomationAction>();
            foreach (var actionDiscoverer in actionDiscoverers ?? Array.Empty<ActionDiscoverer>())
            {
                var oldLen = actions.Count;
                actions.AddRange(actionDiscoverer.DiscoverActions());
                m_Log.Verbose($"Discovered {actions.Count - oldLen} actions from {actionDiscoverer.GetType().FullName}");
            }

            m_DiscoveredActions = actions;
        }
    }
}
