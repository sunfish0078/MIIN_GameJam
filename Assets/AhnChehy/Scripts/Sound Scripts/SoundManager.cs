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
}
 

