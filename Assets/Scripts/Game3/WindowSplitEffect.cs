using UnityEngine;
using System.Collections;

/// <summary>
/// Game3에서 32초에 1개 창이 3x3 (9개) 창으로 분열되는 효과
/// </summary>
public class WindowSplitEffect : MonoBehaviour
{
    [Header("타이밍 설정")]
    [Tooltip("분열 시작 시간 (초)")]
    public float splitTime = 32f;
    
    [Header("창 오브젝트")]
    [Tooltip("원본 창 (분열 전)")]
    public GameObject originalWindow;
    
    [Tooltip("분열된 창 프리팹")]
    public GameObject splitWindowPrefab;
    
    [Header("그리드 설정")]
    [Tooltip("왼쪽 위 창 위치")]
    public Vector3 topLeftPosition = new Vector3(-380f, 360f, 0f);
    
    [Tooltip("오른쪽 아래 창 위치")]
    public Vector3 bottomRightPosition = new Vector3(380f, -360f, 0f);
    
    [Tooltip("분열 애니메이션 시간 (초)")]
    public float splitDuration = 0.3f;
    
    [Header("Canvas 설정")]
    [Tooltip("생성된 창들이 들어갈 Canvas")]
    public Canvas targetCanvas;
    
    private bool hasTriggered = false;
    private GameObject[,] splitWindows; // 3x3 창 배열
    
    void Update()
    {
        if (hasTriggered) return;
        
        // Game3SequenceManager의 DSP 시간 사용
        if (Game3SequenceManager.Instance == null) return;
        
        double musicTime = Game3SequenceManager.Instance.GetMusicTime();
        
        // 32초에 분열 시작
        if (musicTime >= splitTime)
        {
            hasTriggered = true;
            Debug.Log($"[WindowSplitEffect] ========== 분열 시작! ========== musicTime: {musicTime}");
            StartCoroutine(SplitWindowAnimation());
        }
    }
    
