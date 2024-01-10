using UnityEngine;

namespace RegressionGames.Unity.Recording
{
    public class RecordingSession
    {
        private readonly Logger m_Log;
        private readonly RecorderWorker m_RecorderWorker;
        private readonly AutomationRecorder m_Recorder;
        private readonly string m_SessionId;
        private readonly string m_SessionDirectory;

        internal RecordingSession(AutomationRecorder recorder, string sessionId, string sessionDirectory)
        {
            m_Log = Logger.For(typeof(RecordingSession).FullName);
            m_Recorder = recorder;
            m_SessionId = sessionId;
            m_SessionDirectory = sessionDirectory;

            // Spawn the recorder background thread.
            m_RecorderWorker = new RecorderWorker(m_SessionDirectory);
            m_RecorderWorker.Start();
        }

        public void Stop()
        {
            m_Recorder.StopSession(this);
            UnsafeStopFromRecorder();
        }

        /// <summary>
        /// Records a frame to the session.
        /// </summary>
        /// <param name="screenshot">An array of bytes representing the screenshot of the frame.</param>
        /// <param name="snapshot"></param>
        public void Record(byte[] screenshot, FrameSnapshot snapshot)
        {
            m_RecorderWorker.Enqueue(screenshot, snapshot);
        }

        /// <summary>
        /// Stops the session without notifying the recorder.
        /// This is done to avoid issues when the session is stopped by the recorder itself.
        /// </summary>
        internal void UnsafeStopFromRecorder()
        {
            m_RecorderWorker.Stop();
            m_Log.Info($"Recording stopped. Data is available in {m_SessionDirectory}");
        }
    }
}
