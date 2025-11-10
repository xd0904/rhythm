using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Game2에서 Game3로 전환할 때 창들의 부드러운 애니메이션 처리
/// </summary>
public class Game2ToGame3Transition : MonoBehaviour
{
    [Header("창 오브젝트")]
    [Tooltip("보스 창 (오른쪽으로 이동하며 작아짐)")]
    public GameObject bossWindow;
    
    [Tooltip("텍스트 창 (페이드아웃)")]
    public GameObject textWindow;
    
    [Tooltip("게임 창 (페이드인하며 등장)")]
    public GameObject gameWindow;
    
    [Header("보스 창 애니메이션")]
    [Tooltip("보스 창 목표 위치")]
    public Vector3 bossTargetPosition = new Vector3(559f, -35f, 0f);
    
    [Tooltip("보스 창 최종 크기 (절대값)")]
    public Vector3 bossTargetScale = new Vector3(68.5f, 67.73889f, 1f);
    
    [Tooltip("보스 창 이동 시간")]
    public float bossMoveTime = 1.5f;
    
    [Header("텍스트 창 애니메이션")]
    [Tooltip("텍스트 창 페이드아웃 시간")]
    public float textFadeTime = 1.0f;
    
    [Header("게임 창 애니메이션")]
    [Tooltip("게임 창 시작 위치 (화면 왼쪽 밖)")]
    public Vector3 gameWindowStartPos = new Vector3(-800f, 0f, 0f);
    
    [Tooltip("게임 창 최종 위치")]
    public Vector3 gameWindowTargetPos = new Vector3(-322f, -26f, 0f);
    
    [Tooltip("게임 창 등장 시간")]
    public float gameWindowAppearTime = 1.5f;
    
    [Tooltip("게임 창 등장 딜레이 (보스 창이 먼저 움직인 후)")]
    public float gameWindowDelay = 0.5f;
    
    [Header("씬 전환")]
    [Tooltip("전환할 씬 이름")]
    public string nextSceneName = "Game3";
    
    [Tooltip("애니메이션 완료 후 대기 시간")]
    public float waitBeforeTransition = 0.5f;
    
    private CanvasGroup bossCanvasGroup;
    private CanvasGroup textCanvasGroup;
    private CanvasGroup gameCanvasGroup;
    
    private Vector3 bossStartPos;
    private Vector3 bossStartScale;
    
    void Start()
    {
        // CanvasGroup 컴포넌트 확인/추가
        if (bossWindow != null)
        {
            bossCanvasGroup = bossWindow.GetComponent<CanvasGroup>();
            if (bossCanvasGroup == null)
            {
                bossCanvasGroup = bossWindow.AddComponent<CanvasGroup>();
            }
            bossStartPos = bossWindow.transform.localPosition;
            bossStartScale = bossWindow.transform.localScale;
        }
        
        if (textWindow != null)
        {
            textCanvasGroup = textWindow.GetComponent<CanvasGroup>();
            if (textCanvasGroup == null)
            {
                textCanvasGroup = textWindow.AddComponent<CanvasGroup>();
            }
        }
        
        if (gameWindow != null)
        {
            gameCanvasGroup = gameWindow.GetComponent<CanvasGroup>();
            if (gameCanvasGroup == null)
            {
                gameCanvasGroup = gameWindow.AddComponent<CanvasGroup>();
            }
            
            // 게임 창 초기 상태: 투명 + 왼쪽 밖 위치
            gameCanvasGroup.alpha = 0f;
            gameWindow.transform.localPosition = gameWindowStartPos;
            gameWindow.SetActive(true);
        }
    }
    
    /// <summary>
    /// 전환 애니메이션 시작 (외부에서 호출)
    /// </summary>
    public void StartTransition()
    {
        StartCoroutine(TransitionAnimation());
    }
    