    /// <summary>
    /// 창 분열 애니메이션
    /// </summary>
    private IEnumerator SplitWindowAnimation()
    {
        if (originalWindow == null || splitWindowPrefab == null)
        {
            Debug.LogError("[WindowSplitEffect] originalWindow 또는 splitWindowPrefab이 없습니다!");
            yield break;
        }
        
        // 1. 원본 창 정보 저장
        RectTransform originalRect = originalWindow.GetComponent<RectTransform>();
        Vector2 centerPosition;
        Vector3 originalScale;
        
        if (originalRect != null)
        {
            // UI Canvas 오브젝트인 경우
            centerPosition = originalRect.anchoredPosition;
            originalScale = originalRect.localScale;
            Debug.Log($"[WindowSplitEffect] 원본 창 (UI) anchoredPosition: {centerPosition}, scale: {originalScale}");
        }
        else
        {
            // 일반 Transform 오브젝트인 경우 (GameProgram 등)
            centerPosition = originalWindow.transform.localPosition;
            originalScale = originalWindow.transform.localScale;
            Debug.Log($"[WindowSplitEffect] 원본 창 (World) localPosition: {centerPosition}, scale: {originalScale}");
        }
        
        // 2. 3x3 그리드 위치 계산
        Vector3[,] gridPositions = CalculateGridPositions();
        
        // 3. 9개 창 생성 (중앙에서 시작)
        splitWindows = new GameObject[3, 3];
        
        // Canvas 찾기
        Transform parentTransform = (targetCanvas != null) ? targetCanvas.transform : null;
        
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                GameObject window;
                if (parentTransform != null)
                {
                    // Instantiate할 때 바로 부모를 지정합니다.
                    window = Instantiate(splitWindowPrefab, parentTransform);
                }
                else
                {
                    window = Instantiate(splitWindowPrefab);
                }
                
                window.name = $"SplitWindow_{row}_{col}";
                splitWindows[row, col] = window;

                RectTransform rectTransform = window.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    // UI 프리팹이면 anchoredPosition 사용
                    rectTransform.localScale = originalScale;
                    rectTransform.anchoredPosition = centerPosition;
                    Debug.Log($"[WindowSplitEffect] 창 생성: {window.name} anchoredPosition: {rectTransform.anchoredPosition}, scale: {rectTransform.localScale}");
                }
                else
                {
                    // 일반 Transform 프리팹이면 localPosition 사용
                    window.transform.localScale = originalScale;
                    window.transform.localPosition = centerPosition;
                    Debug.Log($"[WindowSplitEffect] 창 생성: {window.name} localPosition: {window.transform.localPosition}, scale: {window.transform.localScale}");
                }
            }
        }
        
        // 4. 원본 창 숨기기
        originalWindow.SetActive(false);
        Debug.Log("[WindowSplitEffect] 원본 창 비활성화");
        
        // 5. 분열 애니메이션: 중앙에서 각자 위치로 이동
        float elapsed = 0f;
        
        while (elapsed < splitDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / splitDuration;
            
            // Ease-out 곡선
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);
            
            // 각 창을 목표 위치로 이동
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    if (splitWindows[row, col] != null)
                    {
                        Vector3 targetPos = gridPositions[row, col];
                        
                        // RectTransform 사용
                        RectTransform rectTransform = splitWindows[row, col].GetComponent<RectTransform>();
                        if (rectTransform != null)
                        {
                            rectTransform.anchoredPosition = Vector3.Lerp(centerPosition, targetPos, smoothT);
                        }
                        else
                        {
                            splitWindows[row, col].transform.localPosition = Vector3.Lerp(centerPosition, targetPos, smoothT);
                        }
                    }
                }
            }
            
            yield return null;
        }
        
        // 6. 최종 위치 정확히 설정
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                if (splitWindows[row, col] != null)
                {
                    // RectTransform 사용
                    RectTransform rectTransform = splitWindows[row, col].GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.anchoredPosition = gridPositions[row, col];
                    }
                    else
                    {
                        splitWindows[row, col].transform.localPosition = gridPositions[row, col];
                    }
                }
            }
        }
        
        Debug.Log("[WindowSplitEffect] 분열 애니메이션 완료!");
    }
    
    /// <summary>
    /// 3x3 그리드 위치 계산
    /// </summary>
    private Vector3[,] CalculateGridPositions()
    {
        Vector3[,] positions = new Vector3[3, 3];
        
        // 왼쪽 위 (-380, 360) ~ 오른쪽 아래 (380, -360)
        // Canvas UI 좌표계 사용
        float minX = topLeftPosition.x;
        float maxX = bottomRightPosition.x;
        float maxY = topLeftPosition.y; // Y는 위가 큼
        float minY = bottomRightPosition.y;
        
        // 간격 계산 (3개로 나누므로 2개의 간격)
        float spacingX = (maxX - minX) / 2f;
        float spacingY = (maxY - minY) / 2f;
        
        Debug.Log($"[WindowSplitEffect] X 범위: {minX} ~ {maxX}, 간격: {spacingX}");
        Debug.Log($"[WindowSplitEffect] Y 범위: {minY} ~ {maxY}, 간격: {spacingY}");
        
        // 위에서 아래로, 왼쪽에서 오른쪽으로
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                float x = minX + (col * spacingX);
                float y = maxY - (row * spacingY);
                
                positions[row, col] = new Vector3(x, y, 0f);
                
                Debug.Log($"[WindowSplitEffect] Grid[{row},{col}] = ({x}, {y})");
            }
        }
        
        return positions;
    }
    
    /// <summary>
    /// 분열된 창들 가져오기 (외부에서 접근용)
    /// </summary>
    public GameObject[,] GetSplitWindows()
    {
        return splitWindows;
    }
    
    /// <summary>
    /// 특정 위치의 창 가져오기
    /// </summary>
    public GameObject GetWindow(int row, int col)
    {
        if (splitWindows == null || row < 0 || row >= 3 || col < 0 || col >= 3)
        {
            return null;
        }
        
        return splitWindows[row, col];
    }
}
