using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
 
    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("SFX Pitch Randomness")]
    [SerializeField] private bool randomizeSfxPitch = false;
    [SerializeField] private float pitchRangeMin = 0.95f;
    [SerializeField] private float pitchRangeMax = 1.05f;
    
    [SerializeField] private AudioClip uiClickClip;

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
    
    // BGM을 play함 이미 재생 또 재생 안하게 막음
    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (clip == null || bgmSource == null) return;

        if (bgmSource.clip == clip && bgmSource.isPlaying) return;
 
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }
 

    //브금 정지 함수
    public void StopBGM()
    {
        if (bgmSource == null) return;
        bgmSource.Stop();
    }
 

    //sfx
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;

        if (randomizeSfxPitch)
            sfxSource.pitch = Random.Range(pitchRangeMin, pitchRangeMax); //혹시 랜덤한 소리 피치를 원할수도 있어서 추가
        else
            sfxSource.pitch = 1f;
 
        sfxSource.PlayOneShot(clip); //playOneshot으로 클립 교체 할필요 없당
    }

    public void StopSFX()
    {
        if (sfxSource == null) return;
        sfxSource.Stop();
    }

    // 재생 중인 효과음(긴 울음소리 등)을 서서히 줄이며 끔
    public void FadeOutSFX(float duration)
    {
        if (sfxSource == null) return;
        StartCoroutine(FadeOutSFXRoutine(duration));
    }

    private IEnumerator FadeOutSFXRoutine(float duration)
    {
        float startVolume = sfxSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            sfxSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        sfxSource.volume = 0f;
        sfxSource.Stop();
        sfxSource.volume = startVolume;
    }

    // 씬(게임 클리어 등) 넘어갈 때 재생 중이던 소리 전부 끔
    public void StopAll()
    {
        StopBGM();
        StopSFX();
    }

    public void PlayUIClick()
    {
        PlaySFX(uiClickClip);
    }

    // 반복 재생되는 배경 소음(환풍기 등) - bgmSource 재사용
    public void PlayAmbient(AudioClip clip, bool loop = true)
    {
        PlayBGM(clip, loop);
    }

    public void StopAmbient()
    {
        StopBGM();
    }

    public void FadeOutAmbient(float duration)
    {
        if (bgmSource == null) return;
        StartCoroutine(FadeOutAmbientRoutine(duration));
    }

    private IEnumerator FadeOutAmbientRoutine(float duration)
    {
        float startVolume = bgmSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        bgmSource.volume = 0f;
        bgmSource.Stop();
        bgmSource.volume = startVolume; // 다음 재생을 위해 볼륨 복구
    }
}
 

