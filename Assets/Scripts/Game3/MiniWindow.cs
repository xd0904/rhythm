using System.Collections;
using UnityEngine;

public class MiniWindow : MonoBehaviour
{
    [Header("버튼 상태 게임오브젝트")]
    [Tooltip("기본 상태 버튼")]
    public GameObject normalButton;

    [Tooltip("호버 상태 버튼")]
    public GameObject hoveredButton;

    [Tooltip("클릭 상태 버튼")]
    public GameObject activeButton;

    [Header("애니메이션 설정")]
    public float spawnDuration = 0.3f; // 창이 뜨는 시간
    public AnimationCurve spawnCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 이징 커브

    [Header("효과 설정")]
    public bool usePopEffect = true; // 팝업 효과 사용 여부
    public float overshoot = 1.1f; // 오버슈트 크기 (1.0보다 크면 튀어오르는 효과)

    private Vector3 targetScale;
    private CanvasGroup canvasGroup;
    private bool isClosing = false;

    void Start()
    {
        // 버튼 초기 상태 설정
        SetButtonState(ButtonState.Normal);

        // 초기 스케일 저장 및 0으로 설정
        targetScale = transform.localScale;
        transform.localScale = Vector3.zero;

        // CanvasGroup으로 페이드 효과 (있으면)
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        // 창 뜨는 애니메이션 시작
        StartCoroutine(SpawnAnimation());
    }

    void OnMouseEnter()
    {
        if (isClosing) return;
        SetButtonState(ButtonState.Hovered);
    }

    void OnMouseExit()
    {
        if (isClosing) return;
        SetButtonState(ButtonState.Normal);
    }

    void OnMouseDown()
    {
        if (isClosing) return;
        SetButtonState(ButtonState.Active);
    }

    void OnMouseUp()
    {
        if (isClosing) return;

        // X 버튼 클릭 시 닫기 애니메이션 시작
        CloseWindow();
        SetButtonState(ButtonState.Hovered);
    }

    IEnumerator SpawnAnimation()
    {
        float elapsed = 0f;

        while (elapsed < spawnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / spawnDuration;
            float curveValue = spawnCurve.Evaluate(t);

            // 스케일 애니메이션 (오버슈트 효과)
            if (usePopEffect && t < 0.7f)
            {
                float scale = curveValue * overshoot;
                transform.localScale = targetScale * scale;
            }
            else if (usePopEffect && t >= 0.7f)
            {
                // 오버슈트 후 원래 크기로 복귀
                float backT = (t - 0.7f) / 0.3f;
                float scale = Mathf.Lerp(overshoot, 1f, backT);
                transform.localScale = targetScale * scale;
            }
            else
            {
                transform.localScale = targetScale * curveValue;
            }

            // 페이드 인 효과
            if (canvasGroup != null)
            {
                canvasGroup.alpha = curveValue;
            }

            yield return null;
        }

        // 최종 크기로 설정
        transform.localScale = targetScale;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }

    private void CloseWindow()
    {
        if (isClosing) return;
        isClosing = true;

        StartCoroutine(CloseAnimation());
    }

    IEnumerator CloseAnimation()
    {
        float elapsed = 0f;
        float closeDuration = spawnDuration * 0.5f; // 닫을 때는 더 빠르게

        Vector3 startScale = transform.localScale;
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;

        while (elapsed < closeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / closeDuration;

            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            }

            yield return null;
        }

        // 패턴 매니저에게 창이 닫혔음을 알림
        Boom patternManager = FindFirstObjectByType<Boom>();
        if (patternManager != null)
        {
            patternManager.OnWindowClosed(gameObject); // 여기서 gameObject를 전달
        }

        Destroy(gameObject);
    }

    private void SetButtonState(ButtonState state)
    {
        if (normalButton != null) normalButton.SetActive(state == ButtonState.Normal);
        if (hoveredButton != null) hoveredButton.SetActive(state == ButtonState.Hovered);
        if (activeButton != null) activeButton.SetActive(state == ButtonState.Active);
    }

    private enum ButtonState
    {
        Normal,
        Hovered,
        Active
    }

}
