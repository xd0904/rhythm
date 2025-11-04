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
    public float colorChangeStart = 6.4f;         // 색상 변화 시작 (Beat 16)
    public float colorChangeEnd = 25.6f;          // 색상 변화 끝 (Beat 64)
    
    [Header("추가 색상 변화 오브젝트")]
    public GameObject colorChangeObject1;         // 색상 변화 오브젝트 1
    public GameObject colorChangeObject2;         // 색상 변화 오브젝트 2
    
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

    private double musicStartTime;
    private float beatInterval;
    private int lastBeatIndex = -1;
    private int attackBeatCounter = 0;    // 공격 박자 카운터
    
    // 코루틴 캐싱
    private Coroutine colorChangeCoroutine1;
    private Coroutine colorChangeCoroutine2;


    [Header("Wave Settings")]
    public GameObject wavePrefab;     // 물결 오브젝트 프리팹
    public GameObject wavePrefabAlt;     // 새로운 물결용 프리팹
    public Transform waveParent;      // 빈 오브젝트로 정리용 부모
    public int waveCount = 100;        // 한 줄당 오브젝트 개수
    public float waveSpacing = 0.05f;    // 오브젝트 간 간격
    public float waveAmplitude = 1f;  // 위아래 흔들림 높이
    public float moveSpeed = 5f;      // 이동 속도
    public float waveSpeed = 3f;      // 흔들림 속도
    public float startX = 4f;        // 오른쪽 화면 바깥 시작 위치
    private bool waveSpawned = false; // 한 번만 실행 플래그





    void Start()
    {
        beatInterval = 60f / bpm;
        
        Debug.Log($"[BeatBounce] Start - BPM: {bpm}, Beat Interval: {beatInterval:F3}초");
        
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
        if (colorChangeObject1 != null)
        {
            colorChangeObject1.GetComponent<SpriteRenderer>();
            colorChangeObject1.GetComponent<UnityEngine.UI.Image>();
        }
        
        if (colorChangeObject2 != null)
        {
            colorChangeObject2.GetComponent<SpriteRenderer>();
            colorChangeObject2.GetComponent<UnityEngine.UI.Image>();
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

            // 25.7초 이상이면 물결 생성 (한 번만)
            if (!waveSpawned && currentMusicTime >= 25.7f && currentMusicTime <= 44f)
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
        
        // 배경 어두워지기 (논블로킹)
        if (background != null)
        {
            StartCoroutine(DarkenBackground());
        }
        
        // 추가 오브젝트 색상 변화 시작 (논블로킹, 지연 시작)
        if (colorChangeObject1 != null)
        {
            colorChangeCoroutine1 = StartCoroutine(ChangeObjectColorDelayed(colorChangeObject1, 0.1f));
        }
        
        if (colorChangeObject2 != null)
        {
            colorChangeCoroutine2 = StartCoroutine(ChangeObjectColorDelayed(colorChangeObject2, 0.15f));
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
        
        // 마우스를 화면 밖 랜덤 위치로 설정
        Vector3 randomMousePos = new Vector3(
            Random.Range(mouseMinX, mouseMaxX),  // 화면 양옆 밖에서 랜덤
            Random.Range(minY, maxY),            // Y축 랜덤
            mousePosition.position.z
        );
        
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
        return AudioSettings.dspTime - musicStartTime;
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



}

