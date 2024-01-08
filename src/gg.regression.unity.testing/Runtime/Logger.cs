using System;
using UnityEngine;

namespace RegressionGames.Unity
{
    internal interface ILogger
    {
        void Info(string message);
        void Warning(string message);
        void Error(string message);
        void Exception(Exception ex);
    }

    internal readonly struct Logger: ILogger
    {
        private readonly string m_Category;

        public Logger(string category)
        {
            m_Category = category ?? "RegressionGames.Unity";
        }

        public static Logger For(string category)
        {
            return new Logger(category);
        }

        public static Logger<T> For<T>(T instance) where T: UnityEngine.Object
        {
            return new Logger<T>(instance);
        }

        public void Info(string message) => Debug.Log(FormatMessage(message));
        public void Warning(string message) => Debug.LogWarning(FormatMessage(message));
        public void Error(string message) => Debug.LogError(FormatMessage(message));
        public void Exception(Exception ex) => Debug.LogException(ex);

        public void Info(string message, UnityEngine.Object? context) => Debug.Log(FormatMessage(message), context);
        public void Warning(string message, UnityEngine.Object? context) => Debug.LogWarning(FormatMessage(message), context);
        public void Error(string message, UnityEngine.Object? context) => Debug.LogError(FormatMessage(message), context);
        public void Exception(Exception ex, UnityEngine.Object? context) => Debug.LogException(ex, context);

        // TODO: Make this filterable.
        public void Verbose(string message) => Debug.Log(FormatMessage(message));
        public void Verbose(string message, UnityEngine.Object context) => Debug.Log(FormatMessage(message), context);

        string FormatMessage(string message) => $"[{m_Category}] {message}";
    }

    internal readonly struct Logger<T>: ILogger where T : UnityEngine.Object
    {
        private readonly T m_Instance;
        private readonly string m_Category;

        public Logger(T instance)
        {
            m_Instance = instance;
            m_Category = instance.GetType().FullName ?? "RegressionGames.Unity";
        }

        public void Info(string message) => Debug.Log(FormatMessage(message), m_Instance);
        public void Warning(string message) => Debug.LogWarning(FormatMessage(message), m_Instance);
        public void Error(string message) => Debug.LogError(FormatMessage(message), m_Instance);
        public void Exception(Exception ex) => Debug.LogException(ex, m_Instance);

        public void Info(string message, UnityEngine.Object? context) => Debug.Log(FormatMessage(message), context);
        public void Warning(string message, UnityEngine.Object? context) => Debug.LogWarning(FormatMessage(message), context);
        public void Error(string message, UnityEngine.Object? context) => Debug.LogError(FormatMessage(message), context);
        public void Exception(Exception ex, UnityEngine.Object? context) => Debug.LogException(ex, context);

        // TODO: Make this filterable.
        public void Verbose(string message) => Debug.Log(FormatMessage(message), m_Instance);
        public void Verbose(string message, UnityEngine.Object? context) => Debug.Log(FormatMessage(message), context);

        string FormatMessage(string message) => $"[{m_Category}] {message}";
    }
}
