using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Game3에서 32초~42.7초 동안 에러 패턴을 관리
/// 16박자 패턴을 2번 반복 (총 32박자)
/// </summary>
public class ErrorPatternManager : MonoBehaviour
{
    [Header("타이밍 설정")]
    [Tooltip("패턴 시작 박자 (Game3SequenceManager 기준)")]
    public int patternStartBeat = 96; // 32초 = 96박자 (180 BPM 기준)
    
    [Tooltip("패턴 종료 박자")]
    public int patternEndBeat = 128; // 42.7초 ≈ 128박자
    
    [Header("에러 프리팹")]
    [Tooltip("에러 프리팹 (느낌표 사각형)")]
    public GameObject errorPrefab;
    
    [Header("Canvas 설정")]
    [Tooltip("에러가 생성될 Canvas")]
    public Canvas targetCanvas;
    
    [Header("애니메이션 설정")]
    [Tooltip("에러 등장 시간 (초)")]
    public float errorAppearDuration = 0.1f;
    
    [Tooltip("에러 유지 시간 - 유지되다가 펑할때 같이 터짐")]
    public float errorHoldDuration = 10f;
    
    [Tooltip("에러 폭발 시간 (초) - 2박자")]
    public float errorExplodeDuration = 0.666f; // 180 BPM 기준 2박자 = 0.666초
    
    [Tooltip("에러 최종 스케일 X")]
    public float errorScaleX = 0.7479665f;
    
    [Tooltip("에러 최종 스케일 Y")]
    public float errorScaleY = 0.6842404f;
    
    [Header("패턴 설정")]
    [Tooltip("랜덤 생성할 그리드 위치들 (중앙 포함 9개 위치 모두)")]
    private int[] availablePositions = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
    
    // 16박자 패턴 정의
    // 0 = 쉬기, 1 = 에러 생성, 2 = 펑 (모든 에러 터뜨리기)
    private int[] beatPattern = new int[]
    {
        0,  // 박자 1: 쉬기
        1,  // 박자 2: 에러 생성
        0,  // 박자 3: 쉬기
        1,  // 박자 4: 에러 생성
        0,  // 박자 5: 쉬기
        1,  // 박자 6: 에러 생성
        0,  // 박자 7: 쉬기
        1,  // 박자 8: 에러 생성
        0,  // 박자 9: 쉬기
        1,  // 박자 10: 에러 생성
        0,  // 박자 11: 쉬기
        1,  // 박자 12: 에러 생성
        0,  // 박자 13: 쉬기
        2,  // 박자 14: 펑! 모든 에러 터뜨리기
        0,  // 박자 15: 쉬기
        0   // 박자 16: 쉬기
    };
    
    private WindowSplitEffect windowSplitEffect;
    private bool hasStarted = false;
    private int currentBeatIndex = 0;
    private int patternBeatIndex = 0; // 16박자 패턴 내 인덱스
    private int errorCount = 0; // 현재 생성된 에러 개수
    private int lastProcessedBeat = -1; // 마지막으로 처리한 박자
    private List<GameObject> activeErrors = new List<GameObject>();
    private List<Coroutine> activeCoroutines = new List<Coroutine>();
    private HashSet<int> usedPositions = new HashSet<int>(); // 현재 사용 중인 위치들
    
    void Start()
    {
        // WindowSplitEffect 찾기
        windowSplitEffect = FindFirstObjectByType<WindowSplitEffect>();
        if (windowSplitEffect == null)
        {
            Debug.LogError("[ErrorPatternManager] WindowSplitEffect를 찾을 수 없습니다!");
        }
    }
    
    void Update()
    {
        if (Game3SequenceManager.Instance == null) return;
        
        double musicTime = Game3SequenceManager.Instance.GetMusicTime();
        
        // 현재 박자 계산 (180 BPM 기준)
        float beatInterval = 60f / 180f;
        int currentBeat = Mathf.FloorToInt((float)(musicTime / beatInterval));
        
        // 패턴 시작 체크
        if (!hasStarted && currentBeat >= patternStartBeat)
        {
            hasStarted = true;
            currentBeatIndex = 1; // 패턴 내 박자는 1부터 시작
            patternBeatIndex = 0;
            errorCount = 0;
            lastProcessedBeat = currentBeat - 1;
            
            Debug.Log($"[ErrorPatternManager] ========== 패턴 시작! 박자: {currentBeat}, 음악시간: {musicTime:F2}초 ==========");
        }
        
        // 패턴 진행 중
        if (hasStarted && currentBeat < patternEndBeat)
        {
            // 새로운 박자가 왔을 때만 처리
            if (currentBeat > lastProcessedBeat)
            {
                lastProcessedBeat = currentBeat;
                
                // 16박자 패턴 반복
                int patternAction = beatPattern[patternBeatIndex];
                
                Debug.Log($"[ErrorPatternManager] 전체박자 {currentBeat}, 패턴박자 {currentBeatIndex} (인덱스: {patternBeatIndex}), 액션: {patternAction}, 현재에러: {errorCount}개, 시간: {musicTime:F2}초");
                
                if (patternAction == 1)
                {
                    // 에러 생성 (1개만)
                    SpawnError();
                    errorCount++;
                }
                else if (patternAction == 2)
                {
                    // 펑! 모든 에러 터뜨리기
                    ExplodeAllErrors();
                    errorCount = 0;
                }
                // 0이면 쉬기 (아무것도 안함)
                
                currentBeatIndex++;
                patternBeatIndex++;
                
                // 16박자 패턴 반복
                if (patternBeatIndex >= beatPattern.Length)
                {
                    patternBeatIndex = 0;
                    currentBeatIndex = 1; // 다시 1부터 시작
                    Debug.Log("[ErrorPatternManager] ========== 16박자 패턴 반복! ==========");
                }
            }
        }
    }
    