    /// <summary>
    /// 전환 애니메이션 코루틴
    /// </summary>
    private IEnumerator TransitionAnimation()
    {
        Debug.Log("[Game2ToGame3Transition] 전환 애니메이션 시작");
        
        // 1. 보스 창 이동 + 축소 (동시에)
        Coroutine bossMove = StartCoroutine(AnimateBossWindow());
        
        // 2. 텍스트 창 페이드아웃 (보스 창과 동시에)
        Coroutine textFade = StartCoroutine(AnimateTextWindow());
        
        // 3. 게임 창 페이드인 (약간 딜레이 후)
        yield return new WaitForSeconds(gameWindowDelay);
        Coroutine gameAppear = StartCoroutine(AnimateGameWindow());
        
        // 모든 애니메이션 완료 대기
        yield return bossMove;
        yield return textFade;
        yield return gameAppear;
        
        Debug.Log("[Game2ToGame3Transition] 모든 애니메이션 완료");
        
        // 잠시 대기 후 씬 전환
        yield return new WaitForSeconds(waitBeforeTransition);
        
        // 마우스 위치 저장하고 씬 전환
        SaveMouseAndLoadScene();
    }
    
    /// <summary>
    /// 보스 창 이동 + 축소 애니메이션
    /// </summary>
    private IEnumerator AnimateBossWindow()
    {
        if (bossWindow == null) yield break;
        
        float elapsed = 0f;
        
        while (elapsed < bossMoveTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / bossMoveTime;
            
            // Ease-out 곡선
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);
            
            // 위치 이동 (목표 위치로)
            bossWindow.transform.localPosition = Vector3.Lerp(bossStartPos, bossTargetPosition, smoothT);
            
            // 크기 변경 (절대값으로)
            bossWindow.transform.localScale = Vector3.Lerp(bossStartScale, bossTargetScale, smoothT);
            
            yield return null;
        }
        
        // 최종 위치/크기 정확히 설정
        bossWindow.transform.localPosition = bossTargetPosition;
        bossWindow.transform.localScale = bossTargetScale;
        
        Debug.Log("[Game2ToGame3Transition] 보스 창 애니메이션 완료");
    }
    
    /// <summary>
    /// 텍스트 창 페이드아웃 애니메이션
    /// </summary>
    private IEnumerator AnimateTextWindow()
    {
        if (textWindow == null || textCanvasGroup == null) yield break;
        
        float elapsed = 0f;
        float startAlpha = textCanvasGroup.alpha;
        
        while (elapsed < textFadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / textFadeTime;
            
            // 선형 페이드아웃
            textCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            
            yield return null;
        }
        
        // 완전히 투명
        textCanvasGroup.alpha = 0f;
        textWindow.SetActive(false);
        
        Debug.Log("[Game2ToGame3Transition] 텍스트 창 페이드아웃 완료");
    }
    
    /// <summary>
    /// 게임 창 페이드인 + 이동 애니메이션 (프로그램 켜지는 느낌)
    /// </summary>
    private IEnumerator AnimateGameWindow()
    {
        if (gameWindow == null || gameCanvasGroup == null) yield break;
        
        float elapsed = 0f;
        
        while (elapsed < gameWindowAppearTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / gameWindowAppearTime;
            
            // Ease-out 곡선 (부드러운 감속)
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);
            
            // 위치 이동 (왼쪽에서 중앙으로)
            gameWindow.transform.localPosition = Vector3.Lerp(gameWindowStartPos, gameWindowTargetPos, smoothT);
            
            // 페이드인 (약간 빠르게)
            float alphaT = Mathf.Pow(t, 0.5f);
            gameCanvasGroup.alpha = Mathf.Lerp(0f, 1f, alphaT);
            
            yield return null;
        }
        
        // 최종 위치/투명도 정확히 설정
        gameWindow.transform.localPosition = gameWindowTargetPos;
        gameCanvasGroup.alpha = 1f;
        
        Debug.Log("[Game2ToGame3Transition] 게임 창 등장 완료");
    }
    
    /// <summary>
    /// 마우스 위치 저장하고 다음 씬 로드
    /// </summary>
    private void SaveMouseAndLoadScene()
    {
        // 마우스 위치 저장 (Game2SequenceManager가 있다면 호출)
        Game2SequenceManager game2Manager = FindFirstObjectByType<Game2SequenceManager>();
        if (game2Manager != null)
        {
            game2Manager.SaveMouseAndLoadScene(nextSceneName);
        }
        else
        {
            // 직접 씬 로드
            SceneManager.LoadScene(nextSceneName);
        }
        
        Debug.Log($"[Game2ToGame3Transition] 씬 전환: {nextSceneName}");
    }
}
