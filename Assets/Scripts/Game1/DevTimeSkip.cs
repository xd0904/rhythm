using UnityEngine;

/// <summary>
/// 개발자용 타임 스킵 스크립트
/// 키보드 숫자 키를 눌러 특정 시간대로 바로 이동
/// </summary>
public class DevTimeSkip : MonoBehaviour
{
    [Header("타임 스킵 설정")]
    public bool enableDevMode = true; // 개발 모드 활성화
    
    [Header("스킵 타임 설정 (초)")]
    public float skipTime1 = 90f;     // 1번 키: 1분 30초 (드래그 패턴 직전)
    public float skipTime2 = 96f;     // 2번 키: 1분 36초 (드래그 패턴 시작)
    public float skipTime3 = 108.8f;  // 3번 키: 1분 48.8초 (Rectangle 패턴 시작)
    public float skipTime4 = 121.6f;  // 4번 키: 2분 1.6초 (드래그 패턴 2차)
    public float skipTime5 = 134.4f;  // 5번 키: 2분 14.4초 (Rectangle 패턴 2차)
    public float skipTime6 = 147.2f;  // 6번 키: 2분 27.2초 (Rectangle 패턴 종료)
    public float skipTime7 = 147.6f;  // 7번 키: 2분 27.6초 (Blue Transformation 시작)
    
    void Update()
    {
        if (!enableDevMode) return;
        
        if (BeatBounce.Instance == null) return;
        
        // 1번 키: 1분 30초 (드래그 패턴 직전)
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            SkipToTime(skipTime1);
        }
        
        // 2번 키: 1분 36초 (드래그 패턴 시작)
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            SkipToTime(skipTime2);
        }
        
        // 3번 키: 1분 48.8초 (Rectangle 패턴 시작)
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            SkipToTime(skipTime3);
        }
        
        // 4번 키: 2분 1.6초 (드래그 패턴 2차)
        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            SkipToTime(skipTime4);
        }
        
        // 5번 키: 2분 14.4초 (Rectangle 패턴 2차)
        if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
        {
            SkipToTime(skipTime5);
        }
        
        // 6번 키: 2분 27.2초 (Rectangle 패턴 종료)
        if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
        {
            SkipToTime(skipTime6);
        }
        
        // 7번 키: 2분 27.6초 (Blue Transformation 시작)
        if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7))
        {
            SkipToTime(skipTime7);
        }
        
        // 0번 키: 처음으로 (0초)
        if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
        {
            SkipToTime(0f);
        }
        
        // Space 키: 일시정지/재생
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePause();
        }
    }
    
    void SkipToTime(float targetTime)
    {
        if (BeatBounce.Instance == null)
        {
            Debug.LogWarning("[DevTimeSkip] BeatBounce.Instance가 없습니다!");
            return;
        }
        
        // BeatBounce의 AudioSource 찾기
        AudioSource audioSource = BeatBounce.Instance.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("[DevTimeSkip] AudioSource를 찾을 수 없습니다!");
            return;
        }
        
        // 오디오 시간 설정
        audioSource.time = targetTime;
        
        Debug.Log($"[DevTimeSkip] 타임 스킵: {FormatTime(targetTime)} ({targetTime}초)");
    }
    
    void TogglePause()
    {
        if (BeatBounce.Instance == null) return;
        
        AudioSource audioSource = BeatBounce.Instance.GetComponent<AudioSource>();
        if (audioSource == null) return;
        
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            Debug.Log("[DevTimeSkip] 일시정지");
        }
        else
        {
            audioSource.UnPause();
            Debug.Log("[DevTimeSkip] 재생");
        }
    }
    
    string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        float remainingSeconds = seconds % 60f;
        return $"{minutes}:{remainingSeconds:F1}";
    }
    
    void OnGUI()
    {
        if (!enableDevMode) return;
        
        // 개발자 모드 안내 표시
        GUIStyle style = new GUIStyle();
        style.fontSize = 16;
        style.normal.textColor = Color.yellow;
        
        string helpText = "[개발자 모드]\n" +
                         "1: 1분 30초 (드래그 직전)\n" +
                         "2: 1분 36초 (드래그 시작)\n" +
                         "3: 1분 48.8초 (Rectangle 시작)\n" +
                         "4: 2분 1.6초 (드래그 2차)\n" +
                         "5: 2분 14.4초 (Rectangle 2차)\n" +
                         "6: 2분 27.2초 (Rectangle 종료)\n" +
                         "7: 2분 27.6초 (Blue Transform)\n" +
                         "0: 처음으로\n" +
                         "Space: 일시정지/재생";
        
        GUI.Label(new Rect(10, 10, 400, 300), helpText, style);
        
        // 현재 시간 표시
        if (BeatBounce.Instance != null)
        {
            float currentTime = (float)BeatBounce.Instance.GetMusicTime();
            string timeText = $"현재 시간: {FormatTime(currentTime)}";
            
            GUIStyle timeStyle = new GUIStyle();
            timeStyle.fontSize = 20;
            timeStyle.normal.textColor = Color.cyan;
            
            GUI.Label(new Rect(10, 320, 400, 30), timeText, timeStyle);
        }
    }
}
