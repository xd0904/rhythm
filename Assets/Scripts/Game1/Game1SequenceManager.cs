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

    [Header("빠운싱 등장")]
    public GameObject Bounce;

    [Header("연출 등장")]
    public GameObject Control;

    [Header("마우스 애니메이션 설정")]
    [Tooltip("목표 위치 (월드 좌표 X, Y)")]
    public Vector2 targetPosition = new Vector2(7f, 0f);
    
    [Tooltip("이동 시간 (초)")]
    public float moveDuration = 2f;
    
    [Tooltip("최종 크기 (Scale)")]
    public float finalSize = 10f;
    
    [Tooltip("크기 변경 시간 (초) - 짧을수록 공포스러움")]
    public float scaleDuration = 0.5f;
    
    [Tooltip("흔들림 강도")]
    public float shakeIntensity = 10f;
    
    [Tooltip("회전 각도")]
    public float rotationAmount = 15f;
    
    [Header("음악 설정")]
    [Tooltip("마우스 애니메이션 후 재생할 BGM")]
    public AudioClip bgmClip;

    [Tooltip("마우스 이동 사운드")]
    public AudioClip mouseMove;

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
        
        // Game1 씬의 빨간 마우스 사용
        if (redMouse != null)
        {
            Debug.Log($"[Game1SequenceManager] redMouse 발견! 현재 상태: {(redMouse.activeSelf ? "활성화" : "비활성화")}");
            
            // 빨간 마우스 활성화
            redMouse.SetActive(true);
            Debug.Log($"[Game1SequenceManager] redMouse.SetActive(true) 호출 완료");
            
            // Intro 씬에서 저장한 위치로 설정
            RectTransform rectTransform = redMouse.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // UI 오브젝트면 anchoredPosition 사용
                rectTransform.anchoredPosition = savedPosition;
                Debug.Log($"[Game1SequenceManager] UI 마우스 위치 설정: {savedPosition}");
            }
            else
            {
                // World 오브젝트면 position 사용
                redMouse.transform.position = savedPosition;
                Debug.Log($"[Game1SequenceManager] World 마우스 위치 설정: {savedPosition}");
            }
            
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
            Debug.LogWarning("[Game1SequenceManager] 빨간 마우스가 없습니다!");
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
        
        // RectTransform인지 확인 (UI 오브젝트)
        RectTransform rectTransform = redMouse.GetComponent<RectTransform>();
        bool isUIObject = rectTransform != null;
        
        Transform mouseTransform = redMouse.transform;
        
        // 시작 위치 저장 (이미 RestoreMousePosition에서 Intro 씬 위치로 설정됨)
        Vector3 startPos;
        if (isUIObject)
        {
            startPos = rectTransform.anchoredPosition;
            Debug.Log($"[Game1SequenceManager] UI 오브젝트 감지 (RectTransform) - 시작 위치: {startPos}");
        }
        else
        {
            startPos = mouseTransform.position;
            Debug.Log($"[Game1SequenceManager] World Space 오브젝트 감지 (Transform) - 시작 위치: {startPos}");
        }
        
        Vector3 startScale = mouseTransform.localScale;
        
        // 목표 위치 (targetPosition 사용)
        Vector3 targetWorldPos = new Vector3(targetPosition.x, targetPosition.y, isUIObject ? 0 : startPos.z);
        
        Debug.Log($"[Game1SequenceManager] 마우스 이동 시작 - {startPos} → {targetWorldPos}");

        // 1단계: 위치 이동 (Ease-out 곡선)
        SoundManager.Instance.PlaySFX(mouseMove);

        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            if (redMouse == null) yield break;
            
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            
            // Ease-out 곡선 (부드러운 감속)
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);
            
            // 위치 보간
            Vector3 currentPos = Vector3.Lerp(startPos, targetWorldPos, smoothT);
            
            if (isUIObject)
            {
                rectTransform.anchoredPosition = currentPos;
            }
            else
            {
                mouseTransform.position = currentPos;
            }
            
            yield return null;
        }
        
        // 최종 위치 정확히 설정
        if (redMouse == null) yield break;
        
        if (isUIObject)
        {
            rectTransform.anchoredPosition = targetWorldPos;
        }
        else
        {
            mouseTransform.position = targetWorldPos;
        }
        
        Debug.Log($"[Game1SequenceManager] 마우스 이동 완료 - 최종 위치: {targetWorldPos}");
        
        // 2단계: 공포스럽게 크기 키우기 (SpriteRenderer는 localScale 사용)
        Vector3 targetScale = new Vector3(finalSize, finalSize, 1f);
        elapsed = 0f;
        
        Debug.Log($"[Game1SequenceManager] 공포스러운 크기 변경 시작 - {startScale} → {targetScale}");
        
        // 시작 회전 저장
        float startRotation = mouseTransform.localEulerAngles.z;
        
        while (elapsed < scaleDuration)
        {
            if (redMouse == null) yield break;
            
            elapsed += Time.deltaTime;
            float t = elapsed / scaleDuration;
            
            // 지수 함수로 더 급격하게 커지기 (공포스러움 증가)
            float exponentialT = Mathf.Pow(t, 0.3f); // 0.5 → 0.3으로 더 빠르게
            
            // 크기 보간 (localScale 사용)
            Vector3 currentScale = Vector3.Lerp(startScale, targetScale, exponentialT);
            mouseTransform.localScale = currentScale;
            
            // 흔들림 효과 추가 (무작위 offset)
            float shakeAmount = isUIObject ? 1f : 0.01f;
            float shakeX = Random.Range(-shakeIntensity, shakeIntensity) * (1f - t) * shakeAmount;
            float shakeY = Random.Range(-shakeIntensity, shakeIntensity) * (1f - t) * shakeAmount;
            
            if (isUIObject)
            {
                rectTransform.anchoredPosition = targetWorldPos + new Vector3(shakeX, shakeY, 0);
            }
            else
            {
                mouseTransform.position = targetWorldPos + new Vector3(shakeX, shakeY, 0);
            }
            
            // 회전 효과 (좌우로 흔들림)
            float rotation = Mathf.Sin(t * Mathf.PI * 4) * rotationAmount * (1f - t);
            mouseTransform.localEulerAngles = new Vector3(0, 0, startRotation + rotation);
            
            yield return null;
        }
        
        // 최종 상태 정확히 설정 (흔들림 제거)
        if (redMouse == null) yield break;
        
        mouseTransform.localScale = targetScale;
        
        if (isUIObject)
        {
            rectTransform.anchoredPosition = targetWorldPos;
        }
        else
        {
            mouseTransform.position = targetWorldPos;
        }
        
        mouseTransform.localEulerAngles = new Vector3(0, 0, startRotation);
        
        // 마우스 움직임 완전히 비활성화 (다시 확인)
        Mouse redMouseScript = redMouse.GetComponent<Mouse>();
        if (redMouseScript != null)
        {
            redMouseScript.enabled = false;
            Debug.Log("[Game1SequenceManager] 마우스 움직임 스크립트 비활성화 완료");
        }
        
        Debug.Log($"[Game1SequenceManager] 공포스러운 크기 변경 완료 - 최종 크기: {targetScale}");
        
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
            
            // BGM 루프 끄기 (노래 반복 안 되게)
            if (SoundManager.Instance.BGMSource != null)
            {
                SoundManager.Instance.BGMSource.loop = false;
                Debug.Log("[Game1SequenceManager] BGM Loop OFF");
                
                // 피치 초기화 (죽었다 재시작했을 때를 위해)
                SoundManager.Instance.BGMSource.pitch = 1.0f;
                Debug.Log("[Game1SequenceManager] BGM Pitch 초기화: 1.0");
                
                // ⚠️ BGM 볼륨 키우기
                SoundManager.Instance.BGMSource.volume = 1.0f;
                Debug.Log("[Game1SequenceManager] BGM Volume: 1.0 (최대)");
            }
        }
        else
        {
            Debug.LogWarning("[Game1SequenceManager] BGM Clip이 설정되지 않았습니다!");
        }

        Player.SetActive(true);
        Bounce.SetActive(true);
        Control.SetActive(true);
        
        // BeatBounce 음악 시작 시간 동기화
        BeatBounce beatBounce = Bounce.GetComponent<BeatBounce>();
        if (beatBounce != null)
        {
            beatBounce.ResetMusicStartTime();
            Debug.Log("[Game1SequenceManager] BeatBounce 음악 시간 동기화 완료");
        }
    }
    
    private void OnDestroy()
    {
        // 씬이 파괴될 때 마우스 커서 다시 보이게 (다음 씬을 위해)
        Cursor.visible = true;
        Debug.Log("[Game1SequenceManager] OnDestroy - 마우스 커서 복원");
    }
}
