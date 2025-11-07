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
        // 1. 패턴 시작 시간(57초)까지 대기
        while (beatBounce.GetMusicTime() < 57f) { yield return null; }


        while (beatBounce.GetMusicTime() >= 57f && beatBounce.GetMusicTime() < 89f)
        {
            // ✅ 1. 발사 시작 시점에 플레이어 위치 '고정' (중요!)
            Vector3 fixedPlayerPos = playerTransform.position;


            // 생성된 미사일을 저장할 리스트
            List<GuidedMissile> spawnedMissiles = new List<GuidedMissile>();

            // ✅ 2. 미사일 생성
            for (int i = 0; i < missileCount; i++)
            {
                // 부채꼴 각도 계산
                float angleStep = missileCount > 1 ? spreadAngle / (missileCount - 1f) : 0;
                float angle = -spreadAngle / 2f + angleStep * i;

                // 스포너 → 고정된 플레이어 위치 방향
                Vector3 directionToTarget = (fixedPlayerPos - transform.position).normalized;

                // 기본 회전 (플레이어를 향함)
                Quaternion baseRotation = Quaternion.LookRotation(Vector3.forward, directionToTarget);

                // 부채꼴 각도만큼 회전 추가
                Quaternion finalRotation = baseRotation * Quaternion.Euler(0, 0, angle);

                // ✅ 플레이어의 '고정된 위치' 주변에 생성 (움직여도 변하지 않음)
                float spawnRadius = 1.0f;
                Vector3 spawnPos = fixedPlayerPos + (finalRotation * Vector3.up) * spawnRadius;

                // 미사일 생성
                GameObject missileObj = Instantiate(missilePrefab, spawnPos, finalRotation);
                GuidedMissile missile = missileObj.GetComponent<GuidedMissile>();

                if (missile != null)
                {
                    missile.IsReadyToFire = false;
                    missile.missileSpeed = this.missileSpeed;
                    spawnedMissiles.Add(missile);
                }

                // 미사일 사이 생성 딜레이
                yield return new WaitForSeconds(spawnDelay);
            }

            // ✅ 3. 잠깐 대기 후 일제히 발사
            yield return new WaitForSeconds(fireDelay);

            foreach (var missile in spawnedMissiles)
            {
                if (missile != null)
                    missile.IsReadyToFire = true;
            }

        }
    }
}
