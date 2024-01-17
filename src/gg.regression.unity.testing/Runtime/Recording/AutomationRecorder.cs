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

        private HashSet<int> m_ScreenshotRequests = new HashSet<int>();

        [SerializeField]
        [Tooltip(
            "The directory in which recordings will be saved. A relative path will be considered relative to the persistent data path. If not specified, a default directory will be used.")]
        private string recordingDirectory;

        public AutomationRecorder()
        {
            m_Log = Logger.For(this);
        }

        /// <summary>
        /// Requests that a screenshot be taken in the specified number of frames.
        /// </summary>
        /// <param name="delayInFrames">The number of frames to wait before taking a screenshot. Defaults to '0' which means a screenshot will be recorded THIS frame.</param>
        public void RequestScreenshot(int delayInFrames = 0)
        {
            m_ScreenshotRequests.Add(Time.frameCount + delayInFrames);
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

            // Take a screenshot if someone requested it.
            var screenshotBytes = TakeScreenshotIfRequested();

            // Send it to all active sessions
            foreach (var session in m_ActiveSessions)
            {
                session.Record(screenshotBytes, snapshot);
            }
        }

        private byte[] TakeScreenshotIfRequested()
        {
            if (m_ScreenshotRequests.Remove(Time.frameCount))
            {
                var texture = ScreenCapture.CaptureScreenshotAsTexture();
                return texture.EncodeToPNG();
            }

            return null;
        }

        internal void StopSession(RecordingSession session)
        {
            m_ActiveSessions.Remove(session);
        }

#if UNITY_EDITOR
        [ContextMenu("Open Recording Directory")]
        public void OpenRecordingsDirectory()
        {
            // Find the recorder in the active scene
            var recorder = FindObjectOfType<AutomationRecorder>();
            if (recorder == null)
            {
                EditorUtility.DisplayDialog(
                    "No Recorder Found",
                    "No AutomationRecorder component was found in the active scene.",
                    "OK");
                return;
            }

            var recordingDir = recorder.GetRecordingDirectory();
            if(string.IsNullOrEmpty(recordingDir))
            {
                EditorUtility.DisplayDialog(
                    "No Recording Directory",
                    "The AutomationRecorder component in the active scene does not have a recording directory specified.",
                    "OK");
                return;
            }

            // Open the directory
            EditorUtilities.OpenFileBrowser(recordingDir);
        }
#endif
    }
}
