using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShockWave : MonoBehaviour
{
    [Header("충격파 프리팹")]
    public GameObject shockwavePrefab;
    public GameObject shockwavePrefab2;

    [Header("충격파 생성 간격")]
    public float firstTwoDelay = 0.2f;
    public float lastDelay = 0.3f;
    public float spawnInterval = 2f;

    [Header("음악 시간 정보")]
    public BeatBounce beatBounce;

    [Header("활성화 시간 구간 (초 단위)")]
    public float startTime = 72.7f;
    public float endTime = 83.2f;
    public float fadeOutDuration = 1f; // 페이드아웃 시간

    private bool isActive = false;
    private bool bossSpawned = false;
    private bool isMoving = false;

    [Header("보스 관련")]
    public GameObject Bossobj;
    public float moveSpeed = 3f;
    public float minMoveDistance = 3f;

    private List<GameObject> activeShockwaves = new List<GameObject>(); // 활성 충격파 추적

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

        // 보스 SpriteRenderer 가져오기
        SpriteRenderer bossSr = Bossobj.GetComponent<SpriteRenderer>();
        Color bossOriginalColor = bossSr != null ? bossSr.color : Color.white;

        // 모든 활성 충격파의 SpriteRenderer 가져오기
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

        // 페이드아웃
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            float alpha = Mathf.Lerp(1f, 0f, t);

            // 보스 페이드
            if (bossSr != null)
            {
                Color c = bossOriginalColor;
                c.a = alpha;
                bossSr.color = c;
            }

            // 충격파들 페이드
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

        // 정리
        foreach (GameObject shockwave in activeShockwaves)
        {
            if (shockwave != null)
            {
                Destroy(shockwave);
            }
        }
        activeShockwaves.Clear();

        Bossobj.SetActive(false);

        // 보스 알파값 원래대로 복구 (다음 사용을 위해)
        if (bossSr != null)
        {
            Color c = bossOriginalColor;
            c.a = 1f;
            bossSr.color = c;
        }
    }

    IEnumerator ShockwaveRoutine()
    {
        // 첫 이동이 시작될 때까지 대기
        yield return new WaitUntil(() => isMoving);
        // 첫 이동이 완료될 때까지 대기
        yield return new WaitUntil(() => !isMoving);

        while (true)
        {
            // 보스 현재 위치에서 충격파 발사
            Vector2 bossPos = Bossobj.transform.position;

            // 처음 4개
            for (int i = 0; i < 4; i++)
            {
                StartCoroutine(SpawnAndExpandShockwave(bossPos, shockwavePrefab, 1f, 1.2f, 5f));
                yield return new WaitForSeconds(firstTwoDelay);
            }

            // 마지막 1개
            yield return new WaitForSeconds(lastDelay);
            StartCoroutine(SpawnAndExpandShockwave(bossPos, shockwavePrefab2, 1.5f, 1.6f, 5f));

            yield return new WaitForSeconds(spawnInterval);

            // 다음 이동이 시작될 때까지 대기
            yield return new WaitUntil(() => isMoving);
            // 다음 이동이 완료될 때까지 대기
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

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                obj.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            obj.position = targetPos;
            isMoving = false;

            yield return new WaitForSeconds(2.5f);
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

        // 최대 시도 후에도 못 찾으면 현재 위치에서 minMoveDistance만큼 떨어진 곳
        return currentPos + Random.insideUnitCircle.normalized * minMoveDistance;
    }

    IEnumerator SpawnAndExpandShockwave(Vector2 pos, GameObject prefab, float expandSpeed, float lifetime, float maxScale)
    {
        GameObject obj = Instantiate(prefab, pos, Quaternion.identity);
        activeShockwaves.Add(obj); // 리스트에 추가

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

        activeShockwaves.Remove(obj); // 리스트에서 제거
        Destroy(obj);
    }
}
