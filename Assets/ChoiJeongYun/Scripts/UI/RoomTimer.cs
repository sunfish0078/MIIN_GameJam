using System;
using ChoiJeongYun.Scripts.Anomaly;
using TMPro;
using UnityEngine;

namespace ChoiJeongYun.Scripts.Timer
{
    public class RoomTimer : MonoBehaviour
    {
        public static RoomTimer Instance { get; private set; }

        // 0이 되면 발동 (관리실 침입/사망연출 작업에서 소비 예정)
        public event Action OnEncroachmentReached;

        [Header("UI")]
        [SerializeField] private GameObject timerRoot;
        [SerializeField] private TMP_Text currentTimeText;
        [SerializeField] private TMP_Text remainingTimeText;

        [Header("현재시간 진행 속도 (실제 1초당 게임 내 분)")]
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
