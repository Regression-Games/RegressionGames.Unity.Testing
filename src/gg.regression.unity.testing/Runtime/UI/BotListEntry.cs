using RegressionGames.Unity.Automation;
using RegressionGames.Unity.Recording;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RegressionGames.Unity.UI
{
    [AddComponentMenu("Regression Games/Internal/Bot List Entry")]
    internal class BotListEntry: MonoBehaviour
    {
        private int m_MarkedFrame;
        private RecordingSession m_RecordingSession;

        public OverlayMenu overlayMenu;
        public TMP_Text botNameText;
        public Button stopButton;
        public Button destroyButton;

        [HideInInspector]
        public Bot bot;

        public void Populate(Bot activeBot, RecordingSession recordingSession)
        {
            Mark();

            bot = activeBot;
            m_RecordingSession = recordingSession;

            var state = activeBot.gameObject.activeInHierarchy ? "Running" : "Stopped";
            botNameText.text = $"{activeBot.name} ({state})";

            stopButton.gameObject.SetActive(activeBot.gameObject.activeInHierarchy);
            destroyButton.gameObject.SetActive(!activeBot.gameObject.activeInHierarchy);
        }

        public void Mark() => m_MarkedFrame = Time.frameCount;
        public bool IsMarked() => m_MarkedFrame == Time.frameCount;

        public void OnStopButtonClick()
        {
            overlayMenu.StopBot(bot);
        }

        public void OnDestroyButtonClick()
        {
            overlayMenu.DestroyBot(bot);
        }
    }
}