    /// <summary>
    /// 에러 스폰 (랜덤 위치, 중복 방지)
    /// </summary>
    void SpawnError()
    {
        if (errorPrefab == null)
        {
            Debug.LogError("[ErrorPatternManager] errorPrefab이 없습니다!");
            return;
        }
        
        if (windowSplitEffect == null)
        {
            Debug.LogError("[ErrorPatternManager] windowSplitEffect가 없습니다!");
            return;
        }
        
        // 사용 가능한 위치 필터링 (이미 사용 중인 위치 제외)
        List<int> availablePos = new List<int>();
        foreach (int pos in availablePositions)
        {
            if (!usedPositions.Contains(pos))
            {
                availablePos.Add(pos);
            }
        }
        
        // 사용 가능한 위치가 없으면 경고 후 종료
        if (availablePos.Count == 0)
        {
            Debug.LogWarning("[ErrorPatternManager] 사용 가능한 위치가 없습니다! 모든 칸이 사용 중입니다.");
            return;
        }
        
        // 사용 가능한 위치 중 랜덤 선택
        int randomIndex = Random.Range(0, availablePos.Count);
        int gridIndex = availablePos[randomIndex];
        
        // 선택한 위치를 사용 중으로 표시
        usedPositions.Add(gridIndex);
        
        // gridIndex를 row, col로 변환 (0~8, 중앙 4 제외)
        // 0 1 2
        // 3 4 5
        // 6 7 8
        int row = gridIndex / 3;
        int col = gridIndex % 3;
        
        // 해당 위치의 창 가져오기
        GameObject targetWindow = windowSplitEffect.GetWindow(row, col);
        
        if (targetWindow == null)
        {
            Debug.LogWarning($"[ErrorPatternManager] 창을 찾을 수 없습니다! gridIndex: {gridIndex}, row: {row}, col: {col}");
            return;
        }
        
        Debug.Log($"[ErrorPatternManager] 대상 창 찾음: {targetWindow.name}, 위치: {targetWindow.transform.position}");
        
        // 에러를 창의 자식으로 생성
        GameObject error = Instantiate(errorPrefab, targetWindow.transform);
        error.name = $"Error_{currentBeatIndex}_{row}_{col}";
        
        // 자식으로 넣었으므로 localPosition/anchoredPosition을 (0,0)으로
        RectTransform errorRect = error.GetComponent<RectTransform>();
        
        if (errorRect != null)
        {
            // UI 오브젝트인 경우
            errorRect.anchoredPosition = Vector2.zero; // 부모 중앙에 배치
            // sizeDelta는 프리팹 값 유지 (변경하지 않음)
            errorRect.localScale = Vector3.zero; // 처음엔 크기 0으로 시작 (애니메이션용)
            Debug.Log($"[ErrorPatternManager] UI 에러 설정 - anchoredPosition: {errorRect.anchoredPosition}, sizeDelta: {errorRect.sizeDelta}, localScale: {errorRect.localScale}");
        }
        else
        {
            // World 오브젝트인 경우
            error.transform.localPosition = Vector3.zero; // 부모 위치 기준
            error.transform.localScale = Vector3.zero; // 처음엔 크기 0
            Debug.Log($"[ErrorPatternManager] World 에러 설정 - localPosition: {error.transform.localPosition}, localScale: {error.transform.localScale}");
        }
        
        activeErrors.Add(error);
        
        Debug.Log($"[ErrorPatternManager] ✅ 에러 생성 완료: {error.name} at Grid[{row},{col}], 총 {activeErrors.Count}개");
        
        // 애니메이션 시작 (코루틴 저장)
        Coroutine coroutine = StartCoroutine(ErrorAnimation(error, errorRect));
        activeCoroutines.Add(coroutine);
    }
    
