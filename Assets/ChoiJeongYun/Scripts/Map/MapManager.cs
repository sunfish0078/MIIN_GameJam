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
                RoomTimer.Instance.SetupRoom(map.startHour, map.startMinute, map.durationSec);
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

        // 처치 연출(사진 찢기 등)이 재생되는 동안에도 타이머가 계속 돌고 있어서, 그 사이에
        // 잠식 시간이 다 되면 사망 시퀀스가 동시에 겹쳐 터지는 문제가 있었음 -> 처치 즉시 타이머부터 정지
        if (RoomTimer.Instance != null)
            RoomTimer.Instance.StopRoom();

        // 죽는 사운드 길이가 보스마다 달라서, 고정 딜레이만 쓰면 사운드가 다 나오기 전에
        // 찢기 연출이 시작되는 보스가 있었음 -> 실제 사운드 길이를 같이 넘겨서 그만큼은 무조건 기다리게 함
        float deathSoundLength = 0f;
        if (currentMonster != null && currentMonster.TryGetComponent(out AbstractEnemy deadEnemy))
            deathSoundLength = deadEnemy.DeathSoundLength;

        // 죽는소리 자체가 뚝 끊기지 않고 재생 시간에 맞춰 서서히 잦아들도록, 죽은 직후 바로 페이드 시작
        // (재생 길이 == 페이드 길이라서 다 끝날 때쯤 자연스럽게 0이 됨. 아래 코루틴들의 대기 시간도 여기 맞춰져 있음)
        if (deathSoundLength > 0f && SoundManager.Instance != null)
            SoundManager.Instance.FadeOutSFX(deathSoundLength);

        // BRoom1/2/3 다 클리어했으면 게임 클리어로
        if (clearedRooms.Count >= 3)
        {
            StartCoroutine(GameClearSequence(deathSoundLength));
            return;
        }

        StartCoroutine(ReturnToARoomAfterDelay(deathSoundLength));
    }

    private IEnumerator GameClearSequence(float deathSoundLength)
    {
        PhotoTransitionEffect.SetInputLocked(true);

        try
        {
            // 죽는소리 페이드는 HandleMonsterDead에서 이미 시작함. 배경음만 여기서 서서히 줄임
            if (SoundManager.Instance != null)
                SoundManager.Instance.FadeOutAmbient(fadeDuration);

            // 죽는소리가 다 잦아들 때까지 기다렸다가 찢기 연출 시작 (안 그러면 소리가 덜 끝난 채로 찢겨나감)
            if (deathSoundLength > 0f)
                yield return new WaitForSeconds(deathSoundLength);

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

            if (MainMenuUIManager.Instance != null)
                MainMenuUIManager.Instance.ShowGameClearPanel();
        }
        finally
        {
            PhotoTransitionEffect.SetInputLocked(false);
        }
    }

    private IEnumerator ReturnToARoomAfterDelay(float deathSoundLength)
    {
        PhotoTransitionEffect.SetInputLocked(true);

        try
        {
            // 사망 사운드가 returnToARoomDelay보다 긴 보스가 있어서, 둘 중 더 긴 쪽만큼 기다림
            yield return new WaitForSeconds(Mathf.Max(returnToARoomDelay, deathSoundLength));

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

    // 잠식(시간초과) 사망 연출용 - 현재 방의 RoomMapSO에 지정된 이미지를 GameOverManager가 가져다 씀
    public Sprite GetCurrentEncroachmentDeathImage()
    {
        RoomMapSO map = FindMap(currentRoomType);
        return map != null ? map.deathImage : null;
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
