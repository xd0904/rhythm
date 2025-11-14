using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;

public class BossBlueTransformation : MonoBehaviour
{
    [Header("타이밍 설정")]
    public float transformStartTime = 147.6f; // 2:27.6초 - 1단계 시작
    public float bulletStartTime = 150.4f; // 2:30.4초 - 2단계 탄막 시작
    public float fadeStartTime = 156.8f; // 2:36.8초 - 3단계 페이드 시작
    public float transformEndTime = 160f; // 2:40초 - 변신 완료
    
    [Header("씬 전환 설정")]
    public float sceneTransitionDelay = 1f; // 변신 완료 후 대기 시간
    public float sceneTransitionFadeDuration = 2f; // 페이드 아웃 시간
    
    [Header("대상 오브젝트")]
    public Transform boss; // 보스 오브젝트 (Boss)
    public Transform gameWindow; // 게임 창
    public Transform player; // 플레이어
    public GameObject redBoss; // 빨간 보스 (Boss)
    public GameObject blueBoss; // 파란 보스 (BlueBoss)
    public GameObject bossHead; // BossHead (Inspector에서 할당)
    public GameObject bossMouse; // BossMouse (Inspector에서 할당)
    
    [Header("이동 설정")]
    public Vector3 bossCenterPosition = new Vector3(0f, 0f, 0f); // 화면 중앙
    public float moveSpeed = 3f; // 이동 속도
    public float moveOutDistance = 15f; // 화면 밖으로 나가는 거리
    
    [Header("파란 탄막 설정")]
    public int bulletCount = 60; // 탄막 개수
    public float bulletSpawnRadius = 10f; // 생성 반경
    public Color bulletColor = new Color(0.3f, 0.5f, 1f, 0.9f); // 파란색
    public float bulletSize = 0.4f; // 탄막 크기
    public float bulletSpeed = 2.5f; // 탄막 속도
    public float bulletTrailLength = 1.5f; // 궤도 선 길이
    
    [Header("파란 보스 페이드 설정")]
    public Sprite blueBossSprite; // 파란색으로 칠한 보스 이미지 (Inspector에서 넣기)
    
    [Header("폭발 효과 설정")]
    public Color explosionColor = Color.white; // 하얀색 폭발
    public int explosionBulletCount = 30; // 폭발 시 퍼지는 탄막 개수
    public float explosionSpeed = 5f; // 폭발 탄막 속도
    
    private bool transformationStarted = false;
    private bool stage1Complete = false;
    private bool stage2Complete = false;
    private bool stage3Complete = false;
    private List<GameObject> bullets = new List<GameObject>();
    private Vector3 originalWindowPosition;
    private Vector3 originalPlayerPosition;
    private SpriteRenderer bossHeadRenderer; // BossHead의 SpriteRenderer
    private SpriteRenderer bossMouseRenderer; // BossMouse의 SpriteRenderer
    private GameObject blueBossGhost; // 페이드 인용 파란 보스 이미지
    private GameObject sceneTransitionFade; // 씬 전환용 페이드 오브젝트

    [Tooltip("에러 사운드")]
    public AudioClip Explosion;

    void Start()
    {
        // Boss 자동 찾기
        if (boss == null)
        {
            GameObject bossObj = GameObject.Find("Boss");
            if (bossObj != null)
            {
                boss = bossObj.transform;
                redBoss = bossObj;
            }
        }
        
        // BossHead, BossMouse SpriteRenderer 가져오기
        if (bossHead != null)
        {
            bossHeadRenderer = bossHead.GetComponent<SpriteRenderer>();
            if (bossHeadRenderer != null)
            {
                Debug.Log("[BossBlueTransformation] BossHead SpriteRenderer 찾기 완료");
            }
        }
        else
        {
            Debug.LogError("[BossBlueTransformation] bossHead가 할당되지 않았습니다! Inspector에서 할당하세요.");
        }
        
        if (bossMouse != null)
        {
            bossMouseRenderer = bossMouse.GetComponent<SpriteRenderer>();
            if (bossMouseRenderer != null)
            {
                Debug.Log("[BossBlueTransformation] BossMouse SpriteRenderer 찾기 완료");
            }
        }
        else
        {
            Debug.LogError("[BossBlueTransformation] bossMouse가 할당되지 않았습니다! Inspector에서 할당하세요.");
        }
        
        // BlueBoss 자동 찾기
        if (blueBoss == null)
        {
            blueBoss = GameObject.Find("BlueBoss");
        }
        
        // GameWindow 자동 찾기
        if (gameWindow == null)
        {
            GameObject windowObj = GameObject.Find("GameWindow");
            if (windowObj != null)
            {
                gameWindow = windowObj.transform;
                originalWindowPosition = gameWindow.position;
            }
        }
        
        // Player 자동 찾기
        if (player == null)
        {
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                originalPlayerPosition = player.position;
            }
        }
        
        // 초기 상태: 파란 보스만 비활성화
        // ⚠️ Boss(redBoss)는 절대 끄지 않음! (패턴 스크립트들이 붙어있어서 꺼지면 실행 안됨)
        if (blueBoss != null)
        {
            blueBoss.SetActive(false);
        }
        
        Debug.Log($"[BossBlueTransformation] 초기화 완료 - 시작: {transformStartTime}초");
    }

    void Update()
    {
        if (BeatBounce.Instance == null) return;
        
        double musicTime = BeatBounce.Instance.GetMusicTime();
        
        // 2:27.6초에 변신 시작
        if (!transformationStarted && musicTime >= transformStartTime)
        {
            transformationStarted = true;
            Debug.Log($"[BossBlueTransformation] 변신 연출 시작! musicTime: {musicTime}");
            StartCoroutine(TransformationSequence());
        }
    }

    IEnumerator TransformationSequence()
    {
        // === 1단계: 보스 중앙 이동, 윈도우/플레이어 퇴장 (2:27.6 ~ 2:30.4, 2.8초) ===
        Debug.Log("[BossBlueTransformation] 1단계: 보스 중앙 이동, 윈도우/플레이어 퇴장 시작");
        yield return StartCoroutine(Stage1_MoveToCenter());
        stage1Complete = true;
        
        // === 2단계: 빨간 보스 페이드 아웃 + 파란 탄막 집중 (2:30.4 ~ 2:36.8, 6.4초) ===
        Debug.Log("[BossBlueTransformation] 2단계: 빨간 보스 페이드 아웃 + 파란 탄막 집중");
        yield return StartCoroutine(Stage2_FadeOutAndBullets());
        stage2Complete = true;
        
        // === 3단계: 파란 보스 페이드 인 + 하얀 폭발 (2:36.8 ~ 2:40, 3.2초) ===
        Debug.Log("[BossBlueTransformation] 3단계: 파란 보스 페이드 인 + 하얀 폭발");
        yield return StartCoroutine(Stage3_FadeInBlueAndExplode());
        stage3Complete = true;
        
        Debug.Log("[BossBlueTransformation] 변신 연출 완료!");
        
        // === 4단계: 페이드 아웃 후 Game2 씬 전환 ===
        yield return new WaitForSeconds(sceneTransitionDelay);
        yield return StartCoroutine(FadeOutAndLoadScene());
    }

    // === 1단계: 보스 중앙 이동, 윈도우/플레이어 퇴장 ===
    IEnumerator Stage1_MoveToCenter()
    {
        float duration = 2.8f; // 2:27.6 ~ 2:30.4
        float elapsed = 0f;
        
        Vector3 bossStartPos = boss != null ? boss.position : Vector3.zero;
        
        // ⚠️ 창과 플레이어가 이미 아래로 내려가 있을 수 있으므로, 
        // 현재 위치에서 더 아래로 내려가게 설정
        Vector3 windowStartPos = gameWindow != null ? gameWindow.position : Vector3.zero;
        Vector3 playerStartPos = player != null ? player.position : Vector3.zero;
        
        // 목표 위치: 화면 아래로 충분히 멀리 (절대 안 보이게)
        Vector3 windowTargetPos = new Vector3(windowStartPos.x, -20f, windowStartPos.z); // Y = -20 (완전히 아래)
        Vector3 playerTargetPos = new Vector3(playerStartPos.x, -20f, playerStartPos.z); // Y = -20 (완전히 아래)
        
        Debug.Log($"[BossBlueTransformation] 창 시작위치: {windowStartPos}, 목표위치: {windowTargetPos}");
        Debug.Log($"[BossBlueTransformation] 플레이어 시작위치: {playerStartPos}, 목표위치: {playerTargetPos}");
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            // 보스 중앙으로 이동
            if (boss != null)
            {
                boss.position = Vector3.Lerp(bossStartPos, bossCenterPosition, smoothT);
            }
            
            // 윈도우 아래로 이동 (영구적으로)
            if (gameWindow != null)
            {
                gameWindow.position = Vector3.Lerp(windowStartPos, windowTargetPos, smoothT);
            }
            
            // 플레이어 아래로 이동 (영구적으로)
            if (player != null)
            {
                player.position = Vector3.Lerp(playerStartPos, playerTargetPos, smoothT);
            }
            
            yield return null;
        }
        
        // 최종 위치 강제 설정 (확실하게)
        if (gameWindow != null)
        {
            gameWindow.position = windowTargetPos;
            // 게임창 완전히 비활성화 (다시 올라오지 않도록)
            gameWindow.gameObject.SetActive(false);
            Debug.Log("[BossBlueTransformation] 게임창 비활성화됨 - 다시 올라오지 않음");
        }
        if (player != null)
        {
            player.position = playerTargetPos;
        }
        
        Debug.Log("[BossBlueTransformation] 1단계 완료 - 보스 중앙, 창/플레이어 퇴장 완료 (Y=-20으로 고정, 창 비활성화)");
    }

    // === 2단계: 빨간 보스 페이드 아웃 + 파란 탄막 집중 ===
    IEnumerator Stage2_FadeOutAndBullets()
    {
        float duration = 6.4f; // 2:30.4 ~ 2:36.8
        float fadeOutDuration = 3f; // 보스 페이드 아웃 시간
        float bulletSpawnDuration = 4f; // 탄막 생성 시간
        
        // 1. 빨간 보스 페이드 아웃 시작
        StartCoroutine(FadeOutRedBoss(fadeOutDuration));
        
        // 2. 파란 탄막 생성 (주변에서 중앙으로 모이게)
        float spawnInterval = bulletSpawnDuration / bulletCount;
        
        for (int i = 0; i < bulletCount; i++)
        {
            CreateBlueBullet();
            yield return new WaitForSeconds(spawnInterval);
        }
        
        // 3. 탄막들이 중앙에 모일 때까지 대기
        yield return new WaitForSeconds(duration - bulletSpawnDuration);
        
        Debug.Log("[BossBlueTransformation] 2단계 완료 - 빨간 보스 사라지고 탄막 집중");
    }
    
    IEnumerator FadeOutRedBoss(float duration)
    {
        Color originalHeadColor = bossHeadRenderer != null ? bossHeadRenderer.color : Color.white;
        Color originalMouseColor = bossMouseRenderer != null ? bossMouseRenderer.color : Color.white;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // BossHead 페이드 아웃
            if (bossHeadRenderer != null)
            {
                Color color = originalHeadColor;
                color.a = Mathf.Lerp(1f, 0f, t);
                bossHeadRenderer.color = color;
            }
            
            // BossMouse 페이드 아웃
            if (bossMouseRenderer != null)
            {
                Color color = originalMouseColor;
                color.a = Mathf.Lerp(1f, 0f, t);
                bossMouseRenderer.color = color;
            }
            
            yield return null;
        }
        
        // 완전히 투명하게
        if (bossHeadRenderer != null)
        {
            Color finalColor = originalHeadColor;
            finalColor.a = 0f;
            bossHeadRenderer.color = finalColor;
        }
        
        if (bossMouseRenderer != null)
        {
            Color finalColor = originalMouseColor;
            finalColor.a = 0f;
            bossMouseRenderer.color = finalColor;
        }
        
        Debug.Log("[BossBlueTransformation] 빨간 보스 페이드 아웃 완료 (Head + Mouse)");
    }

    void CreateBlueBullet()
    {
        if (boss == null) return;
        
        GameObject bullet = new GameObject($"BlueBullet_{bullets.Count}");
        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        
        // 파란 원형 텍스처 생성
        Texture2D bulletTexture = CreateCircleTexture(32, bulletColor);
        Sprite bulletSprite = Sprite.Create(bulletTexture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 50f);
        
        sr.sprite = bulletSprite;
        sr.sortingOrder = 450;
        
        // 보스 주변 랜덤 위치에서 생성 (원형 배치)
        float angle = Random.Range(0f, 360f);
        Vector2 spawnOffset = new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        ) * bulletSpawnRadius;
        
        bullet.transform.position = boss.position + new Vector3(spawnOffset.x, spawnOffset.y, 0f);
        bullet.transform.localScale = Vector3.one * bulletSize;
        
        bullets.Add(bullet);
        
        // 탄막 이동 시작 (궤도 선 포함)
        StartCoroutine(AnimateBlueBullet(bullet, sr));
    }

    IEnumerator AnimateBlueBullet(GameObject bullet, SpriteRenderer sr)
    {
        if (bullet == null || boss == null) yield break;
        
        Vector3 startPos = bullet.transform.position;
        Vector3 targetPos = boss.position; // 정확히 보스 중앙으로
        float duration = 3.5f;
        float elapsed = 0f;
        
        // 궤도 선 생성
        LineRenderer trail = bullet.AddComponent<LineRenderer>();
        trail.startWidth = 0.1f;
        trail.endWidth = 0.05f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.startColor = bulletColor;
        trail.endColor = new Color(bulletColor.r, bulletColor.g, bulletColor.b, 0f);
        trail.positionCount = 0;
        trail.sortingOrder = 449;
        
        List<Vector3> trailPositions = new List<Vector3>();
        
        while (elapsed < duration && bullet != null && boss != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 보스 중앙으로 직선 이동 (Lerp 사용)
            bullet.transform.position = Vector3.Lerp(startPos, targetPos, t);
            
            // 회전 효과
            bullet.transform.Rotate(0f, 0f, Time.deltaTime * 360f);
            
            // 궤도 선 업데이트
            trailPositions.Add(bullet.transform.position);
            if (trailPositions.Count > 15)
            {
                trailPositions.RemoveAt(0);
            }
            
            trail.positionCount = trailPositions.Count;
            trail.SetPositions(trailPositions.ToArray());
            
            yield return null;
        }
        
        // 정확히 중앙에 위치
        if (bullet != null && boss != null)
        {
            bullet.transform.position = boss.position;
        }
    }    // === 3단계: 파란 보스 페이드 인 + 하얀 폭발 ===
    IEnumerator Stage3_FadeInBlueAndExplode()
    {
        float duration = 3.2f; // 2:36.8 ~ 2:40
        float fadeInDuration = 2f; // 파란 보스 페이드 인 (탄막 계속 오는 동안)
        float explosionDelay = 1.8f; // 폭발 타이밍
        
        // 1. BossHead만 blueBossSprite으로 교체 (투명 상태)
        if (bossHeadRenderer != null && blueBossSprite != null)
        {
            bossHeadRenderer.sprite = blueBossSprite;
            Color color = bossHeadRenderer.color;
            color.a = 0f; // 투명
            bossHeadRenderer.color = color;
            
            Debug.Log("[BossBlueTransformation] BossHead 스프라이트를 파란색으로 교체 (투명 상태)");
        }
        
        // ⚠️ BossMouse는 스프라이트 교체 안 함! (원본 유지)
        // BossMouse는 페이드만 처리
        if (bossMouseRenderer != null)
        {
            Color color = bossMouseRenderer.color;
            color.a = 0f; // 투명 상태로 시작
            bossMouseRenderer.color = color;
            
            Debug.Log("[BossBlueTransformation] BossMouse는 원본 스프라이트 유지 (투명 상태)");
        }
        
        // 2. 파란 보스 페이드 인 시작 (탄막이 계속 모이는 동안)
        StartCoroutine(FadeInBlueBoss(fadeInDuration));

        // 3. 탄막들 중앙으로 계속 모임 (페이드 인과 동시에)
        SoundManager.Instance.PlaySFX(Explosion);

        yield return new WaitForSeconds(explosionDelay);
        
        // 4. 하얀 폭발 효과
        Debug.Log("[BossBlueTransformation] 하얀 폭발 효과!");

        yield return StartCoroutine(WhiteExplosion());
        
        // 5. 모든 탄막 제거
        foreach (GameObject bullet in bullets)
        {
            if (bullet != null)
            {
                Destroy(bullet);
            }
        }
        bullets.Clear();
        
        // 6. 빨간 보스 완전히 끄고, BlueBoss 활성화
        if (redBoss != null && blueBoss != null)
        {
            redBoss.SetActive(false);
            blueBoss.SetActive(true);
            
            // BlueBoss 위치를 Boss와 같은 위치로
            blueBoss.transform.position = boss.position;
            blueBoss.transform.localScale = boss.localScale;
            
            Debug.Log("[BossBlueTransformation] 보스 파란색으로 변신 완료! Boss OFF, BlueBoss ON");
        }
        
        // 7. 남은 시간 대기
        float remainingTime = duration - explosionDelay - 0.5f;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }
        
        Debug.Log("[BossBlueTransformation] 3단계 완료 - 변신 완료");
    }
    
    IEnumerator FadeInBlueBoss(float duration)
    {
        Color originalHeadColor = bossHeadRenderer != null ? bossHeadRenderer.color : Color.white;
        Color originalMouseColor = bossMouseRenderer != null ? bossMouseRenderer.color : Color.white;
        float elapsed = 0f;
        
        // ⚠️ BossMouse 비활성화
        if (bossMouse != null)
        {
            bossMouse.SetActive(false);
            Debug.Log("[BossBlueTransformation] BossMouse 비활성화됨");
        }
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // BossHead 페이드 인
            if (bossHeadRenderer != null)
            {
                Color color = originalHeadColor;
                color.a = Mathf.Lerp(0f, 1f, t);
                bossHeadRenderer.color = color;
            }
            
            yield return null;
        }
        
        // 완전히 불투명하게 (BossHead만)
        if (bossHeadRenderer != null)
        {
            Color finalColor = originalHeadColor;
            finalColor.a = 1f;
            bossHeadRenderer.color = finalColor;
        }
        
        Debug.Log("[BossBlueTransformation] 파란 보스 페이드 인 완료 (Head만, Mouse는 꺼짐)");
    }

    IEnumerator WhiteExplosion()
    {
        if (boss == null) yield break;
        
        // 중앙에서 방사형으로 하얀 탄막 퍼지기
        for (int i = 0; i < explosionBulletCount; i++)
        {
            float angle = i * (360f / explosionBulletCount);
            CreateExplosionBullet(boss.position, angle);
        }
        
        // 화면 플래시 효과
        StartCoroutine(ScreenFlash());
        
        yield return new WaitForSeconds(0.5f);
    }

    void CreateExplosionBullet(Vector3 center, float angle)
    {
        GameObject bullet = new GameObject("ExplosionBullet");
        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        
        // 하얀색 원형 텍스처
        Texture2D bulletTexture = CreateCircleTexture(32, explosionColor);
        Sprite bulletSprite = Sprite.Create(bulletTexture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 50f);
        
        sr.sprite = bulletSprite;
        sr.sortingOrder = 500;
        
        bullet.transform.position = center;
        bullet.transform.localScale = Vector3.one * bulletSize * 1.5f;
        
        // 방향 계산
        Vector3 direction = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad),
            0f
        );
        
        StartCoroutine(AnimateExplosionBullet(bullet, sr, direction));
    }

    IEnumerator AnimateExplosionBullet(GameObject bullet, SpriteRenderer sr, Vector3 direction)
    {
        if (bullet == null) yield break;
        
        float duration = 1.5f;
        float elapsed = 0f;
        Vector3 startPos = bullet.transform.position;
        
        while (elapsed < duration && bullet != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 이동
            bullet.transform.position += direction * explosionSpeed * Time.deltaTime;
            
            // 페이드아웃
            Color color = sr.color;
            color.a = Mathf.Lerp(1f, 0f, t);
            sr.color = color;
            
            // 크기 증가
            float scale = Mathf.Lerp(1f, 2f, t);
            bullet.transform.localScale = Vector3.one * bulletSize * 1.5f * scale;
            
            yield return null;
        }
        
        if (bullet != null)
        {
            Destroy(bullet);
        }
    }

    IEnumerator ScreenFlash()
    {
        // 화면 전체 하얗게 번쩍
        GameObject flash = new GameObject("ScreenFlash");
        SpriteRenderer sr = flash.AddComponent<SpriteRenderer>();
        
        // 큰 사각형 텍스처 생성
        Texture2D flashTexture = new Texture2D(1, 1);
        flashTexture.SetPixel(0, 0, Color.white);
        flashTexture.Apply();
        
        Sprite flashSprite = Sprite.Create(flashTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        sr.sprite = flashSprite;
        sr.sortingOrder = 999;
        
        flash.transform.position = Camera.main.transform.position + new Vector3(0f, 0f, 10f);
        flash.transform.localScale = new Vector3(50f, 50f, 1f);
        
        // 페이드아웃
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration && flash != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            Color color = sr.color;
            color.a = Mathf.Lerp(1f, 0f, t);
            sr.color = color;
            
            yield return null;
        }
        
        if (flash != null)
        {
            Destroy(flash);
        }
    }

    Texture2D CreateCircleTexture(int size, Color color)
    {
        Texture2D texture = new Texture2D(size, size);
        int center = size / 2;
        float radius = size / 2f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float alpha = distance < radius ? color.a : 0f;
                
                // 부드러운 가장자리
                if (distance > radius - 2f && distance < radius)
                {
                    alpha *= (radius - distance) / 2f;
                }
                
                Color pixelColor = color;
                pixelColor.a = alpha;
                texture.SetPixel(x, y, pixelColor);
            }
        }
        
        texture.Apply();
        return texture;
    }

    /// <summary>
    /// 페이드 아웃 후 Game2 씬 전환
    /// </summary>
    IEnumerator FadeOutAndLoadScene()
    {
        Debug.Log("[BossBlueTransformation] 페이드 아웃 시작 - Game2로 전환 준비");
        
        // 페이드 오브젝트 생성 (검은 화면)
        sceneTransitionFade = new GameObject("SceneTransitionFade");
        SpriteRenderer fadeRenderer = sceneTransitionFade.AddComponent<SpriteRenderer>();
        
        // 검은색 사각형 텍스처 생성
        Texture2D fadeTexture = new Texture2D(1, 1);
        fadeTexture.SetPixel(0, 0, Color.black);
        fadeTexture.Apply();
        
        Sprite fadeSprite = Sprite.Create(fadeTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        fadeRenderer.sprite = fadeSprite;
        fadeRenderer.sortingOrder = 1000; // 최상단
        
        // 카메라 중앙에 배치
        sceneTransitionFade.transform.position = Camera.main.transform.position + new Vector3(0f, 0f, 10f);
        sceneTransitionFade.transform.localScale = new Vector3(50f, 50f, 1f); // 화면 전체 덮기
        
        // 초기 색상 (완전 투명)
        Color fadeColor = fadeRenderer.color;
        fadeColor.a = 0f;
        fadeRenderer.color = fadeColor;
        
        // 페이드 아웃 (투명 → 불투명)
        float elapsed = 0f;
        
        while (elapsed < sceneTransitionFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / sceneTransitionFadeDuration;
            
            fadeColor = fadeRenderer.color;
            fadeColor.a = Mathf.Lerp(0f, 1f, t);
            fadeRenderer.color = fadeColor;
            
            yield return null;
        }
        
        // 완전히 불투명하게
        fadeColor = fadeRenderer.color;
        fadeColor.a = 1f;
        fadeRenderer.color = fadeColor;
        
        Debug.Log("[BossBlueTransformation] 페이드 아웃 완료 - Game2 씬 로드");
        
        // Game2 씬 로드
        SceneManager.LoadScene("Game2");
    }
}
