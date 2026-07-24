using System.Collections;
using System.Collections.Generic;
using ChoiJeongYun.Scripts.Anomaly;
using ChoiJeongYun.Scripts.Enemy;
using ChoiJeongYun.Scripts.Feedback;
using ChoiJeongYun.Scripts.Map;
using ChoiJeongYun.Scripts.Timer;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct RoomSpawnPoint
{
    public RoomType roomType;
    public Transform spawnPoint;
}

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("Map Data")]
    [SerializeField] private RoomMapSO[] maps;

    [Header("Monster Spawn Points")]
    [SerializeField] private RoomSpawnPoint[] spawnPoints;

    [Header("Monster Patrol Points")]
    [SerializeField] private Transform[] bRoom1PatrolPoints;
    [SerializeField] private Transform[] bRoom2PatrolPoints;
    [SerializeField] private Transform[] bRoom3PatrolPoints;

    [Header("Anomaly Spawn Points")]
    [SerializeField] private Transform[] bRoom1AnomalyPoints;
    [SerializeField] private Transform[] bRoom2AnomalyPoints;
    [SerializeField] private Transform[] bRoom3AnomalyPoints;

    [Header("Fade")]
    [SerializeField] private Image blackPanel;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Item")]
    [SerializeField] private string itemTag = "item";

    [Header("Monster Death")]
    [SerializeField] private float returnToARoomDelay = 1.5f;
    [SerializeField] private TornPhotoFeedback tornPhotoFeedback;

    private const int RoomCount = 7;
    private const string ControlRoomObjectName = "ControlRoomBG";

    private SpriteRenderer[] roomRenderers;
    private SpriteRenderer controlRoomRenderer;
    private GameObject currentMonster;
    private Texture2D currentSnapshot;
    private List<GameObject> allItems;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        roomRenderers = new SpriteRenderer[RoomCount];
        for (int i = 0; i < RoomCount; i++)
        {
            roomRenderers[i] = FindRenderer($"ARoom{i + 1}BG");
        }

        controlRoomRenderer = FindRenderer(ControlRoomObjectName);

        // FindGameObjectsWithTag는 비활성 오브젝트를 못 찾으므로, 다 켜져있는 지금(Awake) 시점에 미리 캐싱해둠.
        allItems = new List<GameObject>(GameObject.FindGameObjectsWithTag(itemTag));

        if (blackPanel != null)
            SetBlackAlpha(0f);
    }

    private SpriteRenderer FindRenderer(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj == null)
        {
            Debug.LogError($"MapManager: 씬에서 '{objectName}' 오브젝트를 못 찾았어요.");
            return null;
        }

        SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
        if (renderer == null)
            Debug.LogError($"MapManager: '{objectName}'에 SpriteRenderer가 없어요.");

        return renderer;
    }

    private void Start()
    {
        SwitchToMap(RoomType.ARoom);
    }
    
    public void SwitchToMap(RoomType roomType)
    {
        RoomMapSO map = FindMap(roomType);
        if (map == null)
        {
            Debug.LogError($"MapManager: '{roomType}'에 해당하는 RoomMapSO를 못 찾았어요.");
            return;
        }

        Sprite[] roomSprites = { map.room1, map.room2, map.room3, map.room4, map.room5, map.room6, map.room7 };

        for (int i = 0; i < roomRenderers.Length; i++)
        {
            if (roomRenderers[i] != null)
                roomRenderers[i].sprite = roomSprites[i];
        }

        if (controlRoomRenderer != null)
            controlRoomRenderer.sprite = map.controlRoom;

        SpawnMonster(map.mainMonsterPrefab, FindSpawnPoint(roomType), FindPatrolPoints(roomType));
        SetItemsActive(roomType == RoomType.ARoom);

        if (AnomalyManager.Instance != null)
        {
            if (roomType == RoomType.ARoom)
                AnomalyManager.Instance.StopRoom();
            else
                AnomalyManager.Instance.SetupRoom(map.anomalyPrefabA, map.anomalyPrefabB, FindAnomalyPoints(roomType));
        }

        if (RoomTimer.Instance != null)
        {
            if (roomType == RoomType.ARoom)
                RoomTimer.Instance.StopRoom();
            else
                RoomTimer.Instance.SetupRoom(map.startHour, map.startMinute, map.encroachmentDurationSeconds);
        }
    }
    
    public void SwitchToMapWithFade(RoomType roomType)
    {
        StartCoroutine(SwitchWithFadeRoutine(roomType));
    }

    private IEnumerator SwitchWithFadeRoutine(RoomType roomType)
    {
        if (blackPanel != null)
            yield return Fade(0f, 1f, fadeDuration);

        if (CCTVController.Instance != null)
            CCTVController.Instance.ShowControlRoom();

        SwitchToMap(roomType);
        yield return null;

        if (blackPanel != null)
            yield return Fade(1f, 0f, fadeDuration);
    }

    private RoomMapSO FindMap(RoomType roomType)
    {
        foreach (RoomMapSO map in maps)
        {
            if (map != null && map.roomType == roomType)
                return map;
        }

        return null;
    }

    private Transform FindSpawnPoint(RoomType roomType)
    {
        foreach (RoomSpawnPoint entry in spawnPoints)
        {
            if (entry.roomType == roomType)
                return entry.spawnPoint;
        }

        return null;
    }

    private Transform[] FindPatrolPoints(RoomType roomType)
    {
        switch (roomType)
        {
            case RoomType.BRoom1: return bRoom1PatrolPoints;
            case RoomType.BRoom2: return bRoom2PatrolPoints;
            case RoomType.BRoom3: return bRoom3PatrolPoints;
            default: return null;
        }
    }

    private Transform[] FindAnomalyPoints(RoomType roomType)
    {
        switch (roomType)
        {
            case RoomType.BRoom1: return bRoom1AnomalyPoints;
            case RoomType.BRoom2: return bRoom2AnomalyPoints;
            case RoomType.BRoom3: return bRoom3AnomalyPoints;
            default: return null;
        }
    }

    private void SetItemsActive(bool active)
    {
        foreach (GameObject item in allItems)
        {
            if (item != null)
                item.SetActive(active);
        }
    }

    private void SpawnMonster(GameObject monsterPrefab, Transform spawnPoint, Transform[] patrolPoints)
    {
        if (currentMonster != null)
            Destroy(currentMonster);

        if (monsterPrefab != null && spawnPoint != null)
        {
            currentMonster = Instantiate(monsterPrefab, spawnPoint.position, spawnPoint.rotation);

            if (currentMonster.TryGetComponent(out AbstractEnemy enemy))
            {
                enemy.OnDeadEvent.AddListener(HandleMonsterDead);
                enemy.MovementCompo.SetPatrolPoints(patrolPoints);

                if (GameOverManager.Instance != null)
                {
                    enemy.MovementCompo.OnFootstepWarning += GameOverManager.Instance.HandleFootstepWarning;
                    enemy.MovementCompo.OnReachedControlRoom += GameOverManager.Instance.HandleMonsterReachedControlRoom;
                }
            }
        }
    }

    private void HandleMonsterDead()
    {
        StartCoroutine(ReturnToARoomAfterDelay());
    }

    private IEnumerator ReturnToARoomAfterDelay()
    {
        // 몬스터 죽고 나서 A룸에 실제로 돌아갈 때까지(찢어지는 연출+페이드 포함) 다른 입력 다 막음
        PhotoTransitionEffect.SetInputLocked(true);

        try
        {
            yield return new WaitForSeconds(returnToARoomDelay);

            if (tornPhotoFeedback != null && currentSnapshot != null)
            {
                yield return tornPhotoFeedback.PlayAndWait(currentSnapshot);
                Destroy(currentSnapshot);
                currentSnapshot = null;
            }

            // SwitchToMapWithFade는 코루틴을 던지기만 하고 안 기다리므로, 직접 하위 코루틴으로 대기
            yield return SwitchWithFadeRoutine(RoomType.ARoom);
        }
        finally
        {
            PhotoTransitionEffect.SetInputLocked(false);
        }
    }

    public void SetCurrentSnapshot(Texture2D snapshot)
    {
        if (currentSnapshot != null)
            Destroy(currentSnapshot);

        currentSnapshot = snapshot;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetBlackAlpha(Mathf.Lerp(from, to, elapsed / duration));
            yield return null;
        }

        SetBlackAlpha(to);
    }

    private void SetBlackAlpha(float alpha)
    {
        Color c = blackPanel.color;
        c.a = alpha;
        blackPanel.color = c;
    }
}
