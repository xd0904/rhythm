using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FileBossTrans : MonoBehaviour
{
    [Header("타이밍 설정")]
    public float stage1StartTime = 154.7f;  // 2:34.7 - 플레이어 창 이동 시작
    public float stage2StartTime = 155.9f;  // 2:35.9 - 화면 어두워짐 시작
    public float stage3StartTime = 158.0f;  // 2:38.0 - 팡! 효과
    public float cutsceneEndTime = 160.3f;  // 2:40.3 - 연출 종료

    [Header("대상 오브젝트")]
    public GameObject playerWindow;  // 플레이어가 있는 창 스프라이트
    public GameObject bossWindow;    // 보스가 나타날 창 스프라이트
    public GameObject boss;          // 보스 스프라이트

    [Header("탄막 설정")]
    public int bulletCount = 24;     // 탄막 개수
    public float bulletSpeed = 8f;   // 탄막 속도

    [Header("페이드 효과")]
    public float fadeMaxAlpha = 0.7f;  // 최대 어두움 정도

    [Header("알림")]
    public GameObject notificationObject;  // "바이러스가 제거되었습니다" 텍스트

    private Vector3 playerOriginalPos;
    private Color bossOriginalColor;
    private bool cutsceneStarted = false;
    private GameObject fadeOverlay;

    void Start()
    {
        // 타이밍 강제 설정
        stage1StartTime = 154.7f;  // 2:34.7
        stage2StartTime = 155.9f;  // 2:35.9
        stage3StartTime = 158.0f;  // 2:38.0
        cutsceneEndTime = 160.3f;  // 2:40.3

        Debug.Log($"[FileBossTrans] 타이밍: Stage1={stage1StartTime}초, Stage2={stage2StartTime}초, Stage3={stage3StartTime}초");

        // 플레이어 창 자동 찾기
        if (playerWindow == null)
        {
            playerWindow = GameObject.Find("PlayerWindow");

            // 초기 설정
            if (playerWindow != null)
            {
                Debug.Log("[FileBossTrans] PlayerWindow 자동 찾기 완료");
            }
        }

        // 보스 창 자동 찾기
        if (bossWindow == null)
        {
            bossWindow = GameObject.Find("BossWindow");
            if (bossWindow != null)
            {
                Debug.Log("[FileBossTrans] BossWindow 자동 찾기 완료");
            }
        }

        // 보스 자동 찾기
        if (boss == null)
        {
            boss = GameObject.Find("Boss");
            if (boss != null)
            {
                Debug.Log("[FileBossTrans] Boss 자동 찾기 완료");

                SpriteRenderer sr = boss.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    bossOriginalColor = sr.color;
                }
            }
        }

        // 초기 상태: 보스 OFF
        if (boss != null)
        {
            boss.SetActive(false);
            Debug.Log("[FileBossTrans] Boss 초기 상태: 비활성화");
        }
        {
            playerOriginalPos = playerWindow.transform.position;
        }

        if (bossWindow != null)
        {
            bossWindow.SetActive(false);
        }

        if (notificationObject != null)
        {
            notificationObject.SetActive(false);
        }

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
        sr.sortingOrder = 999;  // 가장 위에 렌더링

        // 화면 전체를 덮도록 크기 조정
        fadeOverlay.transform.localScale = new Vector3(100f, 100f, 1f);

        // 카메라 중심에 배치
        if (Camera.main != null)
        {
            fadeOverlay.transform.position = Camera.main.transform.position + new Vector3(0, 0, 10f);
        }

        // 초기 투명도 0
        Color c = sr.color;
        c.a = 0f;
        sr.color = c;
    }

    void Update()
    {
        if (BeatBounce.Instance == null) return;

        double musicTime = BeatBounce.Instance.GetMusicTime();

        if (musicTime <= 0) return;

        // 2:34.7에 컷신 시작
        if (!cutsceneStarted && musicTime >= stage1StartTime)
        {
            Debug.Log($"[FileBossTrans] 컷신 시작! musicTime: {musicTime}");
            cutsceneStarted = true;
            StartCoroutine(CutsceneSequence());
        }
    }

    IEnumerator CutsceneSequence()
    {
        Debug.Log("[FileBossTrans] === 1단계: 플레이어 창 이동 & 보스 창 생성 ===");

        // === 1단계 (2:34.7~2:35.9, 1.2초) ===
        float stage1Duration = stage2StartTime - stage1StartTime;

        // 보스 창 활성화
        if (bossWindow != null)
        {
            bossWindow.SetActive(true);
            Debug.Log("[FileBossTrans] BossWindow 활성화");
        }

        // 플레이어 창을 왼쪽으로 이동
        if (playerWindow != null)
        {
            Vector3 targetPos = playerOriginalPos + new Vector3(-3f, 0f, 0f);
            float elapsed = 0f;

            while (elapsed < stage1Duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / stage1Duration;
                playerWindow.transform.position = Vector3.Lerp(playerOriginalPos, targetPos, t);
                yield return null;
            }

            Debug.Log("[FileBossTrans] 플레이어 창 이동 완료");
        }
        else
        {
            yield return new WaitForSeconds(stage1Duration);
        }

        Debug.Log("[FileBossTrans] === 2단계: 화면 어두워짐 ===");

        // === 2단계 (2:35.9~2:38.0, 2.1초) ===
        float stage2Duration = stage3StartTime - stage2StartTime;

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

            Debug.Log("[FileBossTrans] 화면 어두워짐 완료");
        }
        else
        {
            yield return new WaitForSeconds(stage2Duration);
        }

        Debug.Log("[FileBossTrans] === 3단계: 팡! 효과 & 보스 변경 ===");

        // === 3단계 (2:38.0~2:40.3, 2.3초) ===
        float stage3Duration = cutsceneEndTime - stage3StartTime;

        // 흰색 탄막 사방으로 발사
        SpawnBulletExplosion();

        // 보스 활성화 & 파란색으로 변경
        if (boss != null)
        {
            boss.SetActive(true);
            Debug.Log("[FileBossTrans] Boss 활성화!");

            SpriteRenderer bossSr = boss.GetComponent<SpriteRenderer>();
            if (bossSr != null)
            {
                bossSr.color = Color.blue;
                Debug.Log("[FileBossTrans] 보스 색상 변경: 파란색");
            }
        }

        // 페이드 아웃 (화면 밝아짐) - 절반 시간 동안
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

            Debug.Log("[FileBossTrans] 화면 밝아짐 완료");
        }

        // 알림 표시 - 남은 시간 동안
        if (notificationObject != null)
        {
            notificationObject.SetActive(true);
            Debug.Log("[FileBossTrans] 알림 표시: 바이러스가 제거되었습니다");

            yield return new WaitForSeconds(stage3Duration * 0.5f);

            // 알림 자동 숨김 (선택사항)
            // notificationObject.SetActive(false);
        }

        Debug.Log("[FileBossTrans] 컷신 종료!");
    }

    void SpawnBulletExplosion()
    {
        if (boss == null)
        {
            Debug.LogWarning("[FileBossTrans] Boss가 없습니다!");
            return;
        }

        Vector3 center = boss.transform.position;
        float angleStep = 360f / bulletCount;

        Debug.Log($"[FileBossTrans] 탄막 {bulletCount}개 생성!");

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep;
            float rad = angle * Mathf.Deg2Rad;

            Vector3 direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);

            // 탄막 생성
            GameObject bullet = new GameObject($"Bullet_{i}");
            SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();

            // 원형 텍스처 생성
            Texture2D bulletTexture = CreateCircleTexture(16, Color.white);
            Sprite bulletSprite = Sprite.Create(bulletTexture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 50f);

            sr.sprite = bulletSprite;
            sr.sortingOrder = 400;

            bullet.transform.position = center;
            bullet.transform.localScale = Vector3.one * 0.5f;

            // 이동 코루틴 시작
            StartCoroutine(MoveBulletManually(bullet, direction));

            // 3초 후 자동 삭제
            Destroy(bullet, 3f);
        }
    }

    IEnumerator MoveBulletManually(GameObject bullet, Vector3 direction)
    {
        float lifetime = 0f;

        while (bullet != null && lifetime < 3f)
        {
            bullet.transform.position += direction * bulletSpeed * Time.deltaTime;
            lifetime += Time.deltaTime;
            yield return null;
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
}