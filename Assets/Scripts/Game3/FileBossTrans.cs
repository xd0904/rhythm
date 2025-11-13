using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FileBossTrans : MonoBehaviour
{
    [Header("타이밍 설정")]
    // NOTE: Start()에서 강제로 154.7f, 155.9f, 158.0f, 160.3f로 오버라이드 됩니다.
    public float stage1StartTime = 90.7f;    // 1:30.7 - 플레이어 창 이동 시작
    public float stage2StartTime = 91.9f;    // 1:31.9 - 화면 어두워짐 시작
    public float stage3StartTime = 94.0f;    // 1:34.0 - 팡! 효과
    public float cutsceneEndTime = 96.0f;    // 1:36.0 - 연출 종료

    [Header("대상 오브젝트")]
    public GameObject playerWindow;  // 플레이어가 있는 창 스프라이트
    public GameObject bossWindow;    // 보스가 나타날 창 스프라이트
    public GameObject boss;          // 보스 스프라이트
    public GameObject blueBoss;

    [Header("탄막 설정")]
    public int bulletCount = 24;     // 탄막 개수
    public float bulletSpeed = 8f;   // 탄막 속도

    [Header("페이드 효과")]
    public float fadeMaxAlpha = 0.7f; // 최대 어두움 정도

    [Header("알림")]
    public GameObject notificationObject; // "바이러스가 제거되었습니다" 텍스트

    private Vector3 playerOriginalPos;
    private Color bossOriginalColor;
    private bool cutsceneStarted = false;
    private GameObject fadeOverlay;
    private Game3SequenceManager sequenceManager;

    void Start()
    {

        Debug.Log($"[FileBossTrans] 타이밍: Stage1={stage1StartTime}초, Stage2={stage2StartTime}초, Stage3={stage3StartTime}초");

        // --- 필수 오브젝트 자동 찾기 및 검사 ---

        // 플레이어 창 자동 찾기 및 초기 위치 저장
        if (playerWindow != null)
        {
            playerOriginalPos = playerWindow.transform.position;
            Debug.Log("[FileBossTrans] PlayerWindow 자동 찾기 완료");
        }
        else
        {
            Debug.LogError("[FileBossTrans] PlayerWindow를 찾을 수 없습니다!");
        }


        // 보스 자동 찾기
        if (boss == null) boss = GameObject.Find("Boss");
        if (boss != null)
        {
            SpriteRenderer sr = boss.GetComponent<SpriteRenderer>();
            if (sr != null) bossOriginalColor = sr.color;
            boss.SetActive(false); // 초기 상태: 보스 OFF
            Debug.Log("[FileBossTrans] Boss 초기 상태: 비활성화");
        }

        if (notificationObject != null) notificationObject.SetActive(false);

        // 페이드 오버레이 생성
        CreateFadeOverlay();
    }

    void CreateFadeOverlay()
    {
        fadeOverlay = new GameObject("FadeOverlay");
        SpriteRenderer sr = fadeOverlay.AddComponent<SpriteRenderer>();

        // 검은색 사각형 텍스처 생성
        Texture2D blackTexture = new Texture2D(1, 1);
        blackTexture.SetPixel(0, 0, Color.black);
        blackTexture.Apply();

        Sprite blackSprite = Sprite.Create(blackTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        sr.sprite = blackSprite;
        sr.sortingOrder = 999;  // 가장 위에 렌더링 (UI/페이드)

        // 화면 전체를 덮도록 크기 조정 (월드 공간에서)
        // 카메라의 Orthographic Size를 기준으로 화면 크기를 추정
        if (Camera.main != null && Camera.main.orthographic)
        {
            float height = Camera.main.orthographicSize * 2f;
            float width = height * Camera.main.aspect;
            fadeOverlay.transform.localScale = new Vector3(width, height, 1f) * 1.5f; // 여유있게 더 크게
            fadeOverlay.transform.position = Camera.main.transform.position + new Vector3(0, 0, 10f);
        }
        else // Fallback for general camera
        {
            fadeOverlay.transform.localScale = new Vector3(100f, 100f, 1f);
            fadeOverlay.transform.position = new Vector3(0, 0, 10f);
        }


        // 초기 투명도 0
        Color c = sr.color;
        c.a = 0f;
        sr.color = c;
    }

    void Update()
    {
        // 1. 매니저 인스턴스 확인
        if (Game3SequenceManager.Instance == null) return;

        double musicTime = Game3SequenceManager.Instance.GetMusicTime();

        // 2. 음악 시작 시간 확인 (musicTime <= 0 이면 음악이 시작되지 않았음을 의미)
        if (musicTime <= 0) return;

        // 3. 컷신 시작 트리거
        if (!cutsceneStarted && musicTime >= stage1StartTime)
        {
            Debug.Log($"[FileBossTrans] 컷신 시작! musicTime: {musicTime}");
            cutsceneStarted = true;
            StartCoroutine(CutsceneSequence());
        }
    }

    IEnumerator CutsceneSequence()
    {
        // 컷신이 중복 실행되지 않도록 확인
        if (cutsceneStarted == false) yield break;

        // 컷신 시작 시 플레이어 조작 잠금 (필요하다면)
        // if (playerScript != null) playerScript.enabled = false;

        Debug.Log("[FileBossTrans] === 1단계: 플레이어 창 이동 & 보스 창 생성 (1.2초) ===");

        // === 1단계 (2:34.7~2:35.9) ===
        float stage1Duration = stage2StartTime - stage1StartTime;

        // 보스 창 활성화
        if (bossWindow != null)
        {
            bossWindow.SetActive(true);
        }

        // 플레이어 창을 왼쪽으로 이동
        if (playerWindow != null)
        {
            Vector3 targetPos = playerOriginalPos + new Vector3(-4f, 0f, 0f);
            float elapsed = 0f;

            while (elapsed < stage1Duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / stage1Duration;
                // 이동 가속/감속을 위해 SmoothStep 사용
                playerWindow.transform.position = Vector3.Lerp(playerOriginalPos, targetPos, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            playerWindow.transform.position = targetPos; // 최종 위치 보정
            Debug.Log("[FileBossTrans] 플레이어 창 이동 완료");
        }
        else
        {
            yield return new WaitForSeconds(stage1Duration);
        }

        Debug.Log("[FileBossTrans] === 2단계: 화면 어두워짐 (2.1초) ===");

        // === 2단계 (2:35.9~2:38.0) ===
        float stage2Duration = stage3StartTime - stage2StartTime;

        boss.SetActive(true);

        if (fadeOverlay != null)
        {
            SpriteRenderer fadeSr = fadeOverlay.GetComponent<SpriteRenderer>();
            float elapsed = 0f;

            while (elapsed < stage2Duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / stage2Duration;

                Color fadeColor = fadeSr.color;
                fadeColor.a = Mathf.Lerp(0f, fadeMaxAlpha, t);
                fadeSr.color = fadeColor;

                yield return null;
            }
            Color finalColor = fadeSr.color;
            finalColor.a = fadeMaxAlpha;
            fadeSr.color = finalColor; // 최종 알파값 보정
            Debug.Log("[FileBossTrans] 화면 어두워짐 완료");
        }
        else
        {
            yield return new WaitForSeconds(stage2Duration);
        }

        Debug.Log("[FileBossTrans] === 3단계: 팡! 효과 & 보스 변경 (2.3초) ===");

        // === 3단계 (2:38.0~2:40.3) ===
        float stage3Duration = cutsceneEndTime - stage3StartTime;

        // 흰색 탄막 사방으로 발사 (팡! 효과)
        SpawnBulletExplosion();

        if (boss != null && blueBoss != null)
        {
            // 기존 보스 비활성화
            boss.SetActive(false);

            // 파란 보스 활성화 및 위치/스케일 맞추기
            blueBoss.SetActive(true);
            blueBoss.transform.position = boss.transform.position; // 기존 위치
            blueBoss.transform.localScale = boss.transform.localScale;

            Debug.Log("[FileBossTrans] 보스 오브젝트 교체 완료: 빨간 Boss -> 파란 BlueBoss");
        }

        // 알림 표시 ("바이러스가 제거되었습니다")
        if (notificationObject != null)
        {
            notificationObject.SetActive(true);
            Debug.Log("[FileBossTrans] 알림 표시: 바이러스가 제거되었습니다");
        }

        // 페이드 아웃 (화면 밝아짐) - 총 시간의 절반(1.15초) 동안
        if (fadeOverlay != null)
        {
            SpriteRenderer fadeSr = fadeOverlay.GetComponent<SpriteRenderer>();
            float elapsed = 0f;
            float fadeOutDuration = stage3Duration * 0.5f;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeOutDuration;

                Color fadeColor = fadeSr.color;
                fadeColor.a = Mathf.Lerp(fadeMaxAlpha, 0f, t);
                fadeSr.color = fadeColor;

                yield return null;
            }
            Color clearColor = fadeSr.color;
            clearColor.a = 0f;
            fadeSr.color = clearColor; // 최종 투명도 보정
            Debug.Log("[FileBossTrans] 화면 밝아짐 완료");
        }

        // 남은 시간 동안 대기
        yield return new WaitForSeconds(stage3Duration * 0.5f);

        // 컷신 종료 후 처리
        Debug.Log("[FileBossTrans] 컷신 종료!");
    }

    void SpawnBulletExplosion()
    {
        if (boss == null)
        {
            Debug.LogWarning("[FileBossTrans] Boss 오브젝트가 없어 탄막을 발사할 수 없습니다.");
            return;
        }

        Vector3 center = boss.transform.position;
        float angleStep = 360f / bulletCount;

        Debug.Log($"[FileBossTrans] 탄막 {bulletCount}개 생성 시작.");

        // 탄막 텍스처를 미리 한 번만 생성
        Texture2D bulletTexture = CreateCircleTexture(32, Color.white); // 더 부드러운 원을 위해 크기를 32로 조정
        Sprite bulletSprite = Sprite.Create(bulletTexture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 50f);

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep;
            float rad = angle * Mathf.Deg2Rad;

            Vector3 direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);

            // 탄막 생성
            GameObject bullet = new GameObject($"Bullet_{i}");
            SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();

            sr.sprite = bulletSprite;
            sr.sortingOrder = 400; // 보스보다 높은 Z-Order

            bullet.transform.position = center;
            bullet.transform.localScale = Vector3.one * 0.3f; // 크기 조정

            // 이동 코루틴 시작
            StartCoroutine(MoveBulletManually(bullet, direction));

            // 3초 후 자동 삭제
            Destroy(bullet, 3f);
        }
    }

    IEnumerator MoveBulletManually(GameObject bullet, Vector3 direction)
    {
        // Rigidbody를 사용하지 않고 직접 Transform을 이동
        while (bullet != null)
        {
            bullet.transform.position += direction * bulletSpeed * Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// 지정된 크기와 색상으로 투명 배경을 가진 원형 텍스처를 생성합니다.
    /// </summary>
    Texture2D CreateCircleTexture(int size, Color color)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point; // 픽셀 아트 스타일을 원한다면

        int center = size / 2;
        float radiusSq = center * center;

        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // 중심으로부터의 거리를 제곱으로 계산하여 원형 영역 판별
                int dx = x - center;
                int dy = y - center;
                float distanceSq = dx * dx + dy * dy;

                if (distanceSq <= radiusSq)
                {
                    pixels[y * size + x] = color; // 원 안쪽은 지정된 색상
                }
                else
                {
                    pixels[y * size + x] = Color.clear; // 원 바깥쪽은 투명
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
}