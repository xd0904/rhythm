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
    public SpriteRenderer mouseRenderer; // 마우스 스프라이트
    
    [Header("변신 스프라이트")]
    public Sprite bossSprite; // 보스 이미지
    private Sprite originalMouseSprite; // 원래 마우스 이미지
    
    [Header("오브 설정")]
    public int orbCount = 30; // 생성할 작은 오브 개수
    public float orbSpawnRadius = 8f; // 생성 반경
    public Color orbColor = new Color(1f, 0.8f, 0.2f, 0.9f); // 금색
    public float orbSize = 0.3f; // 오브 크기
    public float orbSpeed = 2f; // 오브가 마우스로 이동하는 속도
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
        if (mouseCursor != null)
        {
            originalMouseScale = mouseCursor.localScale;
        }
        
        if (mouseRenderer != null)
        {
            originalMouseSprite = mouseRenderer.sprite;
        }
        
        // 마우스 자동 찾기
        if (mouseCursor == null)
        {
            GameObject mouseObj = GameObject.Find("MouseCursor");
            if (mouseObj != null)
            {
                mouseCursor = mouseObj.transform;
                mouseRenderer = mouseObj.GetComponent<SpriteRenderer>();
                originalMouseScale = mouseCursor.localScale;
                if (mouseRenderer != null)
                {
                    originalMouseSprite = mouseRenderer.sprite;
                }
            }
        }
    }

    void Update()
    {
        // 44초에 변신 시작
        if (!transformationStarted && Time.time >= transformStartTime)
        {
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
        while (orbsCollected < orbCount && Time.time < transformCompleteTime)
        {
            yield return null;
        }
        
        // 55초 도달 시 변신 완료
        yield return StartCoroutine(CompleteTransformation());
        
        // 57초까지 유지
        float remainingTime = transformEndTime - Time.time;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
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
        
        // 마우스를 보스 이미지로 변경
        if (mouseRenderer != null && bossSprite != null)
        {
            mouseRenderer.sprite = bossSprite;
            isTransformed = true;
            Debug.Log("[BossTransformation] 마우스가 보스로 변신 완료!");
        }
        else
        {
            Debug.LogWarning("[BossTransformation] 보스 스프라이트가 할당되지 않았습니다!");
        }
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
        if (mouseRenderer != null && originalMouseSprite != null)
        {
            mouseRenderer.sprite = originalMouseSprite;
            
            if (mouseCursor != null)
            {
                mouseCursor.localScale = originalMouseScale;
            }
            
            isTransformed = false;
            Debug.Log("[BossTransformation] 원래 마우스로 복귀");
        }
    }
}

