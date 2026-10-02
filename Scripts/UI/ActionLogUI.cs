using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Panahon.Core;

namespace Panahon.UI
{
    public class ActionLogUI : MonoBehaviour
    {
        [Header("Log UI")]
        [SerializeField] private Text logContentText;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private int maxLogLines = 25;

        private List<string> logEntries = new List<string>();

        private void Start()
        {
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.OnGameLog += AddLogEntry;
            }
        }

        private void OnDestroy()
        {
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.OnGameLog -= AddLogEntry;
            }
        }

        public void AddLogEntry(string message)
        {
            string timeStamp = System.DateTime.Now.ToString("HH:mm:ss");
            string entry = $"<color=#888888>[{timeStamp}]</color> {message}";

            logEntries.Add(entry);
            if (logEntries.Count > maxLogLines)
            {
                logEntries.RemoveAt(0);
            }

            if (logContentText != null)
            {
                logContentText.text = string.Join("\n", logEntries);
            }

            if (scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }
    }
}
