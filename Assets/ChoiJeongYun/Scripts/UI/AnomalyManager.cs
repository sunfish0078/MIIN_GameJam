using UnityEngine;
using UnityEngine.UI;

namespace ChoiJeongYun.Scripts.Anomaly
{
    public class AnomalyManager : MonoBehaviour
    {
        public static AnomalyManager Instance { get; private set; }
        
        public static float TimeMultiplier { get; private set; } = 1f;

        private class Slot
        {
            public GameObject prefab;
            public GameObject activeInstance;
            public float spawnTime;
            public float timer;
            public bool waitingToSpawn;
        }

        [Header("경고 문구 UI")]
        [SerializeField] private EncroachmentWarningUI warningUI;

        [Header("경고 화면 색상")]
        [SerializeField] private Image tintOverlay;
        [SerializeField] private Color tier1TintColor = new Color(1f, 0.9f, 0.3f, 0.18f);
        [SerializeField] private Color tier2TintColor = new Color(1f, 0.15f, 0.15f, 0.25f);
        [SerializeField] private float tintLerpSpeed = 3f;

        [Header("소환 딜레이")]
        [SerializeField] private float minSpawnDelay = 3f;
        [SerializeField] private float maxSpawnDelay = 7f;

        [Header("경고 시작 시간")]
        [SerializeField] private float tier1Threshold = 5f;
        [SerializeField] private float tier2Threshold = 10f;

        private readonly Slot slotA = new Slot();
        private readonly Slot slotB = new Slot();
        private Transform[] pointPool;
        private int tier;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (tintOverlay != null)
                tintOverlay.color = new Color(tier1TintColor.r, tier1TintColor.g, tier1TintColor.b, 0f);
        }

        public void SetupRoom(GameObject prefabA, GameObject prefabB, Transform[] points)
        {
            StopRoom();

            slotA.prefab = prefabA;
            slotB.prefab = prefabB;
            pointPool = points;
        }

        public void StopRoom()
        {
            if (slotA.activeInstance != null) Destroy(slotA.activeInstance);
            if (slotB.activeInstance != null) Destroy(slotB.activeInstance);

            slotA.activeInstance = null;
            slotB.activeInstance = null;
            slotA.prefab = null;
            slotB.prefab = null;
            slotA.waitingToSpawn = false;
            slotB.waitingToSpawn = false;
            pointPool = null;

            tier = 0;
            TimeMultiplier = 1f;

            if (tintOverlay != null)
            {
                Color c = tintOverlay.color;
                c.a = 0f;
                tintOverlay.color = c;
            }
        }

        private void Update()
        {
            UpdateSlot(slotA, slotB);
            UpdateSlot(slotB, slotA);
            UpdateTier();
            UpdateTint();
        }

        private void UpdateSlot(Slot slot, Slot other)
        {
            if (slot.prefab == null) return;
            if (slot.activeInstance != null) return;

            // 기존 개체가 없어진(처음 대기 / 사진 찍혀서 파괴됨) 시점부터 새로 3~7초 타이머를 시작.
            if (!slot.waitingToSpawn)
            {
                slot.waitingToSpawn = true;
                slot.timer = Random.Range(minSpawnDelay, maxSpawnDelay);
            }

            slot.timer -= Time.deltaTime;
            if (slot.timer > 0f) return;

            Transform point = PickSpawnPoint(other);
            if (point == null) return;

            slot.activeInstance = Instantiate(slot.prefab, point.position, point.rotation);
            slot.spawnTime = Time.time;
            slot.waitingToSpawn = false;
        }

        private Transform PickSpawnPoint(Slot other)
        {
            if (pointPool == null || pointPool.Length == 0) return null;
            if (pointPool.Length == 1) return pointPool[0];

            Transform point;
            int guard = 0;
            do
            {
                point = pointPool[Random.Range(0, pointPool.Length)];
                guard++;
            }
            while (other.activeInstance != null && point.position == other.activeInstance.transform.position && guard < 10);

            return point;
        }

        private void UpdateTier()
        {
            float maxAge = 0f;
            bool any = false;

            if (slotA.activeInstance != null)
            {
                any = true;
                maxAge = Mathf.Max(maxAge, Time.time - slotA.spawnTime);
            }

            if (slotB.activeInstance != null)
            {
                any = true;
                maxAge = Mathf.Max(maxAge, Time.time - slotB.spawnTime);
            }

            if (!any)
            {
                tier = 0;
                TimeMultiplier = 1f;
                return;
            }

            if (maxAge >= tier2Threshold)
            {
                if (tier < 2 && warningUI != null)
                    warningUI.ShowTier2();

                tier = 2;
                TimeMultiplier = 3f;
            }
            else if (maxAge >= tier1Threshold)
            {
                if (tier < 1 && warningUI != null)
                    warningUI.ShowTier1();

                tier = 1;
                TimeMultiplier = 2f;
            }
            else
            {
                tier = 0;
                TimeMultiplier = 1f;
            }
        }

        private void UpdateTint()
        {
            if (tintOverlay == null) return;

            Color target;
            if (tier == 2)
                target = tier2TintColor;
            else if (tier == 1)
                target = tier1TintColor;
            else
                target = new Color(tier1TintColor.r, tier1TintColor.g, tier1TintColor.b, 0f);

            tintOverlay.color = Color.Lerp(tintOverlay.color, target, Time.deltaTime * tintLerpSpeed);
        }
    }
}
