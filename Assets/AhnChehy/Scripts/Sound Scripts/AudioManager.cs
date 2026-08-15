using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
 
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
 
    // 볼륨 기본값 (처음 실행 시, 저장된 설정 없을 때)
    private const float DEFAULT_VOLUME = 1f;
 
    
    private const float MIN_VOLUME_THRESHOLD = 0.0001f;
    private const float MUTED_DB = -80f;
 
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
 
    //게임 시작할때 다이얼 비주얼, 각도/퍼센트 맞추는 용도로 쓰임
    #region Getters
 
    public float GetMasterVolume()
    {
        return PlayerPrefs.GetFloat(PREF_MASTER, DEFAULT_VOLUME);
    }
 
    public float GetBGMVolume()
    {
        return PlayerPrefs.GetFloat(PREF_BGM, DEFAULT_VOLUME);
    }
 
    public float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat(PREF_SFX, DEFAULT_VOLUME);
    }
 
    #endregion
 
    //값 바뀔때 VolumeDIal스크립트에서 값을 setting하는 용으로 쓰임
    #region Setters
 
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
 
        float dB = linearValue > MIN_VOLUME_THRESHOLD //audio mixer이 처리할수있게 반환해준 코드(잘 모름)
            ? Mathf.Log10(linearValue) * 20f
            : MUTED_DB;
 
        audioMixer.SetFloat(parameterName, dB);
    }
 
    #endregion
    
}
 
