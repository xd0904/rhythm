using UnityEngine;

public class ExeProgram : MonoBehaviour
{
    [Header("열림 애니메이션 설정")]
    [Tooltip("애니메이션 지속 시간 (초)")]
    public float animationDuration = 0.2f;
    
    [Tooltip("시작 크기 비율 (0~1)")]
    public float startScale = 0.5f;

    [Header("UI 요소")]
    [Tooltip("Vaccine 아이콘 오브젝트")]
    public GameObject VaccineIcon;

    private Vector3 targetScale;
    private RectTransform rectTransform;
    private bool initialized = false;

    void Awake()
    {
        Initialize();
    }

    void OnEnable()
    {
        // 초기화가 안되어 있으면 먼저 초기화
        if (!initialized)
        {
            Initialize();
        }
        
        // 게임오브젝트가 활성화될 때 애니메이션 시작
        StartCoroutine(OpenAnimation());
    }

    private void Initialize()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            targetScale = rectTransform.localScale;
        }
        else
        {
            targetScale = transform.localScale;
        }
        initialized = true;
    }

    private System.Collections.IEnumerator OpenAnimation()
    {
        // 시작 크기 설정
        Vector3 initialScale = targetScale * startScale;
        
        if (rectTransform != null)
        {
            rectTransform.localScale = initialScale;
        }
        else
        {
            transform.localScale = initialScale;
        }

        // 애니메이션
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            
            // Ease-out 효과 (빠르게 시작해서 천천히 끝남)
            float easedT = 1f - Mathf.Pow(1f - t, 3f);
            
            Vector3 currentScale = Vector3.Lerp(initialScale, targetScale, easedT);
            
            if (rectTransform != null)
            {
                rectTransform.localScale = currentScale;
            }
            else
            {
                transform.localScale = currentScale;
            }
            
            yield return null;
        }

        // 최종 크기로 정확히 설정
        if (rectTransform != null)
        {
            rectTransform.localScale = targetScale;
        }
        else
        {
            transform.localScale = targetScale;
        }
    }
}
