namespace RegressionGames.Unity.Automation
{
    public interface IAutomationAction
    {
        bool CanActivateThisFrame();
        void Activate();
    }
}
