using System.Linq;
using UnityEngine;

namespace RegressionGames.Unity.Automation
{
    /// <summary>
    /// A bot that just randomly activates actions on each tick.
    /// </summary>
    [AddComponentMenu("Regression Games/Bots/Monkey Bot")]
    public class MonkeyBot: Bot
    {
        private readonly Logger<MonkeyBot> m_Log;

        public int activationIntervalInSeconds = 5;

        private float m_LastActivation;

        public MonkeyBot()
        {
            m_Log = Logger.For(this);
        }

        protected override void Awake()
        {
            base.Awake();
            m_LastActivation = Time.time;

            // Request a screenshot on our first frame.
            AutomationController.RequestScreenshot(0);
        }

        private void Update()
        {
            if(Time.time - m_LastActivation < activationIntervalInSeconds)
            {
                return;
            }
            m_LastActivation = Time.time;

            m_Log.Verbose($"Activating at {Time.time}");

            // TODO: This allocates a new list each frame, which isn't ideal.
            var availableActions = AutomationController.Entities
                .SelectMany(e => e.Actions.Values)
                .Where(a => a.CanActivateThisFrame())
                .ToList();

            if (availableActions.Count == 0)
            {
                m_Log.Verbose("No actions available this frame.");
                return;
            }

            // Select a random action and activate it.
            var action = availableActions[Random.Range(0, availableActions.Count)];
            action.Activate();

            // Request a screenshot next frame
            AutomationController.RequestScreenshot(delayInFrames: 1);
        }
    }
}
