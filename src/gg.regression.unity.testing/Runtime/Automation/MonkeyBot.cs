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

        public MonkeyBot()
        {
            m_Log = Logger.For(this);
        }

        protected internal override void Execute(BotContext context)
        {
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
