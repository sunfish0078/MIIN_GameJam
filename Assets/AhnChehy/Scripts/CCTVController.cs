using System;
using ChoiJeongYun.Scripts.Interaction;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class CCTVController : MonoBehaviour
{
    public static CCTVController Instance { get; private set; }
    
    public event Action OnCameraSwitched;

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
    }

    // 이 오브젝트는 ARoom 씬 소속이라 씬이 리로드될 때마다 파괴되고 새로 생김(DontDestroyOnLoad 아님) ->
    // isCCTVOpen/currentIndex 등도 매번 초기값으로 새로 시작하므로 별도 리바인드 없이 그냥 여기서 한 번 찾으면 됨
    private void Start()
    {
        CCTVSceneCameras sceneCameras = FindFirstObjectByType<CCTVSceneCameras>();
        cctvCameras = (sceneCameras != null) ? sceneCameras.cctvCameras : null;

        ShowControlRoom();
    }

    private void Update()
    {
        if (PhotoTransitionEffect.IsPlaying)
            return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard[openKey].wasPressedThisFrame)
        {
            if (OpenCCTV())
            {
                if (glitchEffect != null)
                    glitchEffect.TriggerGlitch();

                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlayUIClick();
            }
        }
        else if (keyboard[closeKey].wasPressedThisFrame)
        {
            ShowControlRoom();

            if (glitchEffect != null)
                glitchEffect.TriggerGlitch();

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayUIClick();
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
        // 이미 열려있으면 무시 (안 그러면 W 누를 때마다 지지직 이펙트가 다시 나감)
        if (isCCTVOpen)
            return false;

        // 숨어있는 동안엔 CCTV를 열 수 없음
        if (HidingState.Instance != null && HidingState.Instance.IsHidden)
            return false;

        // 이 스테이지에 CCTV가 등록되어 있지 않으면 무시
        if (cctvCameras == null || cctvCameras.Length == 0)
            return false;

        isCCTVOpen = true;
        ActivateCCTVAt(currentIndex);

        if (cctvCanvas != null)
            cctvCanvas.SetActive(true);

        return true;
    }

    public void ShowControlRoom()
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

        OnCameraSwitched?.Invoke();
    }

    private void ShowNextCamera()
    {
        currentIndex++;
        if (currentIndex >= cctvCameras.Length)
            currentIndex = 0;

        ActivateCCTVAt(currentIndex);

        if (glitchEffect != null)
            glitchEffect.TriggerGlitch();

        OnCameraSwitched?.Invoke();
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