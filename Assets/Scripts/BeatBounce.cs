using UnityEngine;
using System.Collections;

public class BeatBounce : MonoBehaviour
{
    [Header("기존 바운스 설정")]
    public GameObject diamondPrefab;      // 다이아몬드 프리팹
    public Transform spawnPoint;          // 생성 위치
    public float maxScale = 1.3f;         // 최대 크기
    public float growDuration = 0.5f;     // 커지는 시간
    
    [Header("다이아몬드 색상 변화")]
    public Color startDiamondColor = Color.gray;  // 회색
    public Color endDiamondColor = Color.red;     // 빨간색
    public float colorChangeStart = 6.3f;         // 색상 변화 시작
    public float colorChangeEnd = 25.7f;          // 색상 변화 끝
    
    [Header("6방향 발사 설정")]
    public GameObject ballPrefab;         // 발사될 볼 (중앙으로 이동)
    public GameObject trianglePrefab;     // 삼각형 투사체
    public GameObject circlePrefab;       // 동그라미 투사체
    public Transform mousePosition;       // 마우스 위치 (시작점)
    public Transform centerPoint;         // 게임 창 중앙 (터지는 지점)
    public float ballSpeed = 10f;         // 볼 이동 속도
    public float projectileSpeed = 5f;    // 투사체 발사 속도
    
    [Header("공격 패턴 설정")]
    public float attackStartTime = 6.3f;  // 공격 시작 시간
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

    private double musicStartTime;
    private float beatInterval;
    private int lastBeatIndex = -1;
    private int attackBeatCounter = 0;    // 공격 박자 카운터

    void Start()
    {
        beatInterval = 60f / bpm;
        
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
            
            // 다이아몬드 바운스 (음악 시작하면 항상 생성)
            SpawnAndGrowDiamond(currentMusicTime);
            
            // 공격 패턴 (6.3초 ~ 25.7초 사이만, 마지막 발사 제외)
            if (currentMusicTime >= attackStartTime && currentMusicTime < attackEndTime - beatInterval)
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
        }
    }
    
    /// <summary>
    /// 음악 시작 시간 리셋 (외부에서 호출)
    /// </summary>
    public void ResetMusicStartTime()
    {
        musicStartTime = AudioSettings.dspTime;
        lastBeatIndex = -1;
        
        // 배경 어두워지기
        if (background != null)
        {
            StartCoroutine(DarkenBackground());
        }
        
        Debug.Log($"[BeatBounce] 음악 시작 시간 리셋: {musicStartTime}");
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
            }
            
            yield return null;
        }
        
        // 최대 크기 도달
        if (diamond != null)
        {
            diamond.transform.localScale = targetScale;
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
    /// 배경 어두워지는 효과
    /// </summary>
    private IEnumerator DarkenBackground()
    {
        if (background == null) yield break;
        
        SpriteRenderer spriteRenderer = background.GetComponent<SpriteRenderer>();
        UnityEngine.UI.Image image = background.GetComponent<UnityEngine.UI.Image>();
        
        Color startColor = Color.white;
        if (spriteRenderer != null) startColor = spriteRenderer.color;
        else if (image != null) startColor = image.color;
        
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            
            Color currentColor = Color.Lerp(startColor, darkColor, t);
            
            if (spriteRenderer != null) spriteRenderer.color = currentColor;
            else if (image != null) image.color = currentColor;
            
            yield return null;
        }
        
        if (spriteRenderer != null) spriteRenderer.color = darkColor;
        else if (image != null) image.color = darkColor;
        
        Debug.Log("[BeatBounce] 배경 어두워짐 완료");
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
        
        // 볼 생성 (항상 현재 마우스 위치에서)
        GameObject ball = Instantiate(ballPrefab, mousePosition.position, Quaternion.identity);
        
        // 볼을 중앙으로 이동시키는 코루틴
        StartCoroutine(MoveBallToCenterAndShoot(ball));
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
    private IEnumerator MoveBallToCenterAndShoot(GameObject ball)
    {
        Vector3 startPos = ball.transform.position;
        Vector3 targetPos = centerPoint.position;
        float distance = Vector3.Distance(startPos, targetPos);
        float duration = distance / ballSpeed;
        
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
        return AudioSettings.dspTime - musicStartTime;
    }
    
    public void SetBPM(float newBpm)
    {
        bpm = newBpm;
        beatInterval = 60f / bpm;
    }
}

