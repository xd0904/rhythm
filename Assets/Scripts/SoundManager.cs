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
    [SerializeField] AudioSource morseAudioSource;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float bgmVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 0.7f;
    [Range(0f, 1f)] public float morseVolume = 1f;

    [Header("Musics")]
    [SerializeField] List<AudioClip> musics;

    double music_start_time;

    void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
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
        if (morseAudioSource != null) morseAudioSource.volume = morseVolume;
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
        
        if (bgmSource.clip == clip) return;
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
        Debug.Log($"[SoundManager] BGM 재생: {clip.name}");
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
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

    // === 모스부호 관련 기능 ===
    
    /// <summary>
    /// 모스부호 사운드 재생
    /// </summary>
    public void PlayMorseSound()
    {
        if (morseAudioSource != null && morseAudioSource.clip != null)
        {
            morseAudioSource.Play();
            Debug.Log($"[SoundManager] 모스부호 사운드 재생 (볼륨: {morseVolume:F2})");
        }
        else
        {
            Debug.LogWarning("[SoundManager] 모스부호 AudioSource 또는 Clip이 설정되지 않았습니다!");
        }
    }

    /// <summary>
    /// 모스부호 사운드 정지
    /// </summary>
    public void StopMorseSound()
    {
        if (morseAudioSource != null && morseAudioSource.isPlaying)
        {
            morseAudioSource.Stop();
            Debug.Log("[SoundManager] 모스부호 사운드 정지");
        }
    }

    /// <summary>
    /// 모스부호 AudioSource가 재생 중인지 확인
    /// </summary>
    public bool IsMorsePlaying()
    {
        return morseAudioSource != null && morseAudioSource.isPlaying;
    }

    /// <summary>
    /// 모스부호 오디오 길이 가져오기
    /// </summary>
    public float GetMorseClipLength()
    {
        if (morseAudioSource != null && morseAudioSource.clip != null)
        {
            return morseAudioSource.clip.length;
        }
        return 0f;
    }
}