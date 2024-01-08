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
        public ActionDiscoverer[] actionDiscoverers;

        [Tooltip("By default, action discovery happens once when the controller is spawned. If this is true, actions will be discovered every frame, during Update")]
        public bool discoverEveryFrame;

        [Tooltip("Bots that should be spawned as soon as the Automation Controller awakens in the scene.")]
        public BotConfiguration[] initialBots;

        private List<IAutomationAction> m_DiscoveredActions;
        private readonly List<BotInstance> m_ActiveBots = new();

        private readonly Logger<AutomationController> m_Log;

        public AutomationController()
        {
            m_Log = Logger.For(this);
        }

        private void Awake()
        {
            DiscoverActions();

            foreach (var configuration in initialBots ?? Array.Empty<BotConfiguration>())
            {
                var spawned = Instantiate(configuration.bot, transform);
                m_ActiveBots.Add(new(spawned, configuration.activationIntervalInFrames, Time.frameCount));
            }
        }

        private void OnDestroy()
        {
            // Bots we spawned belong to us. Destroy them when we're destroyed.
            foreach (var bot in m_ActiveBots)
            {
                Destroy(bot.instance);
            }
            m_ActiveBots.Clear();
        }

        private void Update()
        {
            // A memoized function to scan for available actions.
            // This will only be called if a bot is going to run this frame.
            // But once it's called, we'll save the result so we don't have to scan again this frame.
            IAutomationAction[] availableActions = null;
            IReadOnlyList<IAutomationAction> GetAvailableActions()
            {
                if (availableActions is null)
                {
                    // Discover new actions, if requested.
                    if (discoverEveryFrame)
                    {
                        DiscoverActions();
                    }

                    availableActions = m_DiscoveredActions == null
                        ? Array.Empty<IAutomationAction>()
                        : m_DiscoveredActions.Where(a => a.CanActivateThisFrame()).ToArray();
                    m_Log.Verbose($"Discovered {availableActions.Length} available actions this frame");
                }

                return availableActions;
            }

            // Run through all the bots and give them the actions they can perform
            foreach (var bot in m_ActiveBots)
            {
                if (Time.frameCount - bot.lastActivation < bot.activationIntervalInFrames)
                {
                    continue;
                }

                m_Log.Verbose($"Activating bot {bot.GetType().FullName} on frame {Time.frameCount}");
                bot.lastActivation = Time.frameCount;

                var botContext = new BotContext(GetAvailableActions(), m_DiscoveredActions);

                // Don't allow one bot to stop another by throwing
                try
                {
                    bot.instance.Execute(botContext);
                }
                catch (Exception ex)
                {
                    // TODO: Deactivate the bot if it keeps throwing?
                    m_Log.Exception(ex, bot.instance);
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

        class BotInstance
        {
            public Bot instance;
            public int activationIntervalInFrames;
            public int lastActivation;

            public BotInstance(Bot instance, int activationIntervalInFrames, int lastActivation)
            {
                this.instance = instance;
                this.activationIntervalInFrames = activationIntervalInFrames;
                this.lastActivation = lastActivation;
            }
        }
    }
}
