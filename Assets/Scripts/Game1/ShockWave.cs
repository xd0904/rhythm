using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShockWave : MonoBehaviour
{
    [Header("충격파 프리팹")]
    public GameObject shockwavePrefab;
    public GameObject shockwavePrefab2;

    [Header("충격파 생성 간격")]
    public float firstTwoDelay = 0.5f;
    public float lastDelay = 0.2f;
    public float spawnInterval = 2f;

    [Header("음악 시간 정보")]
    public BeatBounce beatBounce;

    [Header("활성화 시간 구간 (초 단위)")]
    public float startTime = 72.7f;
    public float endTime = 83.2f;
    public float fadeOutDuration = 1f;

    private bool isActive = false;
    private bool bossSpawned = false;
    private bool isMoving = false;

    [Header("보스 관련")]
    public GameObject Bossobj;
    public float moveSpeed = 3f;
    public float minMoveDistance = 3f;
    public float moveWaitTime = 1.5f; // 이동 후 대기 시간 (일정하게)

    private List<GameObject> activeShockwaves = new List<GameObject>();

    [Tooltip("클릭 사운드")]
    public AudioClip Shock;

    void Update()
    {
        double currentTime = beatBounce.GetMusicTime();

        if (!isActive && currentTime >= startTime && currentTime <= endTime)
        {
            isActive = true;
            StartCoroutine(ShockwaveRoutine());
        }

        if (isActive && currentTime > endTime)
        {
            isActive = false;
            StopAllCoroutines();
            StartCoroutine(FadeOutEverything());
        }

        if (!bossSpawned && currentTime >= startTime)
        {
            bossSpawned = true;
            Bossobj.SetActive(true);
            StartCoroutine(MoveRandomly(Bossobj.transform));
        }
    }

    IEnumerator FadeOutEverything()
    {
        float elapsed = 0f;

        SpriteRenderer bossSr = Bossobj.GetComponent<SpriteRenderer>();
        Color bossOriginalColor = bossSr != null ? bossSr.color : Color.white;

        List<SpriteRenderer> shockwaveSrs = new List<SpriteRenderer>();
        List<Color> shockwaveOriginalColors = new List<Color>();

        foreach (GameObject shockwave in activeShockwaves)
        {
            if (shockwave != null)
            {
                SpriteRenderer sr = shockwave.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    shockwaveSrs.Add(sr);
                    shockwaveOriginalColors.Add(sr.color);
                }
            }
        }

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            float alpha = Mathf.Lerp(1f, 0f, t);

            if (bossSr != null)
            {
                Color c = bossOriginalColor;
                c.a = alpha;
                bossSr.color = c;
            }

            for (int i = 0; i < shockwaveSrs.Count; i++)
            {
                if (shockwaveSrs[i] != null)
                {
                    Color c = shockwaveOriginalColors[i];
                    c.a = alpha;
                    shockwaveSrs[i].color = c;
                }
            }

            yield return null;
        }

        foreach (GameObject shockwave in activeShockwaves)
        {
            if (shockwave != null)
            {
                Destroy(shockwave);
            }
        }
        activeShockwaves.Clear();

        Bossobj.SetActive(false);

        if (bossSr != null)
        {
            Color c = bossOriginalColor;
            c.a = 1f;
            bossSr.color = c;
        }
    }

    IEnumerator ShockwaveRoutine()
    {
        yield return new WaitUntil(() => isMoving);
        yield return new WaitUntil(() => !isMoving);

        while (isActive)
        {
            double currentTime = beatBounce.GetMusicTime();

            // endTime에 가까워지면 루틴 종료
            if (currentTime > endTime - 0.5f) // 종료 0.5초 전에 중단
            {
                yield return null;
                continue;
            }

            // 보스 현재 위치에서 충격파 발사 (이동 완료 직후)
            Vector2 bossPos = Bossobj.transform.position;

            yield return new WaitForSeconds(0.3f);

            // 처음 4개의 충격파
            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < 2; i++)
                {
                    StartCoroutine(SpawnAndExpandShockwave(bossPos, shockwavePrefab, 1f, 1.2f, 3f));
                    yield return new WaitForSeconds(firstTwoDelay);
                }
                yield return new WaitForSeconds(0.6f);
            }

            // 마지막 3개의 충격파
            StartCoroutine(SpawnAndExpandShockwave(bossPos, shockwavePrefab2, 1.5f, 1.6f, 3f));
            yield return new WaitForSeconds(1.1f);

            StartCoroutine(SpawnAndExpandShockwave(bossPos, shockwavePrefab2, 1.5f, 1.6f, 3f));
            yield return new WaitForSeconds(0.4f);

            StartCoroutine(SpawnAndExpandShockwave(bossPos, shockwavePrefab2, 1.5f, 1.6f, 3f));

            yield return new WaitForSeconds(0.4f);

            // 다음 이동 완료까지 대기
            yield return new WaitUntil(() => isMoving);
            yield return new WaitUntil(() => !isMoving);
        }
    }

    IEnumerator MoveRandomly(Transform obj)
    {
        while (true)
        {
            isMoving = true;
            Vector2 targetPos = GetRandomPosition();

            Vector3 startPos = obj.position;
            float distance = Vector2.Distance(startPos, targetPos);
            float duration = distance / moveSpeed;
            float elapsed = 0f;

            // 이동 실행
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                obj.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            obj.position = targetPos;
            isMoving = false;

            // 일정한 대기 시간
            yield return new WaitForSeconds(moveWaitTime);
        }
    }

    Vector2 GetRandomPosition()
    {
        Vector2 pos;
        Vector2 currentPos = Bossobj.transform.position;

        int maxAttempts = 100;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            float x = Random.Range(-8f, 8f);
            float y = Random.Range(-4f, 4f);

            bool inCenterX = (x > -4f && x < 4f);
            bool inCenterY = (y > -3.6f && y < 3.6f);
            pos = new Vector2(x, y);

            if (inCenterX && inCenterY)
            {
                attempts++;
                continue;
            }

            if (Vector2.Distance(currentPos, pos) < minMoveDistance)
            {
                attempts++;
                continue;
            }

            return pos;
        }

        return currentPos + Random.insideUnitCircle.normalized * minMoveDistance;
    }

    IEnumerator SpawnAndExpandShockwave(Vector2 pos, GameObject prefab, float expandSpeed, float lifetime, float maxScale)
    {
        SoundManager.Instance.PlaySFX(Shock);

        GameObject obj = Instantiate(prefab, pos, Quaternion.identity);
        activeShockwaves.Add(obj);

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        Vector3 startScale = obj.transform.localScale;
        float elapsed = 0f;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;
            float scale = Mathf.Lerp(1f, maxScale, t * expandSpeed);
            obj.transform.localScale = startScale * scale;

            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                sr.color = c;
            }

            yield return null;
        }

        activeShockwaves.Remove(obj);
        Destroy(obj);
    }
}