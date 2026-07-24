using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using UnityEngine.InputSystem;


// 관리실 시점 ↔ CCTV 시점을 전환하는 컨트롤러.
// 씬(스테이지)이 바뀌어도 파괴되지 않고 유지되는 싱글톤.

// 조작:
// - W : CCTV 감시 모드 켜기
// - S : CCTV 감시 모드 끄기 (관리실 시점으로 복귀)
// - Q : 이전 CCTV로 전환
// - E : 다음 CCTV로 전환

// 동작 원리:
// - 관리실캠(controlRoomCamera)은 모든 스테이지가 공유하는 단 하나의 오브젝트.
//  이 스크립트가 붙은 오브젝트의 자식으로 넣어두면 DontDestroyOnLoad에 같이 딸려가서
// 씬이 바뀌어도 계속 살아있음.
// - CCTV들은 스테이지 씬마다 다른 오브젝트이므로, 새 씬이 로드될 때마다
//  그 씬에 있는 CCTVSceneCameras를 찾아 CCTV 목록만 새로 갱신함.

/*
사용법:
1) 맨 처음 로드되는 씬에 빈 GameObject 만들어서 이 스크립트 부착
2) 그 GameObject의 자식으로 관리실 Virtual Camera를 넣고, controlRoomCamera에 연결
3) 각 스테이지 씬에는 CCTVSceneCameras 컴포넌트를 배치하고, 그 스테이지의 CCTV들을 연결
*/

public class CCTVController : MonoBehaviour
{
    public static CCTVController Instance { get; private set; }
 
    [Header("관리실 카메라")]
    [SerializeField] private CinemachineCamera controlRoomCamera;

    [Header("CCTV 전용 UI")]
    [SerializeField] private GameObject cctvCanvas;
    [SerializeField] private CCTVGlitchEffect glitchEffect;

    [Header("Priority")]
    [SerializeField] private int activePriority = 20;
    [SerializeField] private int inactivePriority = 0;
 
    [Header("입력 키")]
    [SerializeField] private Key openKey = Key.W;
    [SerializeField] private Key closeKey = Key.S;
    [SerializeField] private Key previousKey = Key.Q;
    [SerializeField] private Key nextKey = Key.E;
 
    // 현재 스테이지의 CCTV 목록. 씬이 바뀔 때마다 갱신됨.
    private CCTVCameraEntry[] cctvCameras;
 
    private int currentIndex = 0;
    private bool isCCTVOpen = false;
 
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
 
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
 
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
 
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
 
    private void Start()
    {
        ShowControlRoom();
    }
 

    // 새 씬이 로드될 때마다 호출됨. 그 씬의 CCTVSceneCameras를 찾아 CCTV 목록을 갱신.

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RebindCCTVsForCurrentScene();
    }
 

    // 현재 씬에 있는 CCTVSceneCameras를 찾아서 CCTV 목록을 새로 연결.
    // 해당 씬에 없으면(예: 메인 메뉴 씬) CCTV 목록을 비워둠.
    private void RebindCCTVsForCurrentScene()
    {
        // 유니티 최신 API 사용 권장 (구버전은 FindObjectOfType<CCTVSceneCameras>() 유지)
        CCTVSceneCameras sceneCameras = Object.FindFirstObjectByType<CCTVSceneCameras>();

        cctvCameras = (sceneCameras != null) ? sceneCameras.cctvCameras : null;

        // 만약 새 스테이지에 CCTV가 아예 없다면 관리실 뷰로 전환
        if (cctvCameras == null || cctvCameras.Length == 0)
        {
            ShowControlRoom();
            return;
        }

        // [핵심] 새 스테이지의 카메라 개수가 이전 인덱스보다 적을 경우 범위를 벗어나지 않도록 보정
        if (currentIndex >= cctvCameras.Length)
        {
            currentIndex = cctvCameras.Length - 1;
        }

        // 이전 스테이지에서 CCTV를 켜둔 상태였다면, 새로 들어온 씬에서도 그 인덱스의 CCTV를 바로 켬
        if (isCCTVOpen)
        {
            ActivateCCTVAt(currentIndex);
        }
        else
        {
            ShowControlRoom();
        }
    }
 
    private void Update()
    {
        if (PhotoTransitionEffect.IsPlaying)
            return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard[openKey].wasPressedThisFrame)
        {
            if (OpenCCTV() && glitchEffect != null)
                glitchEffect.TriggerGlitch();
        }
        else if (keyboard[closeKey].wasPressedThisFrame)
        {
            ShowControlRoom();

            if (glitchEffect != null)
                glitchEffect.TriggerGlitch();
        }
 
        if (isCCTVOpen)
        {
            if (keyboard[previousKey].wasPressedThisFrame)
                ShowPreviousCamera();
            else if (keyboard[nextKey].wasPressedThisFrame)
                ShowNextCamera();
        }
    }
 
    private bool OpenCCTV()
    {
        // 이 스테이지에 CCTV가 등록되어 있지 않으면 무시
        if (cctvCameras == null || cctvCameras.Length == 0)
            return false;

        isCCTVOpen = true;
        ActivateCCTVAt(currentIndex);

        if (cctvCanvas != null)
            cctvCanvas.SetActive(true);

        return true;
    }

    private void ShowControlRoom()
    {
        isCCTVOpen = false;

        controlRoomCamera.Priority = activePriority;

        if (cctvCameras != null)
        {
            foreach (var entry in cctvCameras)
            {
                entry.camera.Priority = inactivePriority;
            }
        }

        if (cctvCanvas != null)
            cctvCanvas.SetActive(false);
    }
 
    private void ShowPreviousCamera()
    {
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = cctvCameras.Length - 1;

        ActivateCCTVAt(currentIndex);

        if (glitchEffect != null)
            glitchEffect.TriggerGlitch();
    }

    private void ShowNextCamera()
    {
        currentIndex++;
        if (currentIndex >= cctvCameras.Length)
            currentIndex = 0;

        ActivateCCTVAt(currentIndex);

        if (glitchEffect != null)
            glitchEffect.TriggerGlitch();
    }
 
    private void ActivateCCTVAt(int index)
    {
        controlRoomCamera.Priority = inactivePriority;
 
        for (int i = 0; i < cctvCameras.Length; i++)
        {
            cctvCameras[i].camera.Priority = (i == index) ? activePriority : inactivePriority;
        }
    }

    // 현재 보고 있는 CCTV의 방 이름. CCTV가 꺼져있으면 빈 문자열.
    public string GetCurrentRoomName()
    {
        if (!isCCTVOpen || cctvCameras == null || currentIndex >= cctvCameras.Length)
            return string.Empty;

        return cctvCameras[currentIndex].roomName;
    }
}