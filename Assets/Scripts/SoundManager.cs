using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("오디오 소스")]
    [Tooltip("모스부호 사운드를 재생할 AudioSource")]
    public AudioSource morseAudioSource;

    [Header("볼륨 설정")]
    [Range(0f, 1f)]
    [Tooltip("마스터 볼륨")]
    public float masterVolume = 1f;

    [Range(0f, 1f)]
    [Tooltip("모스부호 볼륨")]
    public float morseVolume = 1f;

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

    /// <summary>
    /// 모스부호 사운드 재생
    /// </summary>
    public void PlayMorseSound()
    {
        if (morseAudioSource != null && morseAudioSource.clip != null)
        {
            morseAudioSource.volume = masterVolume * morseVolume;
            morseAudioSource.Play();
            Debug.Log($"[SoundManager] 모스부호 사운드 재생 (볼륨: {morseAudioSource.volume:F2})");
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

    /// <summary>
    /// 마스터 볼륨 설정
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }

    /// <summary>
    /// 모스부호 볼륨 설정
    /// </summary>
    public void SetMorseVolume(float volume)
    {
        morseVolume = Mathf.Clamp01(volume);
        if (morseAudioSource != null)
        {
            morseAudioSource.volume = masterVolume * morseVolume;
        }
    }

    /// <summary>
    /// 모든 AudioSource 볼륨 업데이트
    /// </summary>
    private void UpdateAllVolumes()
    {
        if (morseAudioSource != null)
        {
            morseAudioSource.volume = masterVolume * morseVolume;
        }
    }

    /// <summary>
    /// Inspector에서 값 변경 시 자동으로 볼륨 업데이트
    /// </summary>
    void OnValidate()
    {
        UpdateAllVolumes();
    }
}
