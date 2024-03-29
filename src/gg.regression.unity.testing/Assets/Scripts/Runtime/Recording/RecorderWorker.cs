using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RegressionGames.Unity.Recording
{
    internal class RecorderWorker
    {
        private readonly RecordingInfo m_Info;
        private readonly string m_SessionName;
        private readonly string m_SessionDirectory;
        private readonly bool m_SaveOnlyOnChanged;
        private readonly Thread m_RecorderThread;
        private readonly CancellationTokenSource m_StopRequested;
        private readonly BlockingCollection<FrameRecordAction> m_Work = new(new ConcurrentQueue<FrameRecordAction>());

        private FrameSnapshot m_LastSnapshot;

        public bool IsRunning => !m_StopRequested.IsCancellationRequested;

        public RecorderWorker(RecordingInfo info, string sessionDirectory, bool saveOnlyOnChanged)
        {
            m_Info = info;
            m_SessionDirectory = sessionDirectory;
            m_SaveOnlyOnChanged = saveOnlyOnChanged;

            // TODO: Multiple sessions could share a thread, since they record the same data.
            m_RecorderThread = new Thread(() =>
            {
                try
                {
                    Main();
                }
                catch (OperationCanceledException)
                {
                    // OCE is not an error, it's just a signal that we should stop.
                }
            })
            {
                Name = $"{GetType().FullName} - Session {info.id}",
            };
            m_StopRequested = new CancellationTokenSource();
        }

        public void Start()
        {
            m_RecorderThread.Start();
        }

        public void Stop()
        {
            m_Work.CompleteAdding();
            m_StopRequested.Cancel();
        }

        public void Enqueue(byte[] screenshot, FrameSnapshot snapshot)
        {
            if (m_StopRequested.IsCancellationRequested)
            {
                // Don't enqueue any more work if we're already stopping.
                return;
            }

            // The screenshot must be encoded to PNG on the main thread.
            m_Work.Add(new(screenshot, snapshot));
        }

        void Main()
        {
            if (!Directory.Exists(m_SessionDirectory))
            {
                Directory.CreateDirectory(m_SessionDirectory);
            }

            // Record the recording metadata file
            var infoJson = RegressionGamesJsonFormat.Serialize(m_Info);
            var infoPath = Path.Combine(m_SessionDirectory, "recording.json");
            File.WriteAllText(infoPath, infoJson);

            while (!m_StopRequested.IsCancellationRequested)
            {
                var action = m_Work.Take(m_StopRequested.Token);
                PerformAction(action);
            }
        }

        private void PerformAction(FrameRecordAction action)
        {
            // Save the screenshot to a PNG file
            var savePngTask = Task.CompletedTask;
            if (action.ScreenshotBytes is { } pngBuf)
            {
                var outputPng = Path.Combine(m_SessionDirectory, $"screenshot.{action.Snapshot.frame.frameCount}.png");
                savePngTask = File.WriteAllBytesAsync(outputPng, pngBuf);
            }

            // Check if the entities in the new snapshot differ from the previous snapshot.
            // If not, don't bother saving this frame.
            // We can't just check the frame snapshot itself, because the frame number _always_ changes.
            var saveJsonTask = Task.CompletedTask;
            if (ShouldSaveSnapshot(action))
            {
                // Serialize the snapshot to JSON and save it
                var outputJson = Path.Combine(m_SessionDirectory, $"snapshot.{action.Snapshot.frame.frameCount}.json");
                var jsonBuf = RegressionGamesJsonFormat.Serialize(action.Snapshot);
                saveJsonTask = File.WriteAllTextAsync(outputJson, jsonBuf);
            }

            // Remember this snapshot to diff it to the next one.
            m_LastSnapshot = action.Snapshot;

            // Wait for both tasks to complete
            Task.WaitAll(savePngTask, saveJsonTask);
        }

        private bool ShouldSaveSnapshot(FrameRecordAction action)
        {
            if (!m_SaveOnlyOnChanged)
            {
                return true;
            }

            if (m_LastSnapshot == null)
            {
                return true;
            }

            return action.Snapshot.HasChangesFrom(m_LastSnapshot);
        }

        internal class FrameRecordAction
        {
            public byte[] ScreenshotBytes;
            public FrameSnapshot Snapshot;

            public FrameRecordAction(byte[] screenshotBytes, FrameSnapshot snapshot)
            {
                ScreenshotBytes = screenshotBytes;
                Snapshot = snapshot;
            }
        }
    }
}
