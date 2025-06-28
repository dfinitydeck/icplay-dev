using System.Collections.Generic;
using System;
using UnityEngine;

namespace Framework
{
    public class LogManager : SingletonManager<LogManager>, IModuleInterface
    {
        private List<String> logEntries = new List<string>();
        private bool isShowLog = false;
        private Rect logRect;
        private Vector2 scrollPosition = Vector2.zero;

        public void Init(Action<bool> onInitEnd)
        {
            int left = Screen.width / 10;
            int top = Screen.height / 10;
            logRect = new Rect(left, top, Screen.width - left * 2, Screen.height - top * 2);
            onInitEnd?.Invoke(true);
        }

        public void Run(Action<bool> onRunEnd)
        {
            Application.logMessageReceived += HandleLog;
            onRunEnd?.Invoke(true);
        }

        private void HandleLog(string logString, string stackTrace, UnityEngine.LogType type)
        {
            if (type == UnityEngine.LogType.Error || type == UnityEngine.LogType.Exception || type == UnityEngine.LogType.Assert)
            {
                isShowLog = false;
                return;
            }
        }

        private void OnGUI()
        {
            if (isShowLog)
            {
                logRect = GUILayout.Window(123456, logRect, LogWindow, "Exception");
            }
        }

        private void LogWindow(int windowID)
        {
            // GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);

            GUILayout.BeginVertical();
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            for (int i = 0; i < logEntries.Count; i++)
            {
                GUILayout.Label(logEntries[i]);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear", GUILayout.MaxWidth(150)))
            {
                logEntries.Clear();
                isShowLog = false;
            }
            if (GUILayout.Button("Close", GUILayout.MaxWidth(150)))
            {
                isShowLog = false;
            }
            GUILayout.EndHorizontal();
        }
    }
}