using System;
using UnityEngine;

namespace RegressionGames.Unity.Automation
{
    [Serializable]
    public class BotConfiguration
    {
        [Tooltip("The bot to spawn.")]
        public Bot bot;

        [Tooltip("The number of frames between activations.")]
        public int activationIntervalInFrames;
    }
}
