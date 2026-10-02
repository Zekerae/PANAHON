using UnityEngine;
using UnityEngine.UI;
using Panahon.Core;

namespace Panahon.UI
{
    public class TimelineMeterUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Slider timelineSlider;
        [SerializeField] private Image fillImage;
        [SerializeField] private Text statusText;
        [SerializeField] private Text meterValueText;
        [SerializeField] private GameObject criticalAlertBadge;

        [Header("Colors")]
        [SerializeField] private Color stableColor = new Color(0.15f, 0.75f, 0.5f);
        [SerializeField] private Color moderateColor = new Color(0.95f, 0.65f, 0.15f);
        [SerializeField] private Color criticalColor = new Color(0.95f, 0.15f, 0.2f);

        private void Start()
        {
            if (TimelineManager.Instance != null)
            {
                TimelineManager.Instance.OnTimelineChanged += UpdateDisplay;
                UpdateDisplay(TimelineManager.Instance.CurrentTimelineMeter, "Timeline Initialized");
            }
        }

        private void OnDestroy()
        {
            if (TimelineManager.Instance != null)
            {
                TimelineManager.Instance.OnTimelineChanged -= UpdateDisplay;
            }
        }

        public void UpdateDisplay(int currentMeter, string reason)
        {
            int max = TimelineManager.Instance != null ? TimelineManager.Instance.MaxTimelineLimit : 10;
            float percent = (float)currentMeter / max;

            if (timelineSlider != null)
            {
                timelineSlider.value = percent;
            }

            if (fillImage != null)
            {
                if (percent < 0.4f) fillImage.color = stableColor;
                else if (percent < 0.7f) fillImage.color = moderateColor;
                else fillImage.color = criticalColor;
            }

            if (meterValueText != null)
            {
                meterValueText.text = $"{currentMeter}/{max} ({Mathf.RoundToInt(percent * 100)}%)";
            }

            if (statusText != null)
            {
                if (currentMeter >= max)
                {
                    statusText.text = "TIMELINE COLLAPSE! History disintegrating!";
                }
                else if (currentMeter >= 7)
                {
                    statusText.text = "CRITICAL ALERT: Paradox threshold near collapse!";
                }
                else if (currentMeter >= 4)
                {
                    statusText.text = "TIMELINE UNSTABLE: Disturbances detected.";
                }
                else
                {
                    statusText.text = "TIMELINE STABLE: Spacetime continuum intact.";
                }
            }

            if (criticalAlertBadge != null)
            {
                criticalAlertBadge.SetActive(currentMeter >= 7);
            }
        }
    }
}
