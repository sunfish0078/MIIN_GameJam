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

    [Header("BGM")]
    [SerializeField] private AudioClip ventHumSound;

    private const int RoomCount = 7;
    private const string ControlRoomObjectName = "ControlRoomBG";

    private SpriteRenderer[] roomRenderers;
    private SpriteRenderer controlRoomRenderer;
    private GameObject currentMonster;
    private Texture2D currentSnapshot;
    private List<GameObject> allItems;
    private RoomType currentRoomType;
    private readonly HashSet<RoomType> clearedRooms = new HashSet<RoomType>();

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
        
        allItems = new List<GameObject>(GameObject.FindGameObjectsWithTag(itemTag));

        if (blackPanel != null)
            SetBlackAlpha(0f);
    }

    private SpriteRenderer FindRenderer(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj == null)
            return null;

        return obj.GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        SwitchToMap(RoomType.ARoom);

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayAmbient(ventHumSound);
    }
    
    public void SwitchToMap(RoomType roomType)
    {
        currentRoomType = roomType;

        RoomMapSO map = FindMap(roomType);
        if (map == null)
        {
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
        if (currentRoomType != RoomType.ARoom)
            clearedRooms.Add(currentRoomType);

        // BRoom1/2/3 다 클리어했으면 게임 클리어로
        if (clearedRooms.Count >= 3)
        {
            StartCoroutine(GameClearSequence());
            return;
        }

        StartCoroutine(ReturnToARoomAfterDelay());
    }

    private IEnumerator GameClearSequence()
    {
        PhotoTransitionEffect.SetInputLocked(true);

        try
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.StopAmbient();

            if (tornPhotoFeedback != null && currentSnapshot != null)
            {
                yield return tornPhotoFeedback.PlayAndWait(currentSnapshot);
                Destroy(currentSnapshot);
                currentSnapshot = null;
            }

            if (blackPanel != null)
                yield return Fade(0f, 1f, fadeDuration);

            if (CCTVController.Instance != null)
                CCTVController.Instance.ShowControlRoom();

            if (RoomTimer.Instance != null)
                RoomTimer.Instance.StopRoom();
            
            if (SoundManager.Instance != null)
                SoundManager.Instance.StopSFX();

            if (MainMenuUIManager.Instance != null)
                MainMenuUIManager.Instance.ShowGameClearPanel();
        }
        finally
        {
            PhotoTransitionEffect.SetInputLocked(false);
        }
    }

    private IEnumerator ReturnToARoomAfterDelay()
    {
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
