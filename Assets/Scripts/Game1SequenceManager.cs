using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Game1 씬의 시퀀스를 관리하는 매니저
/// 이전 씬에서 저장된 마우스 위치를 복원합니다
/// </summary>
public class Game1SequenceManager : MonoBehaviour
{
    public static Game1SequenceManager Instance { get; private set; }
    
    [Header("마우스 설정")]
    [Tooltip("빨간 마우스 GameObject (Game1 씬에서는 일반 마우스 사용 안 함)")]
    public GameObject redMouse;

    [Header("플레이어 등장")]
    public GameObject Player;

    [Header("마우스 애니메이션 설정")]
    [Tooltip("목표 위치 (RectTransform X, Y)")]
    public Vector2 targetPosition = new Vector2(570f, -16f);
    
    [Tooltip("이동 시간 (초)")]
    public float moveDuration = 2f;
    
    [Tooltip("최종 크기 (Width, Height)")]
    public float finalSize = 5f;
    
    [Tooltip("크기 변경 시간 (초) - 짧을수록 공포스러움")]
    public float scaleDuration = 0.5f;
    
    [Tooltip("흔들림 강도")]
    public float shakeIntensity = 10f;
    
    [Tooltip("회전 각도")]
    public float rotationAmount = 15f;
    
    [Header("음악 설정")]
    [Tooltip("마우스 애니메이션 후 재생할 BGM")]
    public AudioClip bgmClip;
    
    private void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[Game1SequenceManager] 싱글톤 초기화");
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
        Debug.Log("[Game1SequenceManager] 실제 마우스 커서 숨김");
        
        // 씬 시작 시 이전 씬의 마우스 위치 복원
        Debug.Log($"[Game1SequenceManager] Start() 호출 - MousePositionData.Instance: {(MousePositionData.Instance != null ? "존재" : "NULL")}");
        RestoreMousePosition();
    }
    
    /// <summary>
    /// 이전 씬에서 저장된 마우스 위치 복원
    /// </summary>
    private void RestoreMousePosition()
    {
        if (MousePositionData.Instance == null)
        {
            Debug.LogWarning("[Game1SequenceManager] MousePositionData가 없습니다!");
            return;
        }
        
        Vector3 savedPosition = MousePositionData.Instance.GetSavedMousePosition();
        bool isRedMouse = MousePositionData.Instance.IsRedMouse();
        
        Debug.Log($"[Game1SequenceManager] 저장된 마우스 위치 복원: {savedPosition}, 빨간마우스: {isRedMouse}");
        
        // 위치가 (0,0,0)이면 저장된 게 없는 것
        if (savedPosition == Vector3.zero && !isRedMouse)
        {
            Debug.LogWarning("[Game1SequenceManager] 저장된 마우스 데이터가 없습니다! (0,0,0)");
            return;
        }
        
        // Game1 씬에서는 빨간 마우스만 사용
        if (isRedMouse && redMouse != null)
        {
            // 빨간 마우스 활성화
            redMouse.SetActive(true);
            redMouse.transform.position = savedPosition;
            
            // 빨간 마우스는 움직임 비활성화
            Mouse redMouseScript = redMouse.GetComponent<Mouse>();
            if (redMouseScript != null)
            {
                redMouseScript.enabled = false;
            }
            
            Debug.Log("[Game1SequenceManager] 빨간 마우스 위치 복원 완료 (고정 상태)");
            
            // 공포스러운 마우스 애니메이션 시작
            StartCoroutine(AnimateRedMouse());
        }
        else
        {
            Debug.LogWarning("[Game1SequenceManager] 빨간 마우스가 없거나 빨간 마우스 상태가 아닙니다!");
        }
    }
    
    /// <summary>
    /// 현재 마우스 위치를 저장하고 다른 씬으로 이동
    /// </summary>
    public void SaveMouseAndLoadScene(string sceneName)
    {
        if (MousePositionData.Instance != null && redMouse != null && redMouse.activeSelf)
        {
            // Game1 씬에서는 빨간 마우스만 사용
            MousePositionData.Instance.SaveMousePosition(redMouse.transform.position, true);
        }
        
        SceneManager.LoadScene(sceneName);
    }
    
    /// <summary>
    /// 빨간 마우스를 목표 위치로 이동하고 크기 키우기
    /// </summary>
    private IEnumerator AnimateRedMouse()
    {
        if (redMouse == null)
        {
            Debug.LogWarning("[Game1SequenceManager] 빨간 마우스가 없습니다!");
            yield break;
        }
        
        RectTransform rectTransform = redMouse.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogWarning("[Game1SequenceManager] RectTransform을 찾을 수 없습니다!");
            yield break;
        }
        
        // 시작 위치 및 크기 저장
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 startSize = rectTransform.sizeDelta;
        
        Debug.Log($"[Game1SequenceManager] 마우스 이동 시작 - {startPos} → {targetPosition}");
        
        // 1단계: 위치 이동 (Ease-out 곡선)
        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            
            // Ease-out 곡선 (부드러운 감속)
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);
            
            // 위치 보간
            Vector2 currentPos = Vector2.Lerp(startPos, targetPosition, smoothT);
            rectTransform.anchoredPosition = currentPos;
            
            yield return null;
        }
        
        // 최종 위치 정확히 설정
        rectTransform.anchoredPosition = targetPosition;
        Debug.Log($"[Game1SequenceManager] 마우스 이동 완료 - 최종 위치: {targetPosition}");
        
        // 2단계: 공포스럽게 크기 키우기
        Vector2 targetSize = new Vector2(finalSize, finalSize);
        elapsed = 0f;
        
        Debug.Log($"[Game1SequenceManager] 공포스러운 크기 변경 시작 - {startSize} → {targetSize}");
        
        // 시작 회전 저장
        float startRotation = rectTransform.localEulerAngles.z;
        
        while (elapsed < scaleDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / scaleDuration;
            
            // 지수 함수로 급격하게 커지기 (공포스러움 증가)
            float exponentialT = Mathf.Pow(t, 0.5f); // 제곱근으로 빠르게 시작
            
            // 크기 보간
            Vector2 currentSize = Vector2.Lerp(startSize, targetSize, exponentialT);
            
            // 흔들림 효과 추가 (무작위 offset)
            float shakeX = Random.Range(-shakeIntensity, shakeIntensity) * (1f - t); // 시간이 지날수록 감소
            float shakeY = Random.Range(-shakeIntensity, shakeIntensity) * (1f - t);
            
            rectTransform.sizeDelta = currentSize;
            rectTransform.anchoredPosition = targetPosition + new Vector2(shakeX, shakeY);
            
            // 회전 효과 (좌우로 흔들림)
            float rotation = Mathf.Sin(t * Mathf.PI * 4) * rotationAmount * (1f - t);
            rectTransform.localEulerAngles = new Vector3(0, 0, startRotation + rotation);
            
            yield return null;
        }
        
        // 최종 상태 정확히 설정 (흔들림 제거)
        rectTransform.sizeDelta = targetSize;
        rectTransform.anchoredPosition = targetPosition;
        rectTransform.localEulerAngles = new Vector3(0, 0, startRotation);
        
        // 마우스 움직임 완전히 비활성화 (다시 확인)
        Mouse redMouseScript = redMouse.GetComponent<Mouse>();
        if (redMouseScript != null)
        {
            redMouseScript.enabled = false;
            Debug.Log("[Game1SequenceManager] 마우스 움직임 스크립트 비활성화 완료");
        }
        
        Debug.Log($"[Game1SequenceManager] 공포스러운 크기 변경 완료 - 최종 크기: {targetSize}");
        
        // 애니메이션 완료 후 음악 시작
        StartMusic();
    }
    
    /// <summary>
    /// BGM 시작
    /// </summary>
    private void StartMusic()
    {
        if (SoundManager.Instance == null)
        {
            Debug.LogWarning("[Game1SequenceManager] SoundManager가 없습니다!");
            return;
        }
        
        if (bgmClip != null)
        {
            SoundManager.Instance.PlayBGM(bgmClip);
            Debug.Log($"[Game1SequenceManager] BGM 시작: {bgmClip.name}");
        }
        else
        {
            Debug.LogWarning("[Game1SequenceManager] BGM Clip이 설정되지 않았습니다!");
        }

        Player.SetActive(true);
    }
    
    private void OnDestroy()
    {
        // 씬이 파괴될 때 마우스 커서 다시 보이게 (다음 씬을 위해)
        Cursor.visible = true;
        Debug.Log("[Game1SequenceManager] OnDestroy - 마우스 커서 복원");
    }
}
