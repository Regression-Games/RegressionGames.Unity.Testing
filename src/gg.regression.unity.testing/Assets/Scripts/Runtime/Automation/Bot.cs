using System;

namespace RegressionGames.Unity.Automation
{
    /// <summary>
    /// Represents a bot that can be spawned by an <see cref="AutomationController"/>
    /// </summary>
    public abstract class Bot: AutomationBehavior
    {
        private Guid m_InstanceId;

        /// <summary>
        /// Gets the instance ID of this bot, which is assigned by the <see cref="AutomationController"/> when it is spawned.
        /// </summary>
        public Guid InstanceId => m_InstanceId;

        /// <summary>
        /// Gets the state of the bot.
        /// </summary>
        public virtual BotState State => gameObject.activeInHierarchy ? BotState.Running : BotState.Stopped;

        internal void Initialize(Guid instanceId)
        {
            m_InstanceId = instanceId;
        }

        public virtual void StopBot()
        {
            gameObject.SetActive(false);
        }

        public virtual void StartBot()
        {
            gameObject.SetActive(true);
        }
    }
}
