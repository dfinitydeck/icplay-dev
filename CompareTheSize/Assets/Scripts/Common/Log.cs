using UnityEngine;

namespace Framework
{
    public class Log:Singleton<Log>
    {
        private const bool m_IsLog = false;
        public static void Info(string log)
        {
            if (!m_IsLog)
                return;
            Debug.Log(log);
        }

        public static void Error(string log)
        {
            if (!m_IsLog)
                return;
            Debug.LogError(log);
        }
        
        public static void Warning(string log)
        {
            if (!m_IsLog)
                return;
            Debug.LogWarning(log);
        }
    }
}