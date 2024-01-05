using System.Collections.Generic;

namespace RegressionGames.Unity.Automation
{
    /// <summary>
    /// Context information provided to a bot when it is executed.
    /// </summary>
    public struct BotContext
    {
        /// <summary>
        /// A list of all the <see cref="IAutomationAction"/>s that can be activated this frame.
        /// </summary>
        public IReadOnlyList<IAutomationAction> AvailableActions { get; }

        /// <summary>
        /// A list of all <see cref="IAutomationAction"/>s discovered in the scene, including those that cannot be activated.
        /// </summary>
        public IReadOnlyList<IAutomationAction> AllDiscoveredActions { get; }

        public BotContext(IReadOnlyList<IAutomationAction> availableActions, IReadOnlyList<IAutomationAction> allDiscoveredActions)
        {
            AvailableActions = availableActions;
            AllDiscoveredActions = allDiscoveredActions;
        }
    }
}
