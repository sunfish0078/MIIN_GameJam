using ChoiJeongYun.Scripts.Enemy;
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
    
    [SerializeField] private AudioClip cuteSound;
    [SerializeField] private AudioClip cuteBGM;

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
    
    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (clip == null || bgmSource == null) return;

        if (bgmSource.clip == clip && bgmSource.isPlaying) return;
 
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }
    
    public void StopBGM()
    {
        if (bgmSource == null) return;
        bgmSource.Stop();
    }
    
    public void PlaySFX(AudioClip clip)
    {
        PlaySFX(clip, 1f);
    }
    
    public void PlaySFX(AudioClip clip, float volume)
    {
        if (clip == null || sfxSource == null) return;

        if (randomizeSfxPitch)
            sfxSource.pitch = Random.Range(pitchRangeMin, pitchRangeMax); 
        else
            sfxSource.pitch = 1f;

        sfxSource.PlayOneShot(clip, volume); 
    }

    public void StopSFX()
    {
        if (sfxSource == null) return;
        sfxSource.Stop();
    }
    
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
    
    public void StopAll()
    {
        StopBGM();
        StopSFX();
    }

    public void PlayUIClick()
    {
        PlaySFX(uiClickClip);
    }
    
    private AudioClip ResolveCowardClip(AudioClip normalClip, AudioClip cuteClip)
    {
        return DevMode.CowardMode && cuteClip != null ? cuteClip : normalClip;
    }

    public AudioClip ResolveMonsterSFX(AudioClip clip) => ResolveCowardClip(clip, cuteSound);

    public void PlayMonsterSFX(AudioClip clip) => PlaySFX(ResolveMonsterSFX(clip));

    public void PlayMonsterSFX(AudioClip clip, float volume) => PlaySFX(ResolveMonsterSFX(clip), volume);

    public void PlayAmbient(AudioClip clip, bool loop = true) => PlayBGM(ResolveCowardClip(clip, cuteBGM), loop);

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
        bgmSource.volume = startVolume; 
    }
}
 

