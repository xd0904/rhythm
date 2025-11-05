using UnityEngine;

public class DebugShortcut : MonoBehaviour
{
    [Header("단축키 설정")]
    [Tooltip("빠른 속도 단축키 (기본: F2)")]
    public KeyCode fastSpeedKey = KeyCode.F2;
    
    [Tooltip("정상 속도 단축키 (기본: F3)")]
    public KeyCode normalSpeedKey = KeyCode.F3;
    
    [Header("속도 설정")]
    [Tooltip("빠른 모드 시간 배율")]
    public float fastTimeScale = 5f;

    void Update()
    {
        // F2: 빠른 속도
        if (Input.GetKeyDown(fastSpeedKey))
        {
            SetFastSpeed();
        }
        
        // F3: 정상 속도
        if (Input.GetKeyDown(normalSpeedKey))
        {
            SetNormalSpeed();
        }
    }

    private void SetFastSpeed()
    {
        Time.timeScale = fastTimeScale;
        AudioListener.volume = 0f; // 소리 끄기
        Debug.Log($"[DebugShortcut] 빠른 모드 활성화 (x{fastTimeScale}) - 소리 음소거");
    }

    private void SetNormalSpeed()
    {
        Time.timeScale = 1f;
        AudioListener.volume = 1f; // 소리 켜기
        Debug.Log("[DebugShortcut] 정상 속도로 복구 - 소리 복구");
    }

    void OnDestroy()
    {
        // 씬 전환 시 속도 및 소리 복구
        Time.timeScale = 1f;
        AudioListener.volume = 1f;
    }
}
