using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class VolumeDial : MonoBehaviour, IDragHandler, IScrollHandler
{
    private enum VolumeType { Master, BGM, SFX }
 
    [Header("Control Volume")]
    [SerializeField] private VolumeType volumeType;
 
    [Header("Settings")]
    [SerializeField] private RectTransform knob;  
    [SerializeField] private TextMeshProUGUI percentText; 
 
    [Header("Rotation Position")]
    [SerializeField] private float minAngle = -135f; // 하얀색 획?이 0일때의 각도 (시계 7시 방향쯤)
    [SerializeField] private float maxAngle = 135f;   // 100일때의 각도 (시계 5시 방향쯤)
 
    [Header("Sensitivity")]
    [SerializeField] private float dragSensitivity = 0.005f;  // 드래그 1픽셀당 값 변화량
    [SerializeField] private float scrollSensitivity = 0.05f; // 휠 한 칸당 값 변화량(더 빨리 변함 값 작으니) 픽셀 값으로 얼마나 움직였는지 계산한다
 
    private float currentValue; // 0~1 범위의 현재 볼륨 값 (75임)
 
    private void Start()
    {
        currentValue = GetVolumeFromManager();
        UpdateVisual();
    }

    /// 드래그하는 동안 매 프레임 호출됨. 위로 드래그하면 값 증가, 아래로 드래그하면 감소.
    public void OnDrag(PointerEventData eventData)
    {
        float deltaY = eventData.delta.y; //유니티가 얼마나 마우스를 내렸는지 또는 올렸는지 계산해줌
        float valueChange = deltaY * dragSensitivity; //그 계산값고 설정한 dragSensitivity 값을 곱해서 얼마난 증가/감소 할것인지 정한다
 
        SetValue(currentValue + valueChange); //증가/감소 값을 더한 최종 계산 값을 넘겨준다
    }
    
    // 다이얼 위에서 마우스 휠을 굴렸을 때 호출됨.

    public void OnScroll(PointerEventData eventData)
    {
        float scrollDelta = eventData.scrollDelta.y;
        SetValue(currentValue + scrollDelta * scrollSensitivity);
    }
 

    // 값을 0~1 범위로 제한하고, 화면 갱신 + AudioManager에 반영까지 처리.
    private void SetValue(float newValue)
    {
        currentValue = Mathf.Clamp01(newValue); //0 ~ 1사이로 반환!
        UpdateVisual(); //비주얼 다이얼도 돌리고
        ApplyToAudioManager(currentValue); //실제 소리도 0 ~ 1사이로 반환한 값으로 넘겨줌
    }
 

    // 현재 값에 맞춰 회전 각도와 텍스트를 업데이트
    private void UpdateVisual()
    {
        if (knob != null)
        {
            float angle = Mathf.Lerp(minAngle, maxAngle, currentValue); //부드럽게 회전
            knob.localEulerAngles = new Vector3(0f, 0f, -angle); // UI는 Z축 회전이 시계방향으로 보이도록 부호 반전
        }
 
        if (percentText != null)
        {
            int percent = Mathf.RoundToInt(currentValue * 100f); //인트값으로
            percentText.text = $"{percent}%"; 
        }
    }
 

    //처음 시작할때 용(값 바꿈 ㄴㄴ)
    private float GetVolumeFromManager()
    {
        switch (volumeType)
        {
            case VolumeType.Master: return AudioManager.Instance.GetMasterVolume();
            case VolumeType.BGM: return AudioManager.Instance.GetBGMVolume();
            case VolumeType.SFX: return AudioManager.Instance.GetSFXVolume();
            default: return 0.75f;
        }
    }
 
    //소리를 이제 바꿔줌
    private void ApplyToAudioManager(float value)
    {
        switch (volumeType)
        {
            case VolumeType.Master: AudioManager.Instance.SetMasterVolume(value); break;
            case VolumeType.BGM: AudioManager.Instance.SetBGMVolume(value); break;
            case VolumeType.SFX: AudioManager.Instance.SetSFXVolume(value); break;
        }
    }
}
 


