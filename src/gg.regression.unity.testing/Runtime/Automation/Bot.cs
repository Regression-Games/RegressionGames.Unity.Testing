using UnityEngine;

namespace RegressionGames.Unity.Automation
{
    /// <summary>
    /// Represents a bot that can be spawned by an <see cref="AutomationController"/>
    /// </summary>
    public abstract class Bot: MonoBehaviour
    {
        protected internal abstract void Execute(BotContext context);
    }
}
