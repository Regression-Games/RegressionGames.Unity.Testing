using System;
using System.Collections.Generic;
using System.Linq;
using RegressionGames.Unity.Automation;
using RegressionGames.Unity.Recording;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace RegressionGames.Unity.UI
{
    [AddComponentMenu("Regression Games/Internal/Overlay Menu")]
    internal class OverlayMenu: MonoBehaviour
    {
        private bool m_LoadedAutomationController;
        private AutomationController m_AutomationController;
        private readonly Dictionary<Guid, BotListEntry> m_BotListEntries = new();
        private readonly Dictionary<Guid, RecordingSession> m_RecordingSessionsByBotInstance = new();
        private readonly Logger<OverlayMenu> m_Log;

        public GameObject overlayPanel;
        public GameObject botListRoot;
        public GameObject botListEntryPrefab;
        public TMP_Dropdown nextBotDropdown;

        public OverlayMenu()
        {
            m_Log = Logger.For(this);
        }

        private void Start()
        {
            // Hide to start with.
            overlayPanel.SetActive(false);
        }

        public void OnOverlayButtonClick()
        {
            UpdateUI();
            overlayPanel.SetActive(true);
        }

        public void OnCloseButtonClick()
        {
            overlayPanel.SetActive(false);
        }

        public void OnStartBotClick()
        {
            var automationController = GetAutomationController();
            if (automationController == null)
            {
                return;
            }

            var bot = automationController.availableBots[nextBotDropdown.value];
            if (bot == null)
            {
                return;
            }

            // Spawn the bot
            var botInstance = automationController.Spawn(bot);
            botInstance.StartBot();

            // TODO: Allow customizing this. We could make it optional to record the bot, and could allow recording sessions that aren't dependent upon bots.
            // Start recording the bot, if there's a recorder
            var automationRecorder = FindObjectOfType<AutomationRecorder>();
            if (automationRecorder != null && !m_RecordingSessionsByBotInstance.ContainsKey(botInstance.InstanceId))
            {
                var date = DateTimeOffset.Now.ToString("s");
                var session = automationRecorder.StartRecordingSession($"Auto-Recording for Bot {botInstance.InstanceId} at {date}");
                m_RecordingSessionsByBotInstance.Add(botInstance.InstanceId, session);
            }
        }

        public void StopBot(Bot bot)
        {
            if (m_RecordingSessionsByBotInstance.TryGetValue(bot.InstanceId, out var session))
            {
                // Stop the recording session
                session.Stop();
                m_RecordingSessionsByBotInstance.Remove(bot.InstanceId);
            }

            // Stop the bot.
            bot.StopBot();
        }

        public void DestroyBot(Bot bot)
        {
#if UNITY_EDITOR
            // Add an extra confirmation step in the editor, to avoid accidentally destroying bots.
            var automationRecorder = FindObjectOfType<AutomationRecorder>();
            var message = automationRecorder == null
                ? $"Are you sure you want to destroy bot '{bot.name}'?"
                : $"Are you sure you want to destroy bot '{bot.name}'? Any recordings will be preserved in {automationRecorder.GetRecordingDirectory()}";
            if (!EditorUtility.DisplayDialog("Destroy Bot", message, "Yes", "No"))
            {
                return;
            }
#endif

            if (m_BotListEntries.TryGetValue(bot.InstanceId, out var entry))
            {
                Destroy(entry.gameObject);
                m_BotListEntries.Remove(bot.InstanceId);
            }

            Destroy(bot.gameObject);
        }

        private void LateUpdate()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (!overlayPanel.activeInHierarchy)
            {
                return;
            }

            // Locate the automation controller
            var automationController = GetAutomationController();
            if (automationController == null)
            {
                return;
            }

            // Build a list of available bots
            var availableBots = automationController.availableBots
                .Select(b => new TMP_Dropdown.OptionData(b.name))
                .OrderBy(s => s)
                .ToList();
            nextBotDropdown.options = availableBots;

            // Update the list of active bots
            var bots = automationController.GetAllBots();
            foreach (var bot in bots)
            {
                if (!m_BotListEntries.TryGetValue(bot.InstanceId, out var entry))
                {
                    var newEntry = Instantiate(botListEntryPrefab, botListRoot.transform);
                    entry = newEntry.GetComponent<BotListEntry>();
                    entry.overlayMenu = this;
                    m_BotListEntries[bot.InstanceId] = entry;
                }

                var recordingSession = m_RecordingSessionsByBotInstance.TryGetValue(bot.InstanceId, out var session)
                    ? session
                    : null;

                entry.Populate(bot, recordingSession);
            }

            // Remove any unmarked entries
            foreach (Transform child in botListRoot.transform)
            {
                var entry = child.GetComponent<BotListEntry>();
                if (!entry.IsMarked())
                {
                    m_BotListEntries.Remove(entry.bot.InstanceId);
                    Destroy(child.gameObject);
                }
            }

            // Update the layout for the entries
            // TODO: Should probably sort these for consistency?
            var entries = botListRoot.GetComponentsInChildren<BotListEntry>();
            for(int i = 0; i < entries.Length; i++)
            {
                var rt = entries[i].GetComponent<RectTransform>();
                var position = new Vector3(0f, rt.rect.height * -i, 0f);
                entries[i].transform.localPosition = position;
            }
        }

        private AutomationController GetAutomationController()
        {
            if (!m_LoadedAutomationController)
            {
                m_LoadedAutomationController = true;
                m_AutomationController = FindObjectOfType<AutomationController>();
                if (m_AutomationController == null)
                {
                    m_Log.Warning("No automation controller found in scene.");
                }
            }

            return m_AutomationController;
        }
    }
}
