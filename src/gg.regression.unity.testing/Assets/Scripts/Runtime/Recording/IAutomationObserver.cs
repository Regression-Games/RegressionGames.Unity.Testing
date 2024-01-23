using RegressionGames.Unity.Automation;

namespace RegressionGames.Unity.Recording
{
    /// <summary>
    /// Provides an interface to an automation observer, which is capable of observing actions happening throughout the automation session.
    /// </summary>
    public interface IAutomationObserver
    {
        void FrameComplete(AutomationController automationController);
    }
}
