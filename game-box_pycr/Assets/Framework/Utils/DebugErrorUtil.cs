#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
    public class DebugErrorUtil : Singleton<DebugErrorUtil>
    {
        public void LogError(string message)
        {
#if UNITY_EDITOR
            if (EditorUtility.DisplayDialog("Debug Error", message, "OK"))
            {
                if (EditorApplication.isPlaying)
                {
                    EditorApplication.ExitPlaymode();
                }
                return;
            }
#endif
            // Restart game
            // Application.Quit();
        }
    }
}