    /// <summary>
    /// 모든 에러 즉시 폭발시키기
    /// </summary>
    void ExplodeAllErrors()
    {
        Debug.Log($"[ErrorPatternManager] 펑! 모든 에러 폭발 - 총 {activeErrors.Count}개");
        
        // 모든 활성 코루틴 중지
        foreach (Coroutine coroutine in activeCoroutines)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        activeCoroutines.Clear();
        
        // 모든 에러에 대해 폭발 애니메이션 시작
        List<GameObject> errorsToExplode = new List<GameObject>(activeErrors);
        activeErrors.Clear();
        
        // 사용 중인 위치 초기화
        usedPositions.Clear();
        
        foreach (GameObject error in errorsToExplode)
        {
            if (error != null)
            {
                StartCoroutine(ExplodeAnimation(error));
            }
        }
    }
    
    /// <summary>
    /// 폭발 애니메이션만 실행
    /// </summary>
    IEnumerator ExplodeAnimation(GameObject error)
    {
        RectTransform errorRect = error.GetComponent<RectTransform>();
        if (errorRect == null)
        {
            Destroy(error);
            yield break;
        }
        
        // CanvasGroup 추가/가져오기
        CanvasGroup canvasGroup = error.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = error.AddComponent<CanvasGroup>();
        }
        
        Vector3 originalScale = errorRect.localScale;
        
        // 폭발 애니메이션
        float elapsed = 0f;
        while (elapsed < errorExplodeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / errorExplodeDuration;
            
            // 크기 커지면서 페이드아웃
            errorRect.localScale = originalScale * (1f + t * 0.5f);
            canvasGroup.alpha = 1f - t;
            
            yield return null;
        }
        
        // 제거
        Destroy(error);
        Debug.Log($"[ErrorPatternManager] 에러 폭발 제거: {error.name}");
    }
    
    /// <summary>
    /// 에러 애니메이션: 등장 -> 유지 (펑 때까지)
    /// </summary>
    IEnumerator ErrorAnimation(GameObject error, RectTransform errorRect)
    {
        if (error == null)
        {
            Debug.LogWarning("[ErrorPatternManager] error가 null입니다!");
            yield break;
        }
        
        if (errorRect == null)
        {
            Debug.LogWarning("[ErrorPatternManager] errorRect가 null입니다! World 오브젝트로 처리합니다.");
            
            // World 오브젝트 애니메이션
            float elapsed = 0f;
            while (elapsed < errorAppearDuration)
            {
                if (error == null) yield break;
                
                elapsed += Time.deltaTime;
                float t = elapsed / errorAppearDuration;
                float scale = Mathf.Lerp(0f, 1f, t);
                error.transform.localScale = new Vector3(errorScaleX * scale, errorScaleY * scale, 1f);
                
                yield return null;
            }
            
            if (error != null)
            {
                error.transform.localScale = new Vector3(errorScaleX, errorScaleY, 1f);
                Debug.Log($"[ErrorPatternManager] World 에러 등장 완료: {error.name}");
            }
            
            yield break;
        }
        
        // UI 오브젝트 애니메이션
        Debug.Log($"[ErrorPatternManager] 에러 등장 애니메이션 시작: {error.name}");
        
        // 1. 등장 애니메이션 (크기 0 -> 1)
        float elapsed2 = 0f;
        while (elapsed2 < errorAppearDuration)
        {
            if (error == null || errorRect == null) yield break;
            
            elapsed2 += Time.deltaTime;
            float t = elapsed2 / errorAppearDuration;
            
            // Ease-out back (약간 튀는 효과)
            float scale = Mathf.Lerp(0f, 1f, t);
            errorRect.localScale = new Vector3(errorScaleX * scale, errorScaleY * scale, 1f);
            
            yield return null;
        }
        
        if (errorRect != null)
        {
            errorRect.localScale = new Vector3(errorScaleX, errorScaleY, 1f);
            Debug.Log($"[ErrorPatternManager] 에러 등장 완료: {error?.name}, scale: {errorRect.localScale}");
        }
        
        // 2. 유지 (펑 때까지 계속 유지)
        // ExplodeAllErrors에서 이 코루틴을 멈추고 폭발 애니메이션 실행
    }
    
    /// <summary>
    /// 모든 활성 에러 제거
    /// </summary>
    public void ClearAllErrors()
    {
        foreach (GameObject error in activeErrors)
        {
            if (error != null)
            {
                Destroy(error);
            }
        }
        
        activeErrors.Clear();
        Debug.Log("[ErrorPatternManager] 모든 에러 제거");
    }
    
    void OnDestroy()
    {
        ClearAllErrors();
    }
}
