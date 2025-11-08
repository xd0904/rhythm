using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossTransformation : MonoBehaviour
{
    [Header("타이밍 설정")]
    public float transformStartTime = 44f; // 변신 시작
    public float transformCompleteTime = 55f; // 변신 완료
    public float transformEndTime = 57f; // 변신 씬 종료
    
    [Header("대상 오브젝트")]
    public Transform mouseCursor; // 마우스 커서 (보스로 변신!)
    public GameObject normalMouse; // Mouse (1) - 일반 마우스
    public GameObject bossHead; // BossHead - 보스 모드
    
    [Header("오브 설정")]
    public int orbCount = 80; // 생성할 작은 오브 개수 (44~55초, 11초 동안)
    public float orbSpawnRadius = 8f; // 생성 반경
    public Color orbColor = new Color(1f, 0.8f, 0.2f, 0.9f); // 금색
    public float orbSize = 0.3f; // 오브 크기
    public float orbSpeed = 1.2f; // 오브가 마우스로 이동하는 속도 (느리게)
    public float collectRadius = 0.5f; // 마우스가 오브를 먹는 반경
    
    [Header("마우스 성장")]
    public float mouseMaxScale = 2.0f; // 마우스 최대 크기
    
    private bool transformationStarted = false;
    private List<GameObject> orbs = new List<GameObject>();
    private int orbsCollected = 0;
    private Vector3 originalMouseScale;
    private bool isTransformed = false;

    void Start()
    {
        // 타이밍 강제 설정 (Inspector 값 무시)
        transformStartTime = 44f;
        transformCompleteTime = 55f;
        transformEndTime = 57f;
        
        // 오브 설정 강제 (Inspector 값 무시)
        orbCount = 80;
        orbSpeed = 1.2f;
        
        Debug.Log($"[BossTransformation] 타이밍 설정: 시작={transformStartTime}초, 완료={transformCompleteTime}초, 종료={transformEndTime}초");
        Debug.Log($"[BossTransformation] 오브 설정: 개수={orbCount}개, 속도={orbSpeed}");
        
        if (mouseCursor != null)
        {
            originalMouseScale = mouseCursor.localScale;
        }
        
        // 마우스 자동 찾기
        if (mouseCursor == null)
        {
            GameObject mouseObj = GameObject.Find("Mouse");
            if (mouseObj != null)
            {
                mouseCursor = mouseObj.transform;
                originalMouseScale = mouseCursor.localScale;
                
                // 자식 오브젝트 찾기
                normalMouse = mouseObj.transform.Find("Mouse (1)")?.gameObject;
                bossHead = mouseObj.transform.Find("BossHead")?.gameObject;
            }
        }
        
        // 초기 상태: 일반 마우스 ON, 보스 OFF
        if (normalMouse != null) normalMouse.SetActive(true);
        if (bossHead != null) bossHead.SetActive(false);
    }

    void Update()
    {
        if (BeatBounce.Instance == null) return; // BeatBounce 없으면 대기
        
        double musicTime = BeatBounce.Instance.GetMusicTime();
        
        // 음악이 아직 시작 안 했으면 대기 (음수 또는 0)
        if (musicTime <= 0)
        {
            // Debug.Log($"[BossTransformation] 음악 대기 중... musicTime: {musicTime}");
            return;
        }
        
        // 44초에 변신 시작
        if (!transformationStarted && musicTime >= transformStartTime)
        {
            Debug.Log($"[BossTransformation] 변신 트리거! musicTime: {musicTime}, 목표: {transformStartTime}");
            transformationStarted = true;
            StartCoroutine(TransformationSequence());
        }
        
        // 오브들을 마우스 쪽으로 이동 & 충돌 체크
        if (transformationStarted && !isTransformed)
        {
            MoveOrbsToMouse();
        }
    }

    IEnumerator TransformationSequence()
    {
        Debug.Log("[BossTransformation] 보스 변신 시작! 마우스로 오브 먹기 시작");
        
        // 오브 생성 (2초 동안 하나씩)
        float spawnDuration = 2f;
        for (int i = 0; i < orbCount; i++)
        {
            CreateOrb();
            yield return new WaitForSeconds(spawnDuration / orbCount);
        }
        
        // 모든 오브를 먹을 때까지 또는 55초까지 대기
        while (orbsCollected < orbCount && BeatBounce.Instance.GetMusicTime() < transformCompleteTime)
        {
            yield return null;
        }
        
        // 55초 도달 시 변신 완료
        yield return StartCoroutine(CompleteTransformation());
        
        // 57초까지 유지
        while (BeatBounce.Instance.GetMusicTime() < transformEndTime)
        {
            yield return null;
        }
        
        Debug.Log("[BossTransformation] 변신 씬 종료");
    }

    void CreateOrb()
    {
        if (mouseCursor == null) return;
        
        GameObject orb = new GameObject($"TransformOrb_{orbs.Count}");
        SpriteRenderer sr = orb.AddComponent<SpriteRenderer>();
        
        // 원형 텍스처 생성
        Texture2D orbTexture = CreateCircleTexture(32, orbColor);
        Sprite orbSprite = Sprite.Create(orbTexture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 50f);
        
        sr.sprite = orbSprite;
        sr.sortingOrder = 400;
        
        // 마우스 주변 랜덤 위치에 생성
        Vector2 randomPos = Random.insideUnitCircle * orbSpawnRadius;
        orb.transform.position = mouseCursor.position + new Vector3(randomPos.x, randomPos.y, 0f);
        orb.transform.localScale = Vector3.one * orbSize;
        
        orbs.Add(orb);
    }

    void MoveOrbsToMouse()
    {
        if (mouseCursor == null) return;
        
        for (int i = orbs.Count - 1; i >= 0; i--)
        {
            GameObject orb = orbs[i];
            if (orb == null) continue;
            
            // 마우스로 이동
            Vector3 direction = (mouseCursor.position - orb.transform.position).normalized;
            orb.transform.position += direction * orbSpeed * Time.deltaTime;
            
            // 회전 효과
            orb.transform.Rotate(0f, 0f, Time.deltaTime * 180f);
            
            // 마우스와의 거리 체크
            float distance = Vector3.Distance(orb.transform.position, mouseCursor.position);
            if (distance < collectRadius)
            {
                // 오브 먹음!
                CollectOrb(orb, i);
            }
        }
    }

    void CollectOrb(GameObject orb, int index)
    {
        orbsCollected++;
        orbs.RemoveAt(index);
        
        // 먹는 효과 (작아지면서 사라짐)
        StartCoroutine(OrbCollectEffect(orb));
        
        // 마우스 크기 증가
        if (mouseCursor != null)
        {
            float progress = (float)orbsCollected / orbCount;
            float newScale = Mathf.Lerp(originalMouseScale.x, originalMouseScale.x * mouseMaxScale, progress);
            mouseCursor.localScale = Vector3.one * newScale;
        }
        
        Debug.Log($"[BossTransformation] 오브 먹음! ({orbsCollected}/{orbCount})");
    }

    IEnumerator OrbCollectEffect(GameObject orb)
    {
        if (orb == null) yield break;
        
        SpriteRenderer sr = orb.GetComponent<SpriteRenderer>();
        Vector3 startScale = orb.transform.localScale;
        float duration = 0.2f;
        float elapsed = 0f;
        
        while (elapsed < duration && orb != null)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // 크기 감소
            orb.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
            
            // 페이드아웃
            Color color = sr.color;
            color.a = 1f - progress;
            sr.color = color;
            
            yield return null;
        }
        
        if (orb != null)
        {
            Destroy(orb);
        }
    }

    IEnumerator CompleteTransformation()
    {
        Debug.Log("[BossTransformation] 변신 완료!");
        
        // 남은 오브 모두 제거
        foreach (GameObject orb in orbs)
        {
            if (orb != null)
            {
                Destroy(orb);
            }
        }
        orbs.Clear();
        
        // 변신 폭발 효과
        yield return StartCoroutine(TransformationFlash());
        
        // 일반 마우스 OFF, 보스 ON
        if (normalMouse != null)
        {
            normalMouse.SetActive(false);
            Debug.Log("[BossTransformation] Mouse (1) 비활성화");
        }
        
        if (bossHead != null)
        {
            bossHead.SetActive(true);
            
            // BossHead 위치를 현재 마우스 위치로 설정 (오른쪽 끝)
            if (normalMouse != null)
            {
                // normalMouse와 같은 위치에 배치
                bossHead.transform.position = normalMouse.transform.position;
                bossHead.transform.rotation = normalMouse.transform.rotation;
            }
            else
            {
                // normalMouse가 없으면 부모 위치 사용
                bossHead.transform.localPosition = Vector3.zero;
            }
            
            isTransformed = true;
            Debug.Log($"[BossTransformation] BossHead 활성화 - 위치: {bossHead.transform.position}");
        }
        else
        {
            Debug.LogWarning("[BossTransformation] BossHead가 할당되지 않았습니다!");
        }
        
        // 보스 등장 임팩트 애니메이션 (2초)
        yield return StartCoroutine(BossAppearanceImpact());
    }
    
    IEnumerator BossAppearanceImpact()
    {
        if (mouseCursor == null) yield break;
        
        Debug.Log("[BossTransformation] 보스 등장 임팩트 시작!");
        
        Vector3 originalScale = mouseCursor.localScale;
        float elapsed = 0f;
        
        // 1. 커졌다가 작아지는 임팩트 (0.5초)
        float scaleUpDuration = 0.3f;
        float scaleDownDuration = 0.2f;
        
        // 커지는 애니메이션
        elapsed = 0f;
        while (elapsed < scaleUpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / scaleUpDuration;
            float scale = Mathf.Lerp(1f, 1.5f, t);
            mouseCursor.localScale = originalScale * scale;
            yield return null;
        }
        
        // 작아지는 애니메이션 (반동)
        elapsed = 0f;
        while (elapsed < scaleDownDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / scaleDownDuration;
            float scale = Mathf.Lerp(1.5f, 1.1f, t);
            mouseCursor.localScale = originalScale * scale;
            yield return null;
        }
        
        // 2. 충격파 링 3개 생성 (0.5초 간격)
        StartCoroutine(CreateShockwaveRing(mouseCursor.position, 0f));
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(CreateShockwaveRing(mouseCursor.position, 0.1f));
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(CreateShockwaveRing(mouseCursor.position, 0.2f));
        
        // 3. 화면 흔들림 (1초)
        if (Camera.main != null)
        {
            StartCoroutine(ScreenShake(1f, 0.3f));
        }
        
        // 4. 보스 주변에 파티클 지속 생성 (0.5초)
        float particleDuration = 0.5f;
        elapsed = 0f;
        int particleCount = 0;
        while (elapsed < particleDuration)
        {
            elapsed += Time.deltaTime;
            
            // 0.05초마다 파티클 생성
            if (Time.frameCount % 3 == 0 && particleCount < 30)
            {
                float randomAngle = Random.Range(0f, 360f);
                CreateFlashParticle(mouseCursor.position, randomAngle);
                particleCount++;
            }
            
            yield return null;
        }
        
        // 5. 원래 크기로 복귀 (부드럽게)
        elapsed = 0f;
        float returnDuration = 0.5f;
        Vector3 currentScale = mouseCursor.localScale;
        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / returnDuration;
            mouseCursor.localScale = Vector3.Lerp(currentScale, originalScale, t);
            yield return null;
        }
        
        mouseCursor.localScale = originalScale;
        Debug.Log("[BossTransformation] 보스 등장 임팩트 완료!");
    }
    
    IEnumerator CreateShockwaveRing(Vector3 center, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        GameObject ring = new GameObject("ShockwaveRing");
        SpriteRenderer sr = ring.AddComponent<SpriteRenderer>();
        
        // 링 텍스처 생성 (원형)
        Texture2D ringTexture = new Texture2D(64, 64);
        Color ringColor = new Color(0.965f, 0.118f, 0.129f, 0.8f); // #F61E21 (빨간색)
        
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float dx = x - 32f;
                float dy = y - 32f;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                
                // 링 모양 (도넛)
                if (distance > 28f && distance < 32f)
                {
                    ringTexture.SetPixel(x, y, ringColor);
                }
                else
                {
                    ringTexture.SetPixel(x, y, Color.clear);
                }
            }
        }
        ringTexture.Apply();
        
        Sprite ringSprite = Sprite.Create(ringTexture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 10f);
        sr.sprite = ringSprite;
        sr.sortingOrder = 500;
        
        ring.transform.position = center;
        ring.transform.localScale = Vector3.one * 0.5f;
        
        // 링 확장 애니메이션
        float duration = 1.5f;
        float elapsed = 0f;
        
        while (elapsed < duration && ring != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 크기 확대
            ring.transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 4f, t);
            
            // 페이드아웃
            Color color = sr.color;
            color.a = Mathf.Lerp(0.8f, 0f, t);
            sr.color = color;
            
            yield return null;
        }
        
        if (ring != null)
        {
            Destroy(ring);
        }
    }
    
    IEnumerator ScreenShake(float duration, float magnitude)
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;
        
        Vector3 originalPos = cam.transform.position;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - (elapsed / duration); // 점점 약해짐
            
            float x = Random.Range(-1f, 1f) * magnitude * t;
            float y = Random.Range(-1f, 1f) * magnitude * t;
            
            cam.transform.position = originalPos + new Vector3(x, y, 0f);
            
            yield return null;
        }
        
        // 원래 위치로 복귀
        cam.transform.position = originalPos;
    }

    IEnumerator TransformationFlash()
    {
        if (mouseCursor == null) yield break;
        
        // 방사형 파티클 폭발 효과
        int flashCount = 20;
        for (int i = 0; i < flashCount; i++)
        {
            float angle = i * (360f / flashCount);
            CreateFlashParticle(mouseCursor.position, angle);
        }
        
        yield return new WaitForSeconds(0.3f);
    }

    void CreateFlashParticle(Vector3 center, float angle)
    {
        GameObject particle = new GameObject("FlashParticle");
        SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();
        
        Texture2D particleTexture = new Texture2D(1, 1);
        particleTexture.SetPixel(0, 0, orbColor);
        particleTexture.Apply();
        
        Sprite particleSprite = Sprite.Create(particleTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 10f);
        sr.sprite = particleSprite;
        sr.sortingOrder = 501;
        
        particle.transform.position = center;
        particle.transform.localScale = Vector3.one * 1f;
        
        StartCoroutine(AnimateFlashParticle(particle, sr, angle));
    }

    IEnumerator AnimateFlashParticle(GameObject particle, SpriteRenderer sr, float angle)
    {
        Vector3 startPos = particle.transform.position;
        float distance = 3f;
        Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0f);
        Vector3 targetPos = startPos + direction * distance;
        
        float duration = 0.6f;
        float elapsed = 0f;
        
        while (elapsed < duration && particle != null)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            particle.transform.position = Vector3.Lerp(startPos, targetPos, progress);
            
            Color color = sr.color;
            color.a = 1f - progress;
            sr.color = color;
            
            yield return null;
        }
        
        if (particle != null)
        {
            Destroy(particle);
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
    
    public bool IsTransformed()
    {
        return isTransformed;
    }
    
    public void RevertTransformation()
    {
        // 보스 OFF, 일반 마우스 ON
        if (bossHead != null)
        {
            bossHead.SetActive(false);
        }
        
        if (normalMouse != null)
        {
            normalMouse.SetActive(true);
        }
        
        if (mouseCursor != null)
        {
            mouseCursor.localScale = originalMouseScale;
        }
        
        isTransformed = false;
        Debug.Log("[BossTransformation] 원래 마우스로 복귀");
    }
}

