using System.Collections.Generic;
using RegressionGames.Unity.Automation;
using UnityEngine;

namespace RegressionGames.Unity.Discovery
{
    public abstract class ActionDiscoverer: MonoBehaviour
    {
        public abstract IEnumerable<IAutomationAction> DiscoverActions();
    }
}
