using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using RegressionGames.Unity.Automation;
using UnityEditor;
using UnityEngine;

namespace RegressionGames.Unity.Recording
{
    /// <summary>
    /// A component that, when placed under an <see cref="AutomationController"/>, can be used to record automation actions.
    /// </summary>
    [AddComponentMenu("Regression Games/Automation Recorder")]
    public class AutomationRecorder : AutomationBehavior
    {
        private readonly Logger<AutomationRecorder> m_Log;
        private readonly List<RecordingSession> m_ActiveSessions = new();

        [SerializeField]
        [Tooltip(
            "The directory in which recordings will be saved. A relative path will be considered relative to the persistent data path. If not specified, a default directory will be used.")]
        private string recordingDirectory;

        private void Start()
        {
        }

        public AutomationRecorder()
        {
            m_Log = Logger.For(this);
        }

        public RecordingSession StartRecordingSession(string name)
        {
            // Generate a session ID for this recording.
            var sessionId = Guid.NewGuid();
            var sessionDirectory = Path.Combine(
                GetRecordingDirectory(),
                sessionId.ToString("N"));
            var archivePath = Path.Combine(
                GetRecordingDirectory(),
                $"{sessionId:N}.rgrec.zip");
            var session = new RecordingSession(this, sessionId, name, sessionDirectory, archivePath);
            m_Log.Info($"Starting recording session {sessionId:N}. Recording to {sessionDirectory}");
            m_ActiveSessions.Add(session);
            return session;
        }

        public string GetRecordingDirectory()
        {
            var dir = recordingDirectory;
            if (string.IsNullOrEmpty(dir))
            {
                dir = Path.Combine(
                    Application.persistentDataPath,
                    "RegressionGames",
                    "Recordings");
            }
            else if (!Path.IsPathRooted(dir))
            {
                dir = Path.Combine(Application.persistentDataPath, dir);
            }

            return dir;
        }

        private void OnDestroy()
        {
            foreach (var session in m_ActiveSessions)
            {
                session.UnsafeStopFromRecorder();
            }

            m_ActiveSessions.Clear();
        }

        private void LateUpdate()
        {
            // Use a Unity Coroutine to ensure we run at the very end of the frame.
            StartCoroutine(RecordFrame());
        }

        IEnumerator RecordFrame()
        {
            yield return new WaitForEndOfFrame();

            // If there are no active sessions, we can skip the rest of this.
            if (m_ActiveSessions.Count == 0)
            {
                yield break;
            }

            // Create a frame snapshot
            var snapshot = FrameSnapshot.Create(
                FrameInfo.ForCurrentFrame(),
                AutomationController.Entities);

            // Take a screenshot
            // TODO: Make this optional, per session. Then only capture the screenshot if an active session wants it.
            var texture = ScreenCapture.CaptureScreenshotAsTexture();
            var screenshotBytes = texture.EncodeToPNG();

            // Send it to all active sessions
            foreach (var session in m_ActiveSessions)
            {
                session.Record(screenshotBytes, snapshot);
            }
        }

        internal void StopSession(RecordingSession session)
        {
            m_ActiveSessions.Remove(session);
        }
    }
}
