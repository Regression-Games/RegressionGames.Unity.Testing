using System;

namespace RegressionGames.Unity.Recording
{
    /// <summary>
    /// Provides high level metadata about a recording.
    /// </summary>
    [Serializable]
    public struct RecordingInfo
    {
        /// <summary>
        /// The ID of the recording session.
        /// </summary>
        public string id;

        /// <summary>
        /// The name of the recording session.
        /// </summary>
        public string name;

        /// <summary>
        /// The version of the recording format.
        /// </summary>
        public int version;

        /// <summary>
        /// The name of the machine on which the recording was made.
        /// </summary>
        public string machineName;

        /// <summary>
        /// The name of the user who made the recording.
        /// </summary>
        public string userName;

        /// <summary>
        /// The time at which the recording was started, in ISO 8601 format with time zone offset.
        /// </summary>
        public string startTime;

        public static RecordingInfo Create(string id, string name)
        {
            return new()
            {
                id = id,
                name = name,
                version = 1,
                machineName = Environment.MachineName,
                userName = Environment.UserName,
                startTime = DateTimeOffset.Now.ToString("O"),
            };
        }
    }
}
