using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Game3 씬의 시퀀스를 관리하는 매니저
/// 이전 씬에서 저장된 마우스 위치를 복원합니다
/// 하얀 마우스를 그대로 사용하고 움직일 수 있게 합니다
/// </summary>
public class Game3SequenceManager : MonoBehaviour
{
    public static Game3SequenceManager Instance { get; private set; }
    
    [Header("마우스 설정")]
    [Tooltip("하얀 마우스 GameObject (일반 마우스)")]
    public GameObject whiteMouse;

    [Header("음악 설정")]
    [Tooltip("씬 시작 시 재생할 BGM")]
    public AudioClip bgmClip;
    
    [Header("타이밍 설정")]
    [Tooltip("BPM (비트 속도)")]
    public float bpm = 180f;
    
    private double musicStartTime = 0; // DSP 기준 음악 시작 시간
    private float beatInterval; // 한 박자 간격
    private int lastBeatIndex = -1; // 마지막 박자 인덱스
    
    private void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[Game3SequenceManager] 싱글톤 초기화");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // BPM 설정
        beatInterval = 60f / bpm;
        Debug.Log($"[Game3SequenceManager] BPM: {bpm}, 박자 간격: {beatInterval:F3}초");
        
        // 실제 마우스 커서 숨기기
        Cursor.visible = false;
        Debug.Log("[Game3SequenceManager] 실제 마우스 커서 숨김");
        
        // 씬 시작 시 이전 씬의 마우스 위치 복원
        Debug.Log($"[Game3SequenceManager] Start() 호출 - MousePositionData.Instance: {(MousePositionData.Instance != null ? "존재" : "NULL")}");
        
        RestoreMousePosition();
        
        // BGM 시작
        StartMusic();
    }
    
    private void Update()
    {
        // 음악 시작 전에는 아무것도 안 함
        if (musicStartTime == 0) return;
        
        // 현재 음악 시간 계산 (DSP 기반)
        double currentMusicTime = GetMusicTime();
        
        // 현재 비트 인덱스 계산
        int currentBeatIndex = Mathf.FloorToInt((float)(currentMusicTime / beatInterval));
        
        // 새로운 비트가 왔을 때만 실행
        if (currentBeatIndex > lastBeatIndex)
        {
            lastBeatIndex = currentBeatIndex;
            Debug.Log($"[Game3SequenceManager] 박자 {currentBeatIndex} - 시간: {currentMusicTime:F2}초");
        }
    }
    
    /// <summary>
    /// 현재 음악 시간 가져오기 (DSP 기반)
    /// </summary>
    public double GetMusicTime()
    {
        if (musicStartTime == 0) return 0;
        return AudioSettings.dspTime - musicStartTime;
    }
    
    /// <summary>
    /// 음악 시작 시간 리셋 (BGM 재생 시 호출)
    /// </summary>
    public void ResetMusicStartTime()
    {
        musicStartTime = AudioSettings.dspTime;
        lastBeatIndex = -1;
        Debug.Log($"[Game3SequenceManager] 음악 시작 시간 리셋: {musicStartTime}");
    }
    
    /// <summary>
    /// 이전 씬에서 저장된 마우스 위치 복원
    /// </summary>
    private void RestoreMousePosition()
    {
        if (MousePositionData.Instance == null)
        {
            Debug.LogWarning("[Game3SequenceManager] MousePositionData가 없습니다!");
            return;
        }
        
        Vector3 savedPosition = MousePositionData.Instance.GetSavedMousePosition();
        bool isRedMouse = MousePositionData.Instance.IsRedMouse();
        
        Debug.Log($"[Game3SequenceManager] 저장된 마우스 위치 복원: {savedPosition}, 빨간마우스: {isRedMouse}");
        
        // Game3 씬의 하얀 마우스 사용
        if (whiteMouse != null)
        {
            Debug.Log($"[Game3SequenceManager] whiteMouse 발견! 현재 상태: {(whiteMouse.activeSelf ? "활성화" : "비활성화")}");
            
            // 하얀 마우스 활성화
            whiteMouse.SetActive(true);
            Debug.Log($"[Game3SequenceManager] whiteMouse.SetActive(true) 호출 완료");
            
            // 이전 씬에서 저장한 위치로 설정
            RectTransform rectTransform = whiteMouse.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // UI 오브젝트면 anchoredPosition 사용
                rectTransform.anchoredPosition = savedPosition;
                Debug.Log($"[Game3SequenceManager] UI 마우스 위치 설정: {savedPosition}");
            }
            else
            {
                // World 오브젝트면 position 사용
                whiteMouse.transform.position = savedPosition;
                Debug.Log($"[Game3SequenceManager] World 마우스 위치 설정: {savedPosition}");
            }
            
            // 하얀 마우스는 움직임 활성화 (플레이어가 조작 가능)
            Mouse whiteMouseScript = whiteMouse.GetComponent<Mouse>();
            if (whiteMouseScript != null)
            {
                whiteMouseScript.enabled = true;
                Debug.Log("[Game3SequenceManager] 하얀 마우스 움직임 활성화 (조작 가능)");
            }
            
            Debug.Log("[Game3SequenceManager] 하얀 마우스 위치 복원 완료 (움직임 가능)");
        }
        else
        {
            Debug.LogWarning("[Game3SequenceManager] 하얀 마우스가 없습니다!");
        }
    }
    
    /// <summary>
    /// 현재 마우스 위치를 저장하고 다른 씬으로 이동
    /// </summary>
    public void SaveMouseAndLoadScene(string sceneName)
    {
        if (MousePositionData.Instance != null && whiteMouse != null && whiteMouse.activeSelf)
        {
            // Game3 씬에서는 하얀 마우스 사용 (빨간색 아님)
            MousePositionData.Instance.SaveMousePosition(whiteMouse.transform.position, false);
        }
        
        SceneManager.LoadScene(sceneName);
    }
    
    /// <summary>
    /// BGM 시작
    /// </summary>
    private void StartMusic()
    {
        if (SoundManager.Instance == null)
        {
            Debug.LogWarning("[Game3SequenceManager] SoundManager가 없습니다!");
            return;
        }
        
        if (bgmClip != null)
        {
            SoundManager.Instance.PlayBGM(bgmClip);
            Debug.Log($"[Game3SequenceManager] BGM 시작: {bgmClip.name}");
            
            // BGM 설정
            if (SoundManager.Instance.BGMSource != null)
            {
                SoundManager.Instance.BGMSource.loop = false;
                Debug.Log("[Game3SequenceManager] BGM Loop OFF");
                
                // 피치 초기화
                SoundManager.Instance.BGMSource.pitch = 1.0f;
                Debug.Log("[Game3SequenceManager] BGM Pitch 초기화: 1.0");
                
                // BGM 볼륨 설정
                SoundManager.Instance.BGMSource.volume = 1.0f;
                Debug.Log("[Game3SequenceManager] BGM Volume: 1.0 (최대)");
            }
            
            // 음악 시작 시간 기록
            ResetMusicStartTime();
        }
        else
        {
            Debug.LogWarning("[Game3SequenceManager] BGM Clip이 설정되지 않았습니다!");
        }
    }
    
    private void OnDestroy()
    {
        // 씬이 파괴될 때 마우스 커서 다시 보이게 (다음 씬을 위해)
        Cursor.visible = true;
        Debug.Log("[Game3SequenceManager] OnDestroy - 마우스 커서 복원");
    }
}
