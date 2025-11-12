using UnityEngine;
using System.Collections;
using UnityEditor.PackageManager;

public class BeatBounce : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static BeatBounce Instance { get; private set; }
    
    [Header("기존 바운스 설정")]
    public GameObject diamondPrefab;      // 다이아몬드 프리팹
    public Transform spawnPoint;          // 생성 위치
    public float maxScale = 1.3f;         // 최대 크기
    public float growDuration = 0.5f;     // 커지는 시간
    
    [Header("다이아몬드 색상 변화")]
    public Color startDiamondColor = Color.gray;  // 회색
    public Color endDiamondColor = Color.red;     // 빨간색
    public float colorChangeStart = 6.4f;         // 색상 변화 시작 (Beat 16)
    public float colorChangeEnd = 25.6f;          // 색상 변화 끝 (Beat 64)
    
    [Header("그림자 설정")]
    public GameObject topShadow;         // 위 그림자
    public GameObject bottomShadow;      // 아래 그림자
    public float shadowMaxY = 278f;      // 그림자 최대 Y 이동 거리
    public float shadowMaxScale = 100f;  // 그림자 최대 스케일
    public float shadowGrowDuration = 0.5f; // 그림자 커지는 시간
    
    [Header("6방향 발사 설정")]
    public GameObject ballPrefab;         // 발사될 볼 (중앙으로 이동)
    public GameObject trianglePrefab;     // 삼각형 투사체
    public GameObject circlePrefab;       // 동그라미 투사체
    public Transform mousePosition;       // 마우스 위치 (시작점)
    public Transform centerPoint;         // 게임 창 중앙 (터지는 지점)
    public float ballTravelTime = 0.4f;   // 볼 이동 시간 (1박자 = 0.4초)
    public float projectileSpeed = 5f;    // 투사체 발사 속도
    public float minY = -4f;              // 볼 Y축 최소 랜덤 범위
    public float maxY = 4f;               // 볼 Y축 최대 랜덤 범위
    public float mouseMoveDuration = 0.3f; // 마우스 이동 시간 (부드럽게)
    public float mouseMinX = -10f;        // 마우스 최소 X (화면 왼쪽 밖)
    public float mouseMaxX = 10f;         // 마우스 최대 X (화면 오른쪽 밖)
    
    [Header("공격 패턴 설정")]
    public float attackStartTime = 6.4f;  // 공격 시작 시간 (Beat 16에 맞춤)
    public float attackEndTime = 25.7f;   // 공격 끝 시간
    public int beatsPerAttack = 4;        // 공격당 박자 수 (4박자마다 공격)
    public GameObject mouseObject;        // 마우스 커서 오브젝트 (SpriteRenderer 또는 Image)
    public Color mouseNormalColor = Color.red;
    public Color mouseAttackColor = Color.white;
    
    [Header("음악 설정")]
    public float bpm = 150f;              // 비트 속도
    
    [Header("배경 어두워지기")]
    public GameObject background;         // 배경 오브젝트
    public Color darkColor = new Color(0.2f, 0.2f, 0.2f); // 어두운 색
    public float fadeDuration = 1f;       // 어두워지는 시간

    private double musicStartTime = 0; // 0으로 초기화 (음악 시작 전)
    private float beatInterval;
    private int lastBeatIndex = -1;
    private int attackBeatCounter = 0;    // 공격 박자 카운터
    
    // 코루틴 캐싱
    private Coroutine colorChangeCoroutine1;
    private Coroutine colorChangeCoroutine2;


    [Header("Wave Settings")]
    public GameObject wavePrefab;     // 물결 오브젝트 프리팹
    public GameObject wavePrefabAlt;     // 새로운 물결용 프리팹
    public GameObject bigWavePrefab;     // 큰 물결 프리팹 (위아래)
    public GameObject bounceWavePrefab;  // 위아래 튕기는 물결 프리팹
    public Transform waveParent;      // 빈 오브젝트로 정리용 부모
    public int waveCount = 100;        // 한 줄당 오브젝트 개수
    public float waveSpacing = 0.05f;    // 오브젝트 간 간격
    public float waveAmplitude = 1f;  // 위아래 흔들림 높이
    public float moveSpeed = 5f;      // 이동 속도
    public float waveSpeed = 3f;      // 흔들림 속도
    public float startX = 4f;        // 오른쪽 화면 바깥 시작 위치
    private bool waveSpawned = false; // 한 번만 실행 플래그
    
    [Header("이등변삼각형 패턴 (13.7초 ~ 25.7초)")]
    public GameObject isoscelesTrianglePrefab;  // 이등변삼각형 프리팹
    public GameObject gameWindow;               // 게임 창 오브젝트 (마스크 영역)
    public float trianglePatternStart = 13.7f;  // 패턴 시작 시간
    public float trianglePatternEnd = 25.7f;    // 패턴 끝 시간
    public float triangleMoveTime = 0.2f;       // 1/4박자 (마우스→창 안 랜덤 위치)
    public float triangleFlySpeed = 8f;         // 날아가는 속도
    public float trailThickness = 0.1f;         // 궤적 선 두께
    public Color trailColor = Color.red;        // 궤적 선 색상
    public float trailFadeDistance = 2f;        // 창에서 이 거리만큼 멀어지면 완전 투명
    public int triangleBeatsPerSpawn = 4;       // 4박자마다 삼각형 생성
    private int trianglePatternCounter = 0;     // 패턴 카운터
    
    [Header("아이콘 설정")]
    public GameObject icon1;  // 아이콘 1
    public GameObject icon2;  // 아이콘 2

    [Tooltip("튕기는 사운드")]
    public AudioClip Bounce;

    [Tooltip("박히는 사운드")]
    public AudioClip Hit;

    [Tooltip("창 날아가는 사운드")]
    public AudioClip Spear;


    void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[BeatBounce] 싱글톤 인스턴스 생성");
        }
        else
        {
            Debug.LogWarning("[BeatBounce] 중복된 인스턴스 발견! 제거됨");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        beatInterval = 60f / bpm;
        
        Debug.Log($"[BeatBounce] Start - BPM: {bpm}, Beat Interval: {beatInterval:F3}초");
        
        // 아이콘 비활성화
        if (icon1 != null)
        {
            icon1.SetActive(false);
            Debug.Log("[BeatBounce] Icon1 비활성화됨");
        }
        
        if (icon2 != null)
        {
            icon2.SetActive(false);
            Debug.Log("[BeatBounce] Icon2 비활성화됨");
        }
        
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
        
        if (centerPoint == null)
        {
            // 중앙점이 없으면 (0, 0) 사용
            GameObject center = new GameObject("CenterPoint");
            centerPoint = center.transform;
            centerPoint.position = Vector3.zero;
        }
        
        // 사전 로딩: 색상 변화 준비 (음악 시작 전에 컴포넌트 캐싱)
        if (topShadow != null)
        {
            topShadow.GetComponent<SpriteRenderer>();
            topShadow.GetComponent<UnityEngine.UI.Image>();
            
            // 그림자 초기 투명 설정
            SpriteRenderer topSR = topShadow.GetComponent<SpriteRenderer>();
            UnityEngine.UI.Image topImg = topShadow.GetComponent<UnityEngine.UI.Image>();
            
            Color transparentColor = startDiamondColor;
            transparentColor.a = 0f;
            
            if (topSR != null) topSR.color = transparentColor;
            else if (topImg != null) topImg.color = transparentColor;
            
            topShadow.transform.localScale = Vector3.one * shadowMaxScale; // 100으로 시작
            topShadow.transform.localPosition = new Vector3(topShadow.transform.localPosition.x, shadowMaxY, topShadow.transform.localPosition.z); // Y=278
        }
        
        if (bottomShadow != null)
        {
            bottomShadow.GetComponent<SpriteRenderer>();
            bottomShadow.GetComponent<UnityEngine.UI.Image>();
            
            // 그림자 초기 투명 설정
            SpriteRenderer bottomSR = bottomShadow.GetComponent<SpriteRenderer>();
            UnityEngine.UI.Image bottomImg = bottomShadow.GetComponent<UnityEngine.UI.Image>();
            
            Color transparentColor = startDiamondColor;
            transparentColor.a = 0f;
            
            if (bottomSR != null) bottomSR.color = transparentColor;
            else if (bottomImg != null) bottomImg.color = transparentColor;
            
            bottomShadow.transform.localScale = Vector3.one * shadowMaxScale; // 100으로 시작
            bottomShadow.transform.localPosition = new Vector3(bottomShadow.transform.localPosition.x, -shadowMaxY, bottomShadow.transform.localPosition.z); // Y=-278
        }
    }

    void Update()
    {
        // 음악 시작 전에는 아무것도 안 함
        if (musicStartTime == 0) return;
        
        // 현재 음악 시간 계산
        double currentMusicTime = GetMusicTime();
        
        // 현재 비트 인덱스 계산
        int currentBeatIndex = Mathf.FloorToInt((float)(currentMusicTime / beatInterval));
        
        // 새로운 비트가 왔을 때만 실행
        if (currentBeatIndex > lastBeatIndex)
        {
            lastBeatIndex = currentBeatIndex;
            
            // 다이아몬드 바운스 (13.7초 이전까지만 생성)
            if (currentMusicTime < trianglePatternStart)
            {
                SpawnAndGrowDiamond(currentMusicTime);
            }
            
            // 그림자 다이아몬드 패턴 (13.7초 ~ 25.7초만)
            if (currentMusicTime >= trianglePatternStart && currentMusicTime < trianglePatternEnd)
            {
                if (topShadow != null)
                {
                    StartCoroutine(GrowShadow(topShadow, true)); // 위쪽
                }
                if (bottomShadow != null)
                {
                    StartCoroutine(GrowShadow(bottomShadow, false)); // 아래쪽
                }
            }
            // 25.7초 이후면 그림자 페이드아웃 및 모든 삼각형 제거
            else if (currentMusicTime >= trianglePatternEnd)
            {
                if (topShadow != null || bottomShadow != null)
                {
                    StartCoroutine(FadeOutShadows());
                    
                    // 모든 이등변삼각형 즉시 제거
                    GameObject[] triangles = GameObject.FindGameObjectsWithTag("IsoscelesTriangle");
                    foreach (GameObject triangle in triangles)
                    {
                        Destroy(triangle);
                    }
                    
                    topShadow = null; // 한 번만 실행되도록
                    bottomShadow = null;
                }
            }
            
            // 이등변삼각형 패턴 (13.7초 ~ 25.7초, 4박자마다)
            if (currentMusicTime >= trianglePatternStart && currentMusicTime < trianglePatternEnd)
            {
                if (trianglePatternCounter % triangleBeatsPerSpawn == 0)
                {
                    // 4박자마다 이등변삼각형 생성
                    SpawnBouncingTriangle();
                }
                trianglePatternCounter++;
            }
            // 공격 패턴 (6.3초 ~ 13.7초, 기존 볼 발사 패턴)
            else if (currentMusicTime >= attackStartTime && currentMusicTime < trianglePatternStart)
            {
                // 4/4박자 기준: 1박자 = 발사, 2박자 = 터짐, 3박자 = 발사, 4박자 = 터짐
                int beatInCycle = attackBeatCounter % 2; // 발사-터짐 2박자 주기
                
                if (beatInCycle == 0)
                {
                    // 발사 박자 (쿵) - 항상 마우스에서 볼 발사
                    if (ballPrefab != null && mousePosition != null)
                    {
                        ShootBallFromMouse();
                        StartCoroutine(FlashMouseCursor());
                    }
                }
                else if (beatInCycle == 1)
                {
                    // 터지는 박자 (짝) - 중앙에서 6방향 퍼짐
                    // 볼이 중앙 도착하면 자동으로 터짐
                }
                
                attackBeatCounter++;
            }
            
            Debug.Log($"[BeatBounce] Beat {currentBeatIndex} at {currentMusicTime:F2}s");

            // 25.7초 이상이면 물결 생성 (한 번만)
            if (!waveSpawned && currentMusicTime >= 25.6f && currentMusicTime <= 51f)
            {
                waveSpawned = true;
                SpawnWave();
            }
        }
    }
    
    /// <summary>
    /// 음악 시작 시간 리셋 (외부에서 호출)
    /// </summary>
    public void ResetMusicStartTime()
    {
        musicStartTime = AudioSettings.dspTime;
        lastBeatIndex = -1;
        
        // 배경 어두워지기 (그림자는 나중에)
        if (background != null)
        {
            StartCoroutine(DarkenBackground());
        }
        
        // 그림자는 6.4초까지 서서히 페이드인
        if (topShadow != null || bottomShadow != null)
        {
            StartCoroutine(FadeInShadowsDelayed());
        }
        
        // 그림자 색상 변화 시작 (논블로킹, 지연 시작)
        if (topShadow != null)
        {
            colorChangeCoroutine1 = StartCoroutine(ChangeObjectColorDelayed(topShadow, 0.1f));
        }
        
        if (bottomShadow != null)
        {
            colorChangeCoroutine2 = StartCoroutine(ChangeObjectColorDelayed(bottomShadow, 0.15f));
        }
        
        Debug.Log($"[BeatBounce] 음악 시작 시간 리셋: {musicStartTime}");
    }
    
    /// <summary>
    /// 지연 후 색상 변화 시작 (부하 분산)
    /// </summary>
    private IEnumerator ChangeObjectColorDelayed(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        yield return StartCoroutine(ChangeObjectColor(obj));
    }
    
    /// <summary>
    /// 다이아몬드 바운스 생성 (색상 변화 + 점점 흐려짐)
    /// </summary>
    void SpawnAndGrowDiamond(double currentTime)
    {
        if (diamondPrefab == null)
        {
            Debug.LogWarning("[BeatBounce] Diamond Prefab이 설정되지 않았습니다!");
            return;
        }

        // 다이아몬드 생성
        GameObject diamond = Instantiate(diamondPrefab, spawnPoint.position, Quaternion.identity);
        
        // 부모 설정 (선택사항)
        diamond.transform.SetParent(spawnPoint);
        
        // 0부터 시작
        diamond.transform.localScale = Vector3.zero;
        
        // 현재 시간에 따른 색상 계산
        Color diamondColor = GetDiamondColorAtTime(currentTime);
        
        // 커지는 애니메이션 시작
        StartCoroutine(GrowAndDestroy(diamond, diamondColor));
    }
    
    /// <summary>
    /// 시간에 따른 다이아몬드 색상 계산
    /// </summary>
    Color GetDiamondColorAtTime(double time)
    {
        if (time < colorChangeStart)
        {
            return startDiamondColor;
        }
        else if (time > colorChangeEnd)
        {
            return endDiamondColor;
        }
        else
        {
            // 6.3초 ~ 25.7초 사이: 회색 → 빨간색
            float t = (float)((time - colorChangeStart) / (colorChangeEnd - colorChangeStart));
            return Color.Lerp(startDiamondColor, endDiamondColor, t);
        }
    }

    IEnumerator GrowAndDestroy(GameObject diamond, Color baseColor)
    {
        float elapsed = 0f;
        Vector3 targetScale = Vector3.one * maxScale;
        Vector3 centerPos = spawnPoint.position; // 중심 위치
        
        // SpriteRenderer 찾기
        SpriteRenderer spriteRenderer = diamond.GetComponent<SpriteRenderer>();
        UnityEngine.UI.Image image = diamond.GetComponent<UnityEngine.UI.Image>();
        
        // 색상 설정
        if (spriteRenderer != null) spriteRenderer.color = baseColor;
        else if (image != null) image.color = baseColor;
        
        // 0에서 maxScale까지 커지기
        while (elapsed < growDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / growDuration;
            
            // Ease-out 곡선으로 부드럽게 커지기
            float smoothT = 1f - Mathf.Pow(1f - t, 2f);
            
            if (diamond != null)
            {
                diamond.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, smoothT);
                
                // 중심에서의 거리 계산 (로컬 스케일 기준)
                float distanceFromCenter = diamond.transform.localScale.x / maxScale;
                
                // 거리에 따라 투명도 감소
                // 40% 크기 = 70% 투명 (0.3 불투명도)
                // 100% 크기 = 거의 안보임 (0.05 불투명도)
                float alpha = 1f - (distanceFromCenter * 1.75f); // 40%일때 0.3 (70% 투명)
                alpha = Mathf.Clamp01(alpha);
                
                // 색상에 알파값 적용
                Color currentColor = baseColor;
                currentColor.a = alpha;
                
                if (spriteRenderer != null) spriteRenderer.color = currentColor;
                else if (image != null) image.color = currentColor;
            }
            
            yield return null;
        }
        
        // 최대 크기 도달
        if (diamond != null)
        {
            diamond.transform.localScale = targetScale;
            
            // 최종 투명도 설정
            Color finalColor = baseColor;
            finalColor.a = 0.05f; // 최대 크기일 때 거의 안보임
            
            if (spriteRenderer != null) spriteRenderer.color = finalColor;
            else if (image != null) image.color = finalColor;
        }
        
        // 잠시 유지
        yield return new WaitForSeconds(0.1f);
        
        // 페이드 아웃하면서 사라지기
        yield return StartCoroutine(FadeOutAndDestroy(diamond));
    }

    IEnumerator FadeOutAndDestroy(GameObject diamond)
    {
        if (diamond == null) yield break;
        
        float fadeDuration = 0.3f;
        float elapsed = 0f;
        
        // SpriteRenderer나 Image 찾기
        SpriteRenderer spriteRenderer = diamond.GetComponent<SpriteRenderer>();
        UnityEngine.UI.Image image = diamond.GetComponent<UnityEngine.UI.Image>();
        
        Color originalColor = Color.white;
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
        else if (image != null) originalColor = image.color;
        
        // 페이드 아웃
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / fadeDuration);
            
            if (spriteRenderer != null)
            {
                Color newColor = originalColor;
                newColor.a = alpha;
                spriteRenderer.color = newColor;
            }
            else if (image != null)
            {
                Color newColor = originalColor;
                newColor.a = alpha;
                image.color = newColor;
            }
            
            yield return null;
        }
        
        // 삭제
        if (diamond != null)
        {
            Destroy(diamond);
        }
    }
    
    /// <summary>
    /// 배경 어두워지는 효과 (그림자 제외)
    /// </summary>
    private IEnumerator DarkenBackground()
    {
        if (background == null) yield break;
        
        SpriteRenderer bgSpriteRenderer = background.GetComponent<SpriteRenderer>();
        UnityEngine.UI.Image bgImage = background.GetComponent<UnityEngine.UI.Image>();
        
        // 시작 색상
        Color bgStartColor = Color.white;
        if (bgSpriteRenderer != null) bgStartColor = bgSpriteRenderer.color;
        else if (bgImage != null) bgStartColor = bgImage.color;
        
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            
            // 배경 어두워지기
            Color currentBgColor = Color.Lerp(bgStartColor, darkColor, t);
            if (bgSpriteRenderer != null) bgSpriteRenderer.color = currentBgColor;
            else if (bgImage != null) bgImage.color = currentBgColor;
            
            yield return null;
        }
        
        // 최종 색상 설정
        if (bgSpriteRenderer != null) bgSpriteRenderer.color = darkColor;
        else if (bgImage != null) bgImage.color = darkColor;
        
        Debug.Log("[BeatBounce] 배경 어두워짐 완료");
    }
    
    /// <summary>
    /// 그림자 페이드인 (0초 ~ 6.4초)
    /// </summary>
    private IEnumerator FadeInShadowsDelayed()
    {
        if (topShadow == null && bottomShadow == null) yield break;
        
        // 그림자 컴포넌트 가져오기
        SpriteRenderer topShadowSR = null;
        UnityEngine.UI.Image topShadowImg = null;
        SpriteRenderer bottomShadowSR = null;
        UnityEngine.UI.Image bottomShadowImg = null;
        
        if (topShadow != null)
        {
            topShadowSR = topShadow.GetComponent<SpriteRenderer>();
            topShadowImg = topShadow.GetComponent<UnityEngine.UI.Image>();
        }
        
        if (bottomShadow != null)
        {
            bottomShadowSR = bottomShadow.GetComponent<SpriteRenderer>();
            bottomShadowImg = bottomShadow.GetComponent<UnityEngine.UI.Image>();
        }
        
        // 그림자 시작 색상 (투명)
        Color topShadowStartColor = startDiamondColor;
        Color bottomShadowStartColor = startDiamondColor;
        topShadowStartColor.a = 0f;
        bottomShadowStartColor.a = 0f;
        
        // 그림자 목표 색상 (불투명)
        Color topShadowTargetColor = startDiamondColor;
        Color bottomShadowTargetColor = startDiamondColor;
        topShadowTargetColor.a = 1f;
        bottomShadowTargetColor.a = 1f;
        
        // 그림자 초기 설정 (투명, 스케일 0, 위치 초기화)
        if (topShadowSR != null) topShadowSR.color = topShadowStartColor;
        else if (topShadowImg != null) topShadowImg.color = topShadowStartColor;
        
        if (bottomShadowSR != null) bottomShadowSR.color = bottomShadowStartColor;
        else if (bottomShadowImg != null) bottomShadowImg.color = bottomShadowStartColor;
        
        // 그림자 초기 스케일 및 위치 설정 (100 크기, Y=±278에서 시작)
        if (topShadow != null)
        {
            topShadow.transform.localScale = Vector3.one * shadowMaxScale; // 100으로 시작
            topShadow.transform.localPosition = new Vector3(topShadow.transform.localPosition.x, shadowMaxY, topShadow.transform.localPosition.z); // Y=278
        }
        if (bottomShadow != null)
        {
            bottomShadow.transform.localScale = Vector3.one * shadowMaxScale; // 100으로 시작
            bottomShadow.transform.localPosition = new Vector3(bottomShadow.transform.localPosition.x, -shadowMaxY, bottomShadow.transform.localPosition.z); // Y=-278
        }
        
        // 6.4초까지 기다리면서 페이드인
        float targetTime = colorChangeStart; // 6.4초
        float startTime = 0f;
        float duration = targetTime - startTime;
        
        while (GetMusicTime() < targetTime)
        {
            double currentTime = GetMusicTime();
            float t = Mathf.Clamp01((float)(currentTime - startTime) / duration);
            
            // 그림자 페이드인
            Color currentTopColor = Color.Lerp(topShadowStartColor, topShadowTargetColor, t);
            Color currentBottomColor = Color.Lerp(bottomShadowStartColor, bottomShadowTargetColor, t);
            
            if (topShadowSR != null) topShadowSR.color = currentTopColor;
            else if (topShadowImg != null) topShadowImg.color = currentTopColor;
            
            if (bottomShadowSR != null) bottomShadowSR.color = currentBottomColor;
            else if (bottomShadowImg != null) bottomShadowImg.color = currentBottomColor;
            
            yield return null;
        }
        
        // 최종 색상 설정
        if (topShadowSR != null) topShadowSR.color = topShadowTargetColor;
        else if (topShadowImg != null) topShadowImg.color = topShadowTargetColor;
        
        if (bottomShadowSR != null) bottomShadowSR.color = bottomShadowTargetColor;
        else if (bottomShadowImg != null) bottomShadowImg.color = bottomShadowTargetColor;
        
        Debug.Log("[BeatBounce] 그림자 페이드인 완료 (0초~6.4초)");
    }
    
    /// <summary>
    /// 그림자 페이드아웃 (25.7초 이후)
    /// </summary>
    private IEnumerator FadeOutShadows()
    {
        if (topShadow == null && bottomShadow == null) yield break;
        
        SpriteRenderer topShadowSR = null;
        UnityEngine.UI.Image topShadowImg = null;
        SpriteRenderer bottomShadowSR = null;
        UnityEngine.UI.Image bottomShadowImg = null;
        
        Color topStartColor = Color.white;
        Color bottomStartColor = Color.white;
        
        if (topShadow != null)
        {
            topShadowSR = topShadow.GetComponent<SpriteRenderer>();
            topShadowImg = topShadow.GetComponent<UnityEngine.UI.Image>();
            
            if (topShadowSR != null) topStartColor = topShadowSR.color;
            else if (topShadowImg != null) topStartColor = topShadowImg.color;
        }
        
        if (bottomShadow != null)
        {
            bottomShadowSR = bottomShadow.GetComponent<SpriteRenderer>();
            bottomShadowImg = bottomShadow.GetComponent<UnityEngine.UI.Image>();
            
            if (bottomShadowSR != null) bottomStartColor = bottomShadowSR.color;
            else if (bottomShadowImg != null) bottomStartColor = bottomShadowImg.color;
        }
        
        float elapsed = 0f;
        float fadeDuration = 1f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            
            // 투명하게
            if (topShadow != null)
            {
                Color topColor = topStartColor;
                topColor.a = Mathf.Lerp(topStartColor.a, 0f, t);
                
                if (topShadowSR != null) topShadowSR.color = topColor;
                else if (topShadowImg != null) topShadowImg.color = topColor;
            }
            
            if (bottomShadow != null)
            {
                Color bottomColor = bottomStartColor;
                bottomColor.a = Mathf.Lerp(bottomStartColor.a, 0f, t);
                
                if (bottomShadowSR != null) bottomShadowSR.color = bottomColor;
                else if (bottomShadowImg != null) bottomShadowImg.color = bottomColor;
            }
            
            yield return null;
        }
        
        // 완전 투명
        if (topShadow != null)
        {
            Color topColor = topStartColor;
            topColor.a = 0f;
            if (topShadowSR != null) topShadowSR.color = topColor;
            else if (topShadowImg != null) topShadowImg.color = topColor;
            
            // 그림자 비활성화
            topShadow.SetActive(false);
        }
        
        if (bottomShadow != null)
        {
            Color bottomColor = bottomStartColor;
            bottomColor.a = 0f;
            if (bottomShadowSR != null) bottomShadowSR.color = bottomColor;
            else if (bottomShadowImg != null) bottomShadowImg.color = bottomColor;
            
            // 그림자 비활성화
            bottomShadow.SetActive(false);
        }
        
        Debug.Log("[BeatBounce] 그림자 페이드아웃 완료 (비활성화됨)");
    }
    
    /// <summary>
    /// 그림자 커지고 움직이는 애니메이션 (작아졌다가 커지기)
    /// </summary>
    private IEnumerator GrowShadow(GameObject shadow, bool isTop)
    {
        if (shadow == null) yield break;
        
        // 현재 색상 가져오기 (빨간색으로 변한 상태)
        SpriteRenderer shadowSR = shadow.GetComponent<SpriteRenderer>();
        UnityEngine.UI.Image shadowImg = shadow.GetComponent<UnityEngine.UI.Image>();
        
        Color currentColor = endDiamondColor; // 빨간색
        if (shadowSR != null) currentColor = shadowSR.color;
        else if (shadowImg != null) currentColor = shadowImg.color;
        
        Vector3 startPos = shadow.transform.localPosition;
        Vector3 targetPos = startPos;
        
        // 위쪽이면 Y를 0→278, 아래쪽이면 Y를 0→-278
        targetPos.y = isTop ? shadowMaxY : -shadowMaxY;
        
        Vector3 startScale = Vector3.one * shadowMaxScale; // 100부터 시작
        Vector3 targetScale = Vector3.zero; // 0으로 작아지기
        
        float elapsed = 0f;
        
        // 100에서 0으로 작아지고 움직이기
        while (elapsed < shadowGrowDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shadowGrowDuration;
            
            // Ease-out 곡선
            float smoothT = 1f - Mathf.Pow(1f - t, 2f);
            
            if (shadow != null)
            {
                shadow.transform.localScale = Vector3.Lerp(startScale, targetScale, smoothT);
                shadow.transform.localPosition = new Vector3(
                    startPos.x,
                    Mathf.Lerp(0, targetPos.y, smoothT),
                    startPos.z
                );
            }
            
            yield return null;
        }
        
        // 최소 크기/위치 도달
        if (shadow != null)
        {
            shadow.transform.localScale = targetScale;
            shadow.transform.localPosition = targetPos;
        }
        
        // 잠시 유지
        yield return new WaitForSeconds(0.1f);
        
        // 다시 100으로 커지면서 돌아가기
        elapsed = 0f;
        while (elapsed < shadowGrowDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shadowGrowDuration;
            
            float smoothT = 1f - Mathf.Pow(1f - t, 2f);
            
            if (shadow != null)
            {
                shadow.transform.localScale = Vector3.Lerp(targetScale, startScale, smoothT);
                shadow.transform.localPosition = new Vector3(
                    startPos.x,
                    Mathf.Lerp(targetPos.y, 0, smoothT),
                    startPos.z
                );
            }
            
            yield return null;
        }
        
        // 최종 위치/스케일 복원
        if (shadow != null)
        {
            shadow.transform.localScale = startScale;
            shadow.transform.localPosition = new Vector3(startPos.x, 0, startPos.z);
        }
    }
    
    /// <summary>
    /// 오브젝트 색상을 시간에 따라 변화 (다이아몬드와 동일한 방식)
    /// </summary>
    private IEnumerator ChangeObjectColor(GameObject obj)
    {
        if (obj == null) yield break;
        
        // 컴포넌트 캐싱 (한 번만)
        SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
        UnityEngine.UI.Image image = obj.GetComponent<UnityEngine.UI.Image>();
        
        if (spriteRenderer == null && image == null)
        {
            Debug.LogWarning($"[BeatBounce] {obj.name}에 SpriteRenderer 또는 Image가 없습니다!");
            yield break;
        }
        
        // 초기 색상 설정
        if (spriteRenderer != null) spriteRenderer.color = startDiamondColor;
        else if (image != null) image.color = startDiamondColor;
        
        // 색상 변화 시작까지 대기
        double startTime = colorChangeStart;
        while (GetMusicTime() < startTime)
        {
            yield return null;
        }
        
        // 색상 변화 진행 (프레임 스킵 최적화)
        double endTime = colorChangeEnd;
        double duration = endTime - startTime;
        int frameSkip = 0;
        
        while (GetMusicTime() < endTime)
        {
            frameSkip++;
            if (frameSkip % 2 != 0) // 2프레임마다 1번만 업데이트
            {
                yield return null;
                continue;
            }
            
            double currentTime = GetMusicTime();
            float t = (float)((currentTime - startTime) / duration);
            t = Mathf.Clamp01(t);
            
            Color currentColor = Color.Lerp(startDiamondColor, endDiamondColor, t);
            
            if (spriteRenderer != null) spriteRenderer.color = currentColor;
            else if (image != null) image.color = currentColor;
            
            yield return null;
        }
        
        // 최종 색상 설정
        if (spriteRenderer != null) spriteRenderer.color = endDiamondColor;
        else if (image != null) image.color = endDiamondColor;
    }
    
    /// <summary>
    /// 마우스 커서 깜빡임 효과
    /// </summary>
    IEnumerator FlashMouseCursor()
    {
        if (mouseObject == null) yield break;
        
        // SpriteRenderer 또는 Image 컴포넌트 찾기
        SpriteRenderer spriteRenderer = mouseObject.GetComponent<SpriteRenderer>();
        UnityEngine.UI.Image image = mouseObject.GetComponent<UnityEngine.UI.Image>();
        
        Color originalColor = Color.white;
        
        // 원래 색상 가져오기
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            spriteRenderer.color = mouseAttackColor;
        }
        else if (image != null)
        {
            originalColor = image.color;
            image.color = mouseAttackColor;
        }
        else
        {
            yield break; // 둘 다 없으면 종료
        }
        
        yield return new WaitForSeconds(0.1f);
        
        // 원래 색상으로 복구
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        else if (image != null)
        {
            image.color = originalColor;
        }
    }
    
    /// <summary>
    /// 마우스 위치에서 볼 발사 (항상 현재 마우스 위치에서)
    /// </summary>
    private void ShootBallFromMouse()
    {
        if (ballPrefab == null || mousePosition == null || centerPoint == null)
        {
            return;
        }
        
        // 화면 경계 및 게임 창 경계 가져오기
        Bounds screenBounds = GetScreenBounds();
        Bounds windowBounds = GetWindowBounds();
        
        // 마우스를 게임창 밖 랜덤 위치로 설정
        Vector3 randomMousePos = GetRandomPositionOutsideWindow(screenBounds, windowBounds);
        
        // 마우스 부드럽게 이동한 후 볼 발사
        StartCoroutine(MoveMouseAndShoot(randomMousePos));
    }
    
    IEnumerator MoveMouseAndShoot(Vector3 targetPos)
    {
        Vector3 startPos = mousePosition.position;
        float elapsed = 0f;
        
        // 1단계: 마우스 이동 (화면 밖으로)
        while (elapsed < mouseMoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / mouseMoveDuration;
            
            // 부드러운 이동 (Ease-out)
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);
            mousePosition.position = Vector3.Lerp(startPos, targetPos, smoothT);
            
            yield return null;
        }
        
        mousePosition.position = targetPos;
        
        // 2단계: 볼 발사 (화면 밖 위치에서)
        Vector3 spawnPos = mousePosition.position;
        GameObject ball = Instantiate(ballPrefab, spawnPos, Quaternion.identity);
        
        // 터지는 위치의 Y축을 랜덤으로 설정
        Vector3 randomTargetPos = centerPoint.position;
        randomTargetPos.y = Random.Range(minY, maxY);
        
        // 볼을 랜덤 Y축 위치로 이동시키는 코루틴
        StartCoroutine(MoveBallToCenterAndShoot(ball, randomTargetPos));
    }
    
    /// <summary>
    /// 마우스에서 볼 발사 (중앙으로 이동만) - 구버전
    /// </summary>
    private void ShootBall()
    {
        ShootBallFromMouse();
    }
    
    /// <summary>
    /// 마우스에서 볼 발사 + 즉시 6방향 투사체 발사 (사용 안 함)
    /// </summary>
    private void ShootBallAndProjectiles()
    {
        ShootBall();
    }
    
    /// <summary>
    /// 볼을 중앙으로 이동 후 6방향 투사체 발사
    /// </summary>
    private IEnumerator MoveBallToCenterAndShoot(GameObject ball, Vector3 targetPos)
    {
        Vector3 startPos = ball.transform.position;
        float duration = ballTravelTime; // 정확히 1박자(0.4초)
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            if (ball == null) yield break;
            
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            ball.transform.position = Vector3.Lerp(startPos, targetPos, t);
            
            yield return null;
        }
        
        // 중앙 도착
        if (ball != null)
        {
            ball.transform.position = targetPos;
            
            // 6방향 투사체 즉시 발사 (삼각형 3개 + 원 3개)
            ShootSixDirectionProjectiles(targetPos);
            
            // 볼 삭제
            Destroy(ball);
        }
    }
    
    /// <summary>
    /// 6방향으로 투사체 즉시 발사 (삼각형 3개 + 원 3개)
    /// </summary>
    private void ShootSixDirectionProjectiles(Vector3 position)
    {
        if (trianglePrefab == null || circlePrefab == null)
        {
            return;
        }
        
        // 6방향 (60도씩): 0도=삼각형, 60도=원, 120도=삼각형, 180도=원, 240도=삼각형, 300도=원
        for (int i = 0; i < 6; i++)
        {
            float angle = i * 60f;
            
            // 짝수 인덱스 = 삼각형, 홀수 인덱스 = 원
            bool isTriangle = (i % 2 == 0);
            GameObject prefabToSpawn = isTriangle ? trianglePrefab : circlePrefab;
            
            // 투사체 생성
            GameObject projectile = Instantiate(prefabToSpawn, position, Quaternion.identity);
            
            // 방향 회전 (삼각형만)
            if (isTriangle)
            {
                projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
            
            // 발사 방향 계산
            Vector3 direction = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad),
                0
            );
            
            // 투사체 즉시 발사
            StartCoroutine(MoveProjectile(projectile, direction));
        }
        
        Debug.Log("[BeatBounce] 6방향 투사체 발사! (삼각형 3개 + 원 3개)");
    }
    
    /// <summary>
    /// 투사체를 방향으로 이동
    /// </summary>
    private IEnumerator MoveProjectile(GameObject projectile, Vector3 direction)
    {
        float lifetime = 5f; // 5초 후 자동 삭제
        float elapsed = 0f;
        
        while (elapsed < lifetime && projectile != null)
        {
            projectile.transform.position += direction * projectileSpeed * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // 삭제
        if (projectile != null)
        {
            Destroy(projectile);
        }
    }
    
    public double GetMusicTime()
    {
        // ⭐ musicStartTime이 0보다 커야 (즉, ResetMusicStartTime()이 호출되어야) 유효한 시간을 반환합니다.
        if (musicStartTime > 0)
        {
            return AudioSettings.dspTime - musicStartTime;
        }
        // 음악 시작 전에는 0.0을 반환합니다.
        return 0.0;
    }
    
    public void SetBPM(float newBpm)
    {
        bpm = newBpm;
        beatInterval = 60f / bpm;
    }

    /// <summary>
    /// UI 물결 생성 (25.7초 시점)
    /// </summary>
    public void SpawnWave()
    {
        StartCoroutine(SpawnWaveCoroutine());
    }

    private IEnumerator SpawnWaveCoroutine()
    {
        if (wavePrefab == null || waveParent == null || wavePrefabAlt == null)
        {
            Debug.LogWarning("[BeatBounce] Wave Prefab 또는 Parent가 설정되지 않았습니다!");
            yield break;
        }

        for (int i = 0; i < waveCount; i++)
        { 
            // 기존 위쪽
            Vector3 topPos = new Vector3(startX + i * waveSpacing, 2f, 0f);
            GameObject topWave = Instantiate(wavePrefab, topPos, Quaternion.identity, waveParent);
            StartCoroutine(MoveWaveObject(topWave, 0, 0.8f, 2f)); // amplitude, speed

            // 기존 아래쪽
            Vector3 bottomPos = new Vector3(startX + i * waveSpacing, -2f, 0f);
            GameObject bottomWave = Instantiate(wavePrefab, bottomPos, Quaternion.identity, waveParent);
            StartCoroutine(MoveWaveObject(bottomWave, 1, 0.8f, 2f));

            // 새로운 위쪽
            Vector3 topPos2 = new Vector3(startX + i * waveSpacing, 3f, 0f);
            GameObject topWave2 = Instantiate(wavePrefabAlt, topPos2, Quaternion.identity, waveParent);
            StartCoroutine(MoveWaveObject(topWave2, 0, 1f, 1.5f));

            // 새로운 아래쪽
            Vector3 bottomPos2 = new Vector3(startX + i * waveSpacing, -3f, 0f);
            GameObject bottomWave2 = Instantiate(wavePrefabAlt, bottomPos2, Quaternion.identity, waveParent);
            StartCoroutine(MoveWaveObject(bottomWave2, 1, 1f, 1.5f));

            // 큰 물결 위쪽
            Vector3 bigTopPos = new Vector3(startX + i * waveSpacing, 4f, 0f);
            GameObject bigTopWave = Instantiate(bigWavePrefab, bigTopPos, Quaternion.identity, waveParent);
            StartCoroutine(MoveWaveObject(bigTopWave, 0, 2f, 0.8f));

            // 큰 물결 아래쪽
            Vector3 bigBottomPos = new Vector3(startX + i * waveSpacing, -4f, 0f);
            GameObject bigBottomWave = Instantiate(bigWavePrefab, bigBottomPos, Quaternion.identity, waveParent);
            StartCoroutine(MoveWaveObject(bigBottomWave, 1, 2f, 0.8f));

         

            yield return new WaitForSeconds(0.1f); // 순차 생성
        }
    }
    private IEnumerator MoveWaveObject(GameObject waveObj, int row, float waveAmplitude = 2f, float waveSpeed = 1f)
    {
        if (waveObj == null) yield break;

        float elapsed = 0f;
        Vector3 startPos = waveObj.transform.position;
        float moveSpeed = 2f;   // 이동 속도
        float xLimit = -10f;    // 왼쪽 화면 끝 위치, 필요에 따라 조절

        SpriteRenderer sr = waveObj.GetComponent<SpriteRenderer>();

        while (waveObj != null && waveObj.transform.position.x > xLimit)
        {
            elapsed += Time.deltaTime;

            float x = waveObj.transform.position.x - moveSpeed * Time.deltaTime;
            float y = startPos.y + Mathf.Sin(elapsed * waveSpeed + row * Mathf.PI / 2f) * waveAmplitude;

            waveObj.transform.position = new Vector3(x, y, 0f);

            yield return null;
        }

        if (waveObj != null)
            Destroy(waveObj);
    }

    
    /// <summary>
    /// 이등변삼각형 생성 및 튕기기 (13.7초 ~ 25.7초 패턴)
    /// </summary>
    private void SpawnBouncingTriangle()
    {
        if (isoscelesTrianglePrefab == null || mousePosition == null)
        {
            Debug.LogWarning("[BeatBounce] 이등변삼각형 프리팹 또는 마우스가 없습니다!");
            return;
        }

        // 화면 경계 및 게임 창 경계 가져오기
        Bounds screenBounds = GetScreenBounds();
        Bounds windowBounds = GetWindowBounds();
        
        // 마우스를 게임창 밖 랜덤 위치로 이동
        Vector3 randomMousePos = GetRandomPositionOutsideWindow(screenBounds, windowBounds);
        
        // 이등변삼각형 생성 위치도 게임창 밖 랜덤 위치
        Vector3 randomSpawnPos = GetRandomPositionOutsideWindow(screenBounds, windowBounds);

        // 랜덤 방향 벡터 (정규화)
        Vector2 randomDirection = new Vector2(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;

        // 마우스를 게임창 밖으로 먼저 이동
        StartCoroutine(MoveMouseToPosition(randomMousePos));
        
        // 이등변삼각형 생성 (게임창 밖에서)
        GameObject triangle = Instantiate(isoscelesTrianglePrefab, randomSpawnPos, Quaternion.identity);
        
        // SpriteRenderer 설정 (원본 유지, sortingOrder = 3으로 마스크 적용)
        SpriteRenderer sr = triangle.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 3; // 마스크가 적용되도록
        }
        
        // TrailRenderer 추가
        TrailRenderer trail = triangle.GetComponent<TrailRenderer>();
        if (trail == null)
        {
            trail = triangle.AddComponent<TrailRenderer>();
        }
        trail.time = 2f; // 궤적 유지 시간
        trail.startWidth = trailThickness;
        trail.endWidth = trailThickness * 0.5f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.startColor = trailColor;
        trail.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
        trail.sortingOrder = -1; // 창보다 아래 (창 밖에서만 보이도록)

        // 삼각형 애니메이션 시작
        StartCoroutine(BouncingTriangleSequence(triangle, randomSpawnPos, randomDirection, trail));
        
        Debug.Log($"[BeatBounce] 이등변삼각형 생성: {randomSpawnPos}, 마우스: {randomMousePos}, 방향: {randomDirection}");
    }
    
    /// <summary>
    /// 게임창 밖의 랜덤 위치 반환 (화면 안, 게임창 밖)
    /// </summary>
    private Vector3 GetRandomPositionOutsideWindow(Bounds screenBounds, Bounds windowBounds)
    {
        Vector3 randomPos;
        int maxAttempts = 20;
        int attempts = 0;
        
        do
        {
            // 화면 안의 랜덤 위치
            randomPos = new Vector3(
                Random.Range(screenBounds.min.x, screenBounds.max.x),
                Random.Range(screenBounds.min.y, screenBounds.max.y),
                0
            );
            attempts++;
        }
        while (IsInsideWindow(randomPos, windowBounds) && attempts < maxAttempts);
        
        // 최대 시도 후에도 창 안이면 강제로 창 밖으로 배치
        if (IsInsideWindow(randomPos, windowBounds))
        {
            // 창의 4방향 중 랜덤으로 선택해서 밖에 배치
            int side = Random.Range(0, 4);
            switch (side)
            {
                case 0: // 왼쪽
                    randomPos.x = windowBounds.min.x - Random.Range(1f, 3f);
                    randomPos.y = Random.Range(screenBounds.min.y, screenBounds.max.y);
                    break;
                case 1: // 오른쪽
                    randomPos.x = windowBounds.max.x + Random.Range(1f, 3f);
                    randomPos.y = Random.Range(screenBounds.min.y, screenBounds.max.y);
                    break;
                case 2: // 위쪽
                    randomPos.y = windowBounds.max.y + Random.Range(1f, 3f);
                    randomPos.x = Random.Range(screenBounds.min.x, screenBounds.max.x);
                    break;
                case 3: // 아래쪽
                    randomPos.y = windowBounds.min.y - Random.Range(1f, 3f);
                    randomPos.x = Random.Range(screenBounds.min.x, screenBounds.max.x);
                    break;
            }
        }
        
        return randomPos;
    }
    
    /// <summary>
    /// 위치가 게임창 안에 있는지 확인
    /// </summary>
    private bool IsInsideWindow(Vector3 position, Bounds windowBounds)
    {
        return position.x >= windowBounds.min.x && position.x <= windowBounds.max.x &&
               position.y >= windowBounds.min.y && position.y <= windowBounds.max.y;
    }
    
    /// <summary>
    /// 마우스를 특정 위치로 이동
    /// </summary>
    private IEnumerator MoveMouseToPosition(Vector3 targetPos)
    {
        if (mousePosition == null) yield break;
        
        Vector3 startPos = mousePosition.position;
        float elapsed = 0f;
        float duration = mouseMoveDuration;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 부드러운 이동 (Ease-out)
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);
            mousePosition.position = Vector3.Lerp(startPos, targetPos, smoothT);
            
            yield return null;
        }
        
        mousePosition.position = targetPos;
    }
    /// <summary>
    /// 이등변삼각형 애니메이션 시퀀스
    /// </summary>
    private IEnumerator BouncingTriangleSequence(GameObject triangle, Vector3 startPos, Vector2 direction, TrailRenderer trail)
    {
        if (triangle == null) yield break;

        // 삼각형은 이미 게임창 밖에서 생성됨
        triangle.transform.position = startPos;
        
        // 방향 회전 (삼각형이 이동 방향을 향하도록)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        triangle.transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // 1/4박자 대기 후 날아가기
        yield return new WaitForSeconds(beatInterval / 4f);
        
        // 2단계: 랜덤 방향으로 쭉 날아가기 (화면 경계에서 반사)
        Vector2 velocity = direction * triangleFlySpeed;
        Vector3 currentPos = startPos;

        float maxTime = trianglePatternEnd - trianglePatternStart; // 최대 생존 시간
        float elapsed = 0f;

        while (elapsed < maxTime && triangle != null)
        {
            elapsed += Time.deltaTime;
            
            // 위치 업데이트
            currentPos += (Vector3)(velocity * Time.deltaTime);
            
            // 화면 경계 체크 및 반사 (입사각 = 반사각)
            Bounds screenBounds = GetScreenBounds();
            
            // X축 반사
            if (currentPos.x <= screenBounds.min.x || currentPos.x >= screenBounds.max.x)
            {
                SoundManager.Instance.PlaySFX(Bounce);
                velocity.x = -velocity.x;
                currentPos.x = Mathf.Clamp(currentPos.x, screenBounds.min.x, screenBounds.max.x);
            }
            
            // Y축 반사
            if (currentPos.y <= screenBounds.min.y || currentPos.y >= screenBounds.max.y)
            {
                SoundManager.Instance.PlaySFX(Bounce);
                velocity.y = -velocity.y;
                currentPos.y = Mathf.Clamp(currentPos.y, screenBounds.min.y, screenBounds.max.y);
            }
            
            triangle.transform.position = currentPos;
            
            // 방향 회전
            float rotationAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg - 90f;
            triangle.transform.rotation = Quaternion.Euler(0, 0, rotationAngle);
            
            yield return null;
        }

        // 삭제
        if (triangle != null)
        {
            Destroy(triangle);
        }
    }

    /// <summary>
    /// 게임 창 경계 반환
    /// </summary>
    private Bounds GetWindowBounds()
    {
        if (gameWindow != null)
        {
            // gameWindow의 Collider2D 또는 RectTransform에서 경계 가져오기
            Collider2D col = gameWindow.GetComponent<Collider2D>();
            if (col != null)
            {
                return col.bounds;
            }
            
            // RectTransform인 경우 (UI)
            RectTransform rect = gameWindow.GetComponent<RectTransform>();
            if (rect != null)
            {
                Vector3[] corners = new Vector3[4];
                rect.GetWorldCorners(corners);
                Bounds bounds = new Bounds(corners[0], Vector3.zero);
                foreach (Vector3 corner in corners)
                {
                    bounds.Encapsulate(corner);
                }
                return bounds;
            }
        }

        // 기본값 (대략적인 창 크기)
        return new Bounds(Vector3.zero, new Vector3(6f, 4f, 0f));
    }

    /// <summary>
    /// 화면 경계 반환 (전체 화면)
    /// </summary>
    private Bounds GetScreenBounds()
    {
        // Camera 기준 화면 경계
        Camera cam = Camera.main;
        if (cam != null)
        {
            float height = cam.orthographicSize * 2f;
            float width = height * cam.aspect;
            return new Bounds(Vector3.zero, new Vector3(width, height, 0f));
        }

        // 기본값
        return new Bounds(Vector3.zero, new Vector3(20f, 12f, 0f));
    }


}


