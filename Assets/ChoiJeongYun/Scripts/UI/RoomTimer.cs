using System;
using System.Collections;
using ChoiJeongYun.Scripts.Anomaly;
using TMPro;
using UnityEngine;

namespace ChoiJeongYun.Scripts.Timer
{
    public class RoomTimer : MonoBehaviour
    {
        public static RoomTimer Instance { get; private set; }
        
        public event Action OnEncroachmentReached;

        [Header("UI")]
        [SerializeField] private GameObject timerRoot;
        [SerializeField] private TMP_Text currentTimeText;
        [SerializeField] private TMP_Text remainingTimeText;

        [Header("접근 경고 (깜빡거림)")]
        [SerializeField] private TMP_Text approachWarningText;
        [SerializeField] private float blinkInterval = 0.5f;

        private Coroutine blinkRoutine;

        [Header("현재시간 진행 속도")]
        [SerializeField] private float gameMinutesPerRealSecond = 0.1f;

        private int startHour;
        private int startMinute;
        private float durationSeconds;
        private float remainingSeconds;
        private bool running;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (timerRoot != null)
                timerRoot.SetActive(false);

            if (approachWarningText != null)
                approachWarningText.gameObject.SetActive(false);
        }

        public void ShowApproachWarning()
        {
            if (approachWarningText == null) return;

            approachWarningText.gameObject.SetActive(true);

            if (blinkRoutine != null)
                StopCoroutine(blinkRoutine);

            blinkRoutine = StartCoroutine(BlinkRoutine());
        }

        public void HideApproachWarning()
        {
            if (blinkRoutine != null)
            {
                StopCoroutine(blinkRoutine);
                blinkRoutine = null;
            }

            if (approachWarningText != null)
                approachWarningText.gameObject.SetActive(false);
        }

        private IEnumerator BlinkRoutine()
        {
            bool visible = true;
            while (true)
            {
                SetWarningAlpha(visible ? 1f : 0f);
                visible = !visible;
                yield return new WaitForSeconds(blinkInterval);
            }
        }

        private void SetWarningAlpha(float alpha)
        {
            Color c = approachWarningText.color;
            c.a = alpha;
            approachWarningText.color = c;
        }

        public void SetupRoom(int roomStartHour, int roomStartMinute, float roomDurationSeconds)
        {
            startHour = roomStartHour;
            startMinute = roomStartMinute;
            durationSeconds = roomDurationSeconds;
            remainingSeconds = roomDurationSeconds;
            running = true;

            if (timerRoot != null)
                timerRoot.SetActive(true);

            UpdateDisplay();
        }

        public void StopRoom()
        {
            running = false;

            if (timerRoot != null)
                timerRoot.SetActive(false);

            HideApproachWarning();
        }

        private void Update()
        {
            if (!running) return;

            remainingSeconds -= Time.deltaTime * AnomalyManager.TimeMultiplier;

            if (remainingSeconds <= 0f)
            {
                remainingSeconds = 0f;
                running = false;
                UpdateDisplay();
                OnEncroachmentReached?.Invoke();
                return;
            }

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            float elapsed = durationSeconds - remainingSeconds;
            int totalMinutes = startHour * 60 + startMinute + Mathf.FloorToInt(elapsed * gameMinutesPerRealSecond);

            int hour = (totalMinutes / 60) % 24;
            int minute = totalMinutes % 60;

            if (currentTimeText != null)
                currentTimeText.text = $"{hour}시간 {minute}분";

            int remMinutes = Mathf.FloorToInt(remainingSeconds / 60f);
            int remSeconds = Mathf.FloorToInt(remainingSeconds % 60f);

            if (remainingTimeText != null)
                remainingTimeText.text = $"잠식까지 남은 시간: {remMinutes}분 {remSeconds}초";
        }
    }
}
