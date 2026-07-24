using System.Collections;
using ChoiJeongYun.Scripts.Enemy;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct RoomSpawnPoint
{
    public RoomType roomType;
    public Transform spawnPoint;
}

// 맵(방)마다 다른 배경/몬스터를 SO로 받아서, 고정된 SpriteRenderer들에 스프라이트만 갈아끼우는 방식.
public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("Map Data")]
    [SerializeField] private RoomMapSO[] maps;

    [Header("Monster Spawn Points (씬 오브젝트, 직접 연결)")]
    [SerializeField] private RoomSpawnPoint[] spawnPoints;

    [Header("Fade")]
    [SerializeField] private Image blackPanel;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Item")]
    [SerializeField] private string itemTag = "item";

    [Header("Monster Death")]
    [SerializeField] private float returnToARoomDelay = 1.5f;

    private const int RoomCount = 7;
    private const string ControlRoomObjectName = "ControlRoomBG";

    private SpriteRenderer[] roomRenderers;
    private SpriteRenderer controlRoomRenderer;
    private GameObject currentMonster;

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

    // 페이드 없이 즉시 전환 (PhotoTransitionEffect처럼 이미 자기만의 연출/페이드가 있는 쪽에서 사용)
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

        SpawnMonster(map.monsterPrefab, FindSpawnPoint(roomType));

        // A룸으로 돌아오면 남은 아이템들 다시 켜고, 다른 맵으로 가면 다 꺼둠
        SetItemsActive(roomType == RoomType.ARoom);
    }

    // 자체 연출이 없는 곳(예: 단순 나가기/복귀 상호작용)에서 페이드까지 같이 처리해야 할 때 사용
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

    private void SetItemsActive(bool active)
    {
        foreach (GameObject item in GameObject.FindGameObjectsWithTag(itemTag))
        {
            item.SetActive(active);
        }
    }

    private void SpawnMonster(GameObject monsterPrefab, Transform spawnPoint)
    {
        if (currentMonster != null)
            Destroy(currentMonster);

        if (monsterPrefab != null && spawnPoint != null)
        {
            currentMonster = Instantiate(monsterPrefab, spawnPoint.position, spawnPoint.rotation);

            if (currentMonster.TryGetComponent(out AbstractEnemy enemy))
            {
                enemy.OnDeadEvent.AddListener(HandleMonsterDead);
            }
        }
    }

    private void HandleMonsterDead()
    {
        StartCoroutine(ReturnToARoomAfterDelay());
    }

    private IEnumerator ReturnToARoomAfterDelay()
    {
        yield return new WaitForSeconds(returnToARoomDelay);
        SwitchToMapWithFade(RoomType.ARoom);
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
