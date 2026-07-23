using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour //Audio Mixer의 볼륨(Master / BGM / SFX)을 관리하는 스크립트.
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("UI Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    
    private const string PARAM_MASTER = "MasterVolume";
    private const string PARAM_BGM = "BGMVolume";
    private const string PARAM_SFX = "SFXVolume";
    
    private const string PREF_MASTER = "MasterVolume";
    private const string PREF_BGM = "BGMVolume";
    private const string PREF_SFX = "SFXVolume";

    // 볼륨 기본값 (한 75%)
    private const float DEFAULT_VOLUME = 0.75f;

    // dB 변환 시 완전 무음으로 처리할 최소 임계값
    private const float MIN_VOLUME_THRESHOLD = 0.0001f;
    private const float MUTED_DB = -80f;

    private void Start()
    {
        LoadAndApplySavedVolumes();
    }
    
    private void LoadAndApplySavedVolumes()
    {
        float masterVol = PlayerPrefs.GetFloat(PREF_MASTER, DEFAULT_VOLUME);
        float bgmVol = PlayerPrefs.GetFloat(PREF_BGM, DEFAULT_VOLUME);
        float sfxVol = PlayerPrefs.GetFloat(PREF_SFX, DEFAULT_VOLUME);

        ApplyToSliderOrDirectly(masterSlider, masterVol, SetMasterVolume);
        ApplyToSliderOrDirectly(bgmSlider, bgmVol, SetBGMVolume);
        ApplyToSliderOrDirectly(sfxSlider, sfxVol, SetSFXVolume);
    }

    private void ApplyToSliderOrDirectly(Slider slider, float value, System.Action<float> setter)
    {
        if (slider != null)
            slider.value = value; 
        else
            setter(value);
    }

    //UI 슬라이더의 OnValueChanged 이벤트에 연결할 공개 함수들
    #region Connect w/ sliders
    public void SetMasterVolume(float value)
    {
        ApplyVolume(PARAM_MASTER, value);
        PlayerPrefs.SetFloat(PREF_MASTER, value);
    }

    public void SetBGMVolume(float value)
    {
        ApplyVolume(PARAM_BGM, value);
        PlayerPrefs.SetFloat(PREF_BGM, value);
    }

    public void SetSFXVolume(float value)
    {
        ApplyVolume(PARAM_SFX, value);
        PlayerPrefs.SetFloat(PREF_SFX, value);
    }
    
    private void ApplyVolume(string parameterName, float linearValue)
    {
        if (audioMixer == null) return;

        float dB = linearValue > MIN_VOLUME_THRESHOLD
            ? Mathf.Log10(linearValue) * 20f
            : MUTED_DB;

        audioMixer.SetFloat(parameterName, dB);
    }
    #endregion
}
