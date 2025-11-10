using UnityEngine;
using System.Collections;

public class ShockWave : MonoBehaviour
{
    [Header("충격파 프리팹")]
    public GameObject shockwavePrefab;
    public GameObject shockwavePrefab2;

    [Header("충격파 생성 간격")]
    public float firstTwoDelay = 0.2f;
    public float lastDelay = 0.5f;
    public float spawnInterval = 2f; // 각 세트 간격 (필요하면 조절 가능)

    [Header("음악 시간 정보")]
    public BeatBounce beatBounce; // 누나가 이미 씬에 있는 BeatBounce 넣으면 됨

    [Header("활성화 시간 구간 (초 단위)")]
    public float startTime = 70.4f;
    public float endTime = 83.2f;

    private bool isActive = false;

    // 충격파 기본 파라미터
    public float expandSpeed = 1f;   // 기본 확장 속도
    public float lifetime = 1.2f;    // 기본 존재 시간
    public float maxScale = 3f;      // 최대로 커지는 크기

    public float moveSpeed = 100f; // 이동 속도

    public GameObject Bossobj;
    public float minMoveDistance = 3f; // 💡 추가: 최소 이동 거리 (예: 3유닛)
    private bool bossSpawned = false;

    void Update()
    {
        double currentTime = beatBounce.GetMusicTime();

        // 구간 진입 시 코루틴 시작
        if (!isActive && currentTime >= startTime && currentTime <= endTime)
        {
            isActive = true;
            StartCoroutine(ShockwaveRoutine());
        }

        // 구간 벗어나면 중단
        if (isActive && currentTime > endTime)
        {
            isActive = false;
            StopAllCoroutines();
        }

        if (!bossSpawned && currentTime >= startTime)
        {
            bossSpawned = true;

            // 💡 1. 보스를 활성화합니다.
            Bossobj.SetActive(true);

            // 💡 2. 보스의 이동을 이 시점에 단 한 번 시작합니다.
            StartMoving(Bossobj);
        }

    }

    IEnumerator ShockwaveRoutine()
    {
        while (true)
        {
            Vector2 randomPos = GetRandomPosition();

            // 처음 2개 (기본 속도)
            for (int i = 0; i < 2; i++)
            {
                StartCoroutine(SpawnAndExpandShockwave(randomPos, 1f, 1.2f, 3f));
                yield return new WaitForSeconds(firstTwoDelay);
            }

            // 마지막 1개 (1.5배 빠르게 확장)
            yield return new WaitForSeconds(lastDelay);
            StartCoroutine(SpawnAndExpandShockwave2(randomPos, 1.5f, 1.6f, 3f));

            // 다음 세트 대기
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    public void StartMoving(GameObject obj)
    {
        StartCoroutine(MoveRandomly(obj.transform));
    }

    //보스 서서히 움직이는거 
    // 보스 서서히 움직이는거 
    IEnumerator MoveRandomly(Transform obj)
    {
        while (true)
        {
            Vector2 targetPos = GetRandomPosition();

            // 💡 1. Lerp 이동을 위한 이동 시간 변수(duration)와 경과 시간(elapsed) 추가
            float duration = Vector2.Distance(obj.position, targetPos) / moveSpeed; // moveSpeed를 사용하여 이동 총 시간 계산
            float elapsed = 0f;
            Vector3 startPos = obj.position;

            // 목표 위치까지 이동 (Lerp 사용)
            while (elapsed < duration) // 💡 2. 시간이 다 될 때까지 반복
            {
                // 경과 시간 업데이트
                elapsed += Time.deltaTime;

                // T 값: 0에서 1까지 부드럽게 증가하는 값
                float t = elapsed / duration;

                // Lerp를 사용하여 가속(시작)과 감속(끝) 효과를 줍니다.
                obj.position = Vector3.Lerp(startPos, targetPos, t); // 💡 3. Lerp로 교체

                yield return null;
            }

            // 💡 4. 목표 위치에 정확히 도달하도록 마지막 위치를 설정
            obj.position = targetPos;

            // 잠깐 멈춤
            yield return new WaitForSeconds(1f);
        }
    }



    Vector2 GetRandomPosition()
    {
        Vector2 pos;
        // 💡 현재 보스의 위치를 한 번만 가져옵니다.
        Vector2 currentPos = Bossobj.transform.position;

        while (true)
        {
            float x = Random.Range(-8f, 8f);
            float y = Random.Range(-4f, 4f);

            // 중앙 영역 (금지 구간)
            bool inCenterX = (x > -4f && x < 4f);
            bool inCenterY = (y > -3.6f && y < 3.6f);

            pos = new Vector2(x, y);

            if (inCenterX && inCenterY)
            {
                continue; // 1. 금지 구간이면 다시 뽑기
            }

            // 💡 2. 현재 보스 위치에서 'minMoveDistance'보다 가까우면 다시 뽑기
            if (Vector2.Distance(currentPos, pos) < minMoveDistance)
            {
                continue;
            }

            break;
        }

        return pos;
    }


    // ⚡ 핵심: 생성 + 확장 + 투명도 감소 + 삭제
    IEnumerator SpawnAndExpandShockwave(Vector2 pos, float expandSpeed, float lifetime, float maxScale)
    {

        yield return new WaitForSeconds(1f);

        GameObject obj = Instantiate(shockwavePrefab, pos, Quaternion.identity);
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        Vector3 startScale = obj.transform.localScale;
        float elapsed = 0f;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;

            // 점점 확장
            float t = elapsed / lifetime;
            float scale = Mathf.Lerp(1f, maxScale, t * expandSpeed);
            obj.transform.localScale = startScale * scale;

            // 투명도 감소
            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                sr.color = c;
            }

            yield return null;
        }

        Destroy(obj);
    }

    IEnumerator SpawnAndExpandShockwave2(Vector2 pos, float expandSpeed, float lifetime, float maxScale)
    {
        yield return new WaitForSeconds(1f);

        GameObject obj = Instantiate(shockwavePrefab2, pos, Quaternion.identity);
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        Vector3 startScale = obj.transform.localScale;
        float elapsed = 0f;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;

            // 점점 확장
            float t = elapsed / lifetime;
            float scale = Mathf.Lerp(1f, maxScale, t * expandSpeed);
            obj.transform.localScale = startScale * scale;

            // 투명도 감소
            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                sr.color = c;
            }

            yield return null;
        }

        Destroy(obj);
    }
}
