using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RegressionGames.Unity.Automation;
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

        public AutomationRecorder()
        {
            m_Log = Logger.For(this);
        }

        protected override void Awake()
        {
            base.Awake();

            // TODO: Don't start recording automatically. We should have a way to start/stop recording from the UI.
            StartRecordingSession();
        }

        public RecordingSession StartRecordingSession()
        {
            // Generate a session ID for this recording.
            var sessionId = System.Guid.NewGuid().ToString();
            var sessionDirectory = Path.Combine(
                Application.persistentDataPath,
                "RegressionGames",
                "Recordings",
                sessionId);
            var session = new RecordingSession(this,sessionId, sessionDirectory);
            m_Log.Info($"Starting recording session {sessionId}. Recording to {sessionDirectory}");
            m_ActiveSessions.Add(session);
            return session;
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
