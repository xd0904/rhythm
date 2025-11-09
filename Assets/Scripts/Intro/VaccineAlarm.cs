using UnityEngine;

public class VaccineAlarm : MonoBehaviour
{
    [Header("슬라이드 애니메이션 설정")]
    [Tooltip("슬라이드 애니메이션 지속 시간 (초)")]
    public float slideDuration = 0.5f;
    
    [Tooltip("화면에 표시되는 시간 (초)")]
    public float displayDuration = 8f;
    
    [Tooltip("화면 밖 시작 X 오프셋 (픽셀)")]
    public float offScreenOffset = 800f;
    
    [Tooltip("Ease 곡선 강도 (높을수록 부드러움)")]
    public float easeStrength = 2f;

    private RectTransform rectTransform;
    private Vector2 targetPosition;
    private bool isAnimating = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        if (rectTransform != null)
        {
            // 목표 위치 저장
            targetPosition = rectTransform.anchoredPosition;
            Debug.Log($"[VaccineAlarm] Awake 실행 - 목표 위치 저장: {targetPosition}");
        }
        else
        {
            Debug.LogError("[VaccineAlarm] RectTransform을 찾을 수 없습니다!");
        }
    }

    // GameSequenceManager에서 호출할 public 메서드
    public void TriggerAnimation()
    {
        Debug.Log($"[VaccineAlarm] TriggerAnimation 호출됨! 현재 활성화: {gameObject.activeSelf}, 애니메이션 중: {isAnimating}");
        
        if (!gameObject.activeSelf)
        {
            Debug.Log("[VaccineAlarm] 게임오브젝트 활성화");
            gameObject.SetActive(true);
        }
        
        if (!isAnimating)
        {
            if (rectTransform == null)
            {
                Debug.LogError("[VaccineAlarm] RectTransform이 null입니다! Awake가 실행되지 않았을 수 있습니다.");
                rectTransform = GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    targetPosition = rectTransform.anchoredPosition;
                    Debug.Log($"[VaccineAlarm] RectTransform 다시 찾음: {targetPosition}");
                }
            }
            
            Debug.Log("[VaccineAlarm] AnimationSequence 코루틴 시작");
            StartCoroutine(AnimationSequence());
        }
    }

    private System.Collections.IEnumerator AnimationSequence()
    {
        if (rectTransform == null)
        {
            Debug.LogError("[VaccineAlarm] RectTransform이 없습니다!");
            yield break;
        }

        Debug.Log("[VaccineAlarm] 애니메이션 시작");
        
        // 슬라이드 인
        yield return SlideIn();
        
        // 표시 시간만큼 대기
        Debug.Log($"[VaccineAlarm] {displayDuration}초 동안 표시");
        yield return new WaitForSeconds(displayDuration);
        
        // 슬라이드 아웃
        yield return SlideOutCoroutine();
    }

    private System.Collections.IEnumerator SlideIn()
    {
        if (isAnimating)
        {
            Debug.LogWarning("[VaccineAlarm] 이미 애니메이션 중입니다");
            yield break;
        }
        
        isAnimating = true;
        
        Vector2 startPosition = targetPosition + new Vector2(offScreenOffset, 0);
        rectTransform.anchoredPosition = startPosition;
        
        Debug.Log($"[VaccineAlarm] 슬라이드 인 시작: {startPosition} -> {targetPosition}");
        
        float elapsed = 0f;
        
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideDuration;
            
            // Ease-out 곡선 (부드러운 감속)
            float easedT = 1f - Mathf.Pow(1f - t, easeStrength);
            
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, easedT);
            
            yield return null;
        }
        
        rectTransform.anchoredPosition = targetPosition;
        isAnimating = false;
        
        Debug.Log("[VaccineAlarm] 슬라이드 인 완료");
    }

    private System.Collections.IEnumerator SlideOutCoroutine()
    {
        if (isAnimating)
        {
            Debug.LogWarning("[VaccineAlarm] 이미 애니메이션 중입니다");
            yield break;
        }
        
        isAnimating = true;
        
        Vector2 startPosition = rectTransform.anchoredPosition;
        Vector2 endPosition = targetPosition + new Vector2(offScreenOffset, 0);
        
        Debug.Log($"[VaccineAlarm] 슬라이드 아웃 시작: {startPosition} -> {endPosition}");
        
        float elapsed = 0f;
        
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideDuration;
            
            // Ease-in 곡선 (부드러운 가속)
            float easedT = Mathf.Pow(t, easeStrength);
            
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, easedT);
            
            yield return null;
        }
        
        rectTransform.anchoredPosition = endPosition;
        isAnimating = false;
        
        Debug.Log("[VaccineAlarm] 슬라이드 아웃 완료, 비활성화");
        
        // 위치를 원래대로 되돌린 후 비활성화
        rectTransform.anchoredPosition = targetPosition;
        gameObject.SetActive(false);
    }
}
