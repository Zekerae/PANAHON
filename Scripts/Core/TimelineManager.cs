using System;
using UnityEngine;

namespace Panahon.Core
{
    public class TimelineManager : MonoBehaviour
    {
        public static TimelineManager Instance { get; private set; }

        [Header("Shared Timeline Stability")]
        [Range(0, 10)]
        [SerializeField] private int timelineMeter = 0;
        [SerializeField] private int maxTimelineLimit = 10;
        [SerializeField] private int criticalWarningThreshold = 7;

        public int CurrentTimelineMeter => timelineMeter;
        public int MaxTimelineLimit => maxTimelineLimit;
        public float StabilityPercentage => (float)timelineMeter / maxTimelineLimit;
        public bool IsCritical => timelineMeter >= criticalWarningThreshold;
        public bool HasCollapsed => timelineMeter >= maxTimelineLimit;

        public event Action<int, string> OnTimelineChanged;
        public event Action OnTimelineCritical;
        public event Action OnTimelineCollapse;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void ResetMeter()
        {
            timelineMeter = 0;
            OnTimelineChanged?.Invoke(timelineMeter, "Timeline reset to stable nexus.");
        }

        public void IncreaseDestabilization(int amount, string reason = "")
        {
            if (HasCollapsed) return;

            timelineMeter = Mathf.Clamp(timelineMeter + amount, 0, maxTimelineLimit);
            Debug.Log($"[TimelineManager] Timeline destabilization increased by {amount} ({reason}). Current: {timelineMeter}/{maxTimelineLimit}");
            OnTimelineChanged?.Invoke(timelineMeter, reason);

            if (timelineMeter >= criticalWarningThreshold)
            {
                OnTimelineCritical?.Invoke();
            }

            if (timelineMeter >= maxTimelineLimit)
            {
                Debug.LogWarning("[TimelineManager] CRITICAL FAILURE: Shared Timeline Meter reached maximum! Timeline has collapsed!");
                OnTimelineCollapse?.Invoke();
            }
        }

        public void StabilizeTimeline(int amount, string reason = "")
        {
            if (HasCollapsed) return;

            timelineMeter = Mathf.Clamp(timelineMeter - amount, 0, maxTimelineLimit);
            Debug.Log($"[TimelineManager] Timeline stabilized by {amount} ({reason}). Current: {timelineMeter}/{maxTimelineLimit}");
            OnTimelineChanged?.Invoke(timelineMeter, reason);
        }
    }
}
