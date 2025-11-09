using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] AudioSource bgmSource;
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioSource noteSource;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float bgmVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 0.7f;

    [Header("Musics")]
    [SerializeField] List<AudioClip> musics;

    double music_start_time;
    
    // BGM AudioSource를 외부에서 접근할 수 있도록 public 프로퍼티 추가
    public AudioSource BGMSource => bgmSource;

    void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
            
            // 부모가 있다면 분리 (DontDestroyOnLoad는 루트 오브젝트만 가능)
            if (transform.parent != null)
            {
                Debug.Log($"[SoundManager] 부모({transform.parent.name})로부터 분리");
                transform.SetParent(null);
            }
            
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (bgmSource != null) bgmSource.volume = bgmVolume;
        if (sfxSource != null) sfxSource.volume = sfxVolume;
        if (noteSource != null) noteSource.volume = sfxVolume;
    }

    public void PlayBGM(AudioClip clip)
    {
        // bgmSource가 없으면 자동으로 생성
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
            Debug.Log("[SoundManager] BGM AudioSource 자동 생성");
        }
        
        // 같은 클립이지만 재생 중이 아니면 다시 재생
        if (bgmSource.clip == clip && bgmSource.isPlaying)
        {
            Debug.Log($"[SoundManager] BGM 이미 재생 중: {clip.name}");
            return;
        }
        
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
        Debug.Log($"[SoundManager] BGM 재생: {clip.name}");
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
            Debug.Log($"[SoundManager] SFX 재생: {clip.name}");
        }
    }

    public void ChangeBGM_Vol(Slider slider)
    {
        bgmVolume = slider.value;
    }
    
    public void ChangeSFX_Vol(Slider slider)
    {
        sfxVolume = slider.value;
    }

    public void OnMusicStart()
    {
        if (bgmSource == null) return;
        bgmSource.SetScheduledEndTime(AudioSettings.dspTime);
        // LevelManager가 있는 경우에만 사용
        // int level = LevelManager.Instance.currentLevel;
        music_start_time = AudioSettings.dspTime + 1.0f;
        // bgmSource.clip = musics[level];
        bgmSource.loop = false;
        bgmSource.PlayScheduled(music_start_time);
    }

    public void VolLerpZeroBGM()
    {
        StartCoroutine(LerpToZero(1));
    }

    IEnumerator LerpToZero(float duration)
    {
        float elapsed = 0f;
        float startValue = 0.7f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            bgmVolume = Mathf.Lerp(startValue, 0f, t);
            yield return null;
        }

        bgmVolume = 0;
    }
}