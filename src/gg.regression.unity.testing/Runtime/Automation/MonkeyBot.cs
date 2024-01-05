using System;
using System.Security.Cryptography;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RegressionGames.Unity.Automation
{
    /// <summary>
    /// A bot that just randomly activates actions on each tick.
    /// </summary>
    [AddComponentMenu("Regression Games/Bots/Monkey Bot")]
    public class MonkeyBot: Bot
    {
        private Logger<MonkeyBot> m_Log;
        private float m_LastActivated = 0;

        [Tooltip("The bot will wait this many seconds in between each attempt to activate an action.")]
        public float activationInterval = 5;

        public MonkeyBot()
        {
            m_Log = Logger.For(this);
        }

        protected internal override void Execute(BotContext context)
        {
            // Don't run unless enough time has passed, we don't want to spam.
            var timeSinceLast = Time.time - m_LastActivated;
            if (timeSinceLast < activationInterval)
            {
                return;
            }

            m_LastActivated = Time.time;
            m_Log.Verbose($"Activating at {Time.time}");

            if (context.AvailableActions.Count == 0)
            {
                // Nothing to do if there's nothing to do!
                return;
            }

            var action = context.AvailableActions[Random.Range(0, context.AvailableActions.Count)];
            action.Activate();
        }
    }
}
