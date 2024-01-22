using System;
using System.IO.Compression;
using UnityEngine.Windows;

namespace RegressionGames.Unity.Recording
{
    public class RecordingSession
    {
        private readonly Logger m_Log;
        private readonly RecorderWorker m_RecorderWorker;
        private readonly AutomationRecorder m_Recorder;
        private readonly Guid m_Id;
        private readonly string m_Name;
        private readonly string m_Directory;
        private readonly string m_ArchivePath;
        private bool m_RecordingSaved;

        /// <summary>
        /// Gets the ID of the recording session.
        /// </summary>
        public Guid Id => m_Id;

        /// <summary>
        /// Gets the directory in which data from the recording is being written.
        /// When the recording has stopped, this directory may be deleted and the data moved to <see cref="ArchivePath"/>.
        /// </summary>
        public string Directory => m_Directory;

        /// <summary>
        /// Gets the path to the '.zip' archive containing all the data from the recording.
        /// This will be null until the recording has stopped and saved to the archive.
        /// </summary>
        public string ArchivePath => m_RecordingSaved ? m_ArchivePath : null;

        /// <summary>
        /// Gets a boolean indicating if the recording is running or if it has stopped.
        /// </summary>
        public bool IsRecording => m_RecorderWorker.IsRunning;

        internal RecordingSession(AutomationRecorder recorder, Guid id, string name, string directory, string archivePath, bool saveOnlyOnChanged)
        {
            m_Log = Logger.For(typeof(RecordingSession).FullName);
            m_Recorder = recorder;
            m_Id = id;
            m_Name = name;
            m_Directory = directory;
            m_ArchivePath = archivePath;

            // Spawn the recorder background thread.
            m_RecorderWorker = new RecorderWorker(m_Id.ToString("N"), m_Name, m_Directory, saveOnlyOnChanged);
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
        /// "Unsafe" because it should only be used by the recorder.
        /// </summary>
        internal void UnsafeStopFromRecorder()
        {
            m_RecorderWorker.Stop();

            // Save the recording to a new archive at m_ArchivePath
            if (File.Exists(m_ArchivePath))
            {
                m_Log.Warning($"Recording archive {m_ArchivePath} already exists. Deleting.");
                File.Delete(m_ArchivePath);
            }
            ZipFile.CreateFromDirectory(m_Directory, m_ArchivePath);

            // Delete the session directory
            UnityEngine.Windows.Directory.Delete(m_Directory);
            m_RecordingSaved = true;
            m_Log.Info($"Recording complete. Archive saved to {m_Directory}.");
        }
    }
}
