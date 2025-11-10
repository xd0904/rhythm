using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Game2 씬의 시퀀스를 관리하는 매니저
/// </summary>
public class Game2SequenceManager : MonoBehaviour
{
    public static Game2SequenceManager Instance { get; private set; }
    
    [Header("마우스 설정")]
    [Tooltip("마우스 GameObject")]
    public GameObject mouse;
    
    [Header("음악 설정")]
    [Tooltip("씬 시작 시 재생할 BGM")]
    public AudioClip bgmClip;
    
    private void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[Game2SequenceManager] 싱글톤 초기화");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // 실제 마우스 커서 숨기기
        Cursor.visible = false;
        Debug.Log("[Game2SequenceManager] 실제 마우스 커서 숨김");
        
        // 씬 시작 시 이전 씬의 마우스 위치 복원
        Debug.Log($"[Game2SequenceManager] Start() 호출 - MousePositionData.Instance: {(MousePositionData.Instance != null ? "존재" : "NULL")}");
        
        RestoreMousePosition();
        
        // BGM 시작
        StartMusic();
    }
    
    /// <summary>
    /// 이전 씬에서 저장된 마우스 위치 복원
    /// </summary>
    private void RestoreMousePosition()
    {
        if (MousePositionData.Instance == null)
        {
            Debug.LogWarning("[Game2SequenceManager] MousePositionData가 없습니다!");
            return;
        }
        
        Vector3 savedPosition = MousePositionData.Instance.GetSavedMousePosition();
        bool isRedMouse = MousePositionData.Instance.IsRedMouse();
        
        Debug.Log($"[Game2SequenceManager] 저장된 마우스 위치 복원: {savedPosition}, 빨간마우스: {isRedMouse}");
        
        // 마우스 사용
        if (mouse != null)
        {
            Debug.Log($"[Game2SequenceManager] mouse 발견! 현재 상태: {(mouse.activeSelf ? "활성화" : "비활성화")}");
            
            // 마우스 활성화
            mouse.SetActive(true);
            Debug.Log($"[Game2SequenceManager] mouse.SetActive(true) 호출 완료");
            
            // 이전 씬에서 저장한 위치로 설정
            RectTransform rectTransform = mouse.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // UI 오브젝트면 anchoredPosition 사용
                rectTransform.anchoredPosition = savedPosition;
                Debug.Log($"[Game2SequenceManager] UI 마우스 위치 설정: {savedPosition}");
            }
            else
            {
                // World 오브젝트면 position 사용
                mouse.transform.position = savedPosition;
                Debug.Log($"[Game2SequenceManager] World 마우스 위치 설정: {savedPosition}");
            }
            
            // 마우스 움직임 활성화
            Mouse mouseScript = mouse.GetComponent<Mouse>();
            if (mouseScript != null)
            {
                mouseScript.enabled = true;
                Debug.Log("[Game2SequenceManager] 마우스 움직임 활성화");
            }
            
            Debug.Log("[Game2SequenceManager] 마우스 위치 복원 완료");
        }
        else
        {
            Debug.LogWarning("[Game2SequenceManager] 마우스가 없습니다!");
        }
    }
    
    /// <summary>
    /// 현재 마우스 위치를 저장하고 다른 씬으로 이동
    /// </summary>
    public void SaveMouseAndLoadScene(string sceneName)
    {
        if (MousePositionData.Instance != null && mouse != null && mouse.activeSelf)
        {
            RectTransform rectTransform = mouse.GetComponent<RectTransform>();
            Vector3 positionToSave;
            
            if (rectTransform != null)
            {
                positionToSave = rectTransform.anchoredPosition;
            }
            else
            {
                positionToSave = mouse.transform.position;
            }
            
            // 마우스 색상 확인 (빨간색인지 하얀색인지)
            bool isRed = false;
            SpriteRenderer sr = mouse.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                // 빨간색 계열이면 true
                isRed = sr.color.r > 0.8f && sr.color.g < 0.3f && sr.color.b < 0.3f;
            }
            
            MousePositionData.Instance.SaveMousePosition(positionToSave, isRed);
            Debug.Log($"[Game2SequenceManager] 마우스 위치 저장: {positionToSave}, 빨간색: {isRed}");
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
            Debug.LogWarning("[Game2SequenceManager] SoundManager가 없습니다!");
            return;
        }
        
        if (bgmClip != null)
        {
            SoundManager.Instance.PlayBGM(bgmClip);
            Debug.Log($"[Game2SequenceManager] BGM 시작: {bgmClip.name}");
            
            // BGM 설정
            if (SoundManager.Instance.BGMSource != null)
            {
                SoundManager.Instance.BGMSource.loop = false;
                Debug.Log("[Game2SequenceManager] BGM Loop OFF");
                
                // 피치 초기화
                SoundManager.Instance.BGMSource.pitch = 1.0f;
                Debug.Log("[Game2SequenceManager] BGM Pitch 초기화: 1.0");
                
                // BGM 볼륨 설정
                SoundManager.Instance.BGMSource.volume = 1.0f;
                Debug.Log("[Game2SequenceManager] BGM Volume: 1.0 (최대)");
            }
        }
        else
        {
            Debug.Log("[Game2SequenceManager] BGM Clip이 설정되지 않음 (선택사항)");
        }
    }
    
    private void OnDestroy()
    {
        // 씬이 파괴될 때 마우스 커서 다시 보이게 (다음 씬을 위해)
        Cursor.visible = true;
        Debug.Log("[Game2SequenceManager] OnDestroy - 마우스 커서 복원");
    }
}
