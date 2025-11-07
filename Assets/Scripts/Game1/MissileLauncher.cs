// MissileLauncher.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic; // List를 사용하기 위해 추가

public class MissileLauncher : MonoBehaviour
{
    public BeatBounce beatBounce;

    // 제공된 힌트 변수들
    [Header("미사일 설정")]
    public GameObject missilePrefab;        // 미사일 프리팹 (GuidedMissile 스크립트 포함)
    public GameObject outerObjectPrefab;    // 오브젝트 프리팹
    public GameObject smallCirclePrefab;    // 팡! 터지는 프리팹
    public int missileCount = 4;            // 한 번에 생성할 개수
    public float spawnDelay = 0.2f;         // 각 미사일 생성 간격
    public float fireDelay = 0.5f;          // 마지막 미사일 생성 후 발사까지의 지연
    public float missileSpeed = 8f;         // 발사 속도 (GuidedMissile로 전달)

    [Header("발사 패턴 설정")]
    public float spreadAngle = 90f;         // 미사일이 펼쳐질 부채꼴 각도 (예: 90도)
    public float spawnRadius = 0.5f;        // 스포너를 중심으로 생성될 반경 (곡선 생성 느낌을 위해)

    private Transform playerTransform;      // 플레이어의 위치를 저장할 변수

    void Start()
    {
        // "Player" 태그를 가진 오브젝트를 찾습니다. (씬에 플레이어 오브젝트가 "Player" 태그를 가졌다고 가정)
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player 오브젝트를 찾을 수 없습니다. 'Player' 태그를 확인하세요.");
        }

        // 테스트를 위해 게임 시작 후 바로 발사 루틴을 시작합니다.
        if (playerTransform != null)
        {
            StartCoroutine(SpawnAndFireRoutine());
        }
    }

    IEnumerator SpawnAndFireRoutine()
    {
        while (beatBounce.GetMusicTime() < 57f)
            yield return null;

        while (beatBounce.GetMusicTime() >= 57f && beatBounce.GetMusicTime() < 89f)
        {
            Vector3 fixedPlayerPos = playerTransform.position;
            List<GuidedMissile> spawnedMissiles = new List<GuidedMissile>();
            List<GameObject> spawnedOuters = new List<GameObject>(); // 🔸 큰 오브젝트 저장 리스트


            for (int i = 0; i < missileCount; i++)
            {
                float angleStep = missileCount > 1 ? spreadAngle / (missileCount - 1f) : 0;
                float angle = -spreadAngle / 2f + angleStep * i;

                Vector3 directionToTarget = (fixedPlayerPos - transform.position).normalized;
                Quaternion baseRotation = Quaternion.LookRotation(Vector3.forward, directionToTarget);
                Quaternion finalRotation = baseRotation * Quaternion.Euler(0, 0, angle);

                // 기존 탄막 위치 (작은 원)
                float innerRadius = 1.0f;
                Vector3 missilePos = fixedPlayerPos + (finalRotation * Vector3.up) * innerRadius;

                // 새 오브젝트 위치 (더 큰 원)
                float outerRadius = 2.5f;
                Vector3 outerPos = fixedPlayerPos + (finalRotation * Vector3.up) * outerRadius;

                // 탄막 생성
                GameObject missileObj = Instantiate(missilePrefab, missilePos, finalRotation);
                GuidedMissile missile = missileObj.GetComponent<GuidedMissile>();
                if (missile != null)
                {
                    missile.IsReadyToFire = false;
                    missile.missileSpeed = this.missileSpeed;
                    spawnedMissiles.Add(missile);
                }

                // 🔸 탄막 주위에 작은 원 3개 팡! 튀는 효과
                if (smallCirclePrefab != null)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        // 360도를 3등분해서 각 방향으로 튀게
                        float burstAngle = j * 120f;
                        Vector3 dir = Quaternion.Euler(0, 0, burstAngle) * Vector3.up;

                        // 살짝 랜덤 위치로 퍼지게
                        Vector3 spawnPos = missilePos + dir * Random.Range(0.2f, 0.5f);

                        GameObject circle = Instantiate(smallCirclePrefab, spawnPos, Quaternion.identity);

                        // 크기·속도 랜덤화
                        float moveDistance = Random.Range(0.8f, 1.3f);
                        float fadeTime = Random.Range(0.4f, 0.6f);
                        StartCoroutine(MoveAndFade(circle, dir, moveDistance, fadeTime));
                    }
                }

                // 🔸 더 큰 원 오브젝트 생성 + 페이드아웃
                if (outerObjectPrefab != null)
                {
                    GameObject outer = Instantiate(outerObjectPrefab, outerPos, finalRotation);
                    spawnedOuters.Add(outer);
                }

                yield return new WaitForSeconds(spawnDelay);
            }

            // ✅ 모든 탄막 생성 완료 후 → 큰 오브젝트 한꺼번에 사라지기 시작
            foreach (var outer in spawnedOuters)
            {
                if (outer != null)
                    StartCoroutine(FadeAndDestroy(outer, 0.8f));
            }

            yield return new WaitForSeconds(fireDelay);

            foreach (var missile in spawnedMissiles)
                if (missile != null)
                    missile.IsReadyToFire = true;
        }
    }

    private IEnumerator FadeAndDestroy(GameObject obj, float duration)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            yield break;
        }

        float elapsed = 0f;
        Color startColor = sr.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(obj);
    }

    private IEnumerator MoveAndFade(GameObject obj, Vector3 dir, float distance, float duration)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Vector3 startPos = obj.transform.position;
        Vector3 targetPos = startPos + dir * distance;

        float elapsed = 0f;
        Color startColor = sr.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 위치 이동
            obj.transform.position = Vector3.Lerp(startPos, targetPos, t);

            // 서서히 사라지기
            sr.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);

            // 크기 살짝 줄이기
            obj.transform.localScale = Vector3.one * (1f - 0.3f * t);

            yield return null;
        }

        Destroy(obj);
    }

}
