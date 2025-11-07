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
            // 1. 플레이어 위치 저장
            Vector3 targetPosition = playerTransform.position;
            // 스포너의 현재 위치
            Vector3 spawnCenter = transform.position;

            // 생성된 미사일을 저장할 리스트
            List<GuidedMissile> spawnedMissiles = new List<GuidedMissile>();

            // 2. 미사일 생성 및 곡선 배치
            for (int i = 0; i < missileCount; i++)
            {
                // 2-1. 부채꼴 각도 계산
                // -spreadAngle/2 부터 +spreadAngle/2 까지 균등하게 각도를 분배
                // missileCount가 1개일 때를 대비하여 (missileCount - 1f) 대신 안전하게 나눕니다.
                float angleStep = missileCount > 1 ? spreadAngle / (missileCount - 1f) : 0;
                float angle = -spreadAngle / 2f + angleStep * i;

                // 2-2. 회전 계산: 플레이어를 바라보는 회전에 부채꼴 각도를 더함

                // 스포너 -> 플레이어를 향하는 벡터 (기본 방향)
                Vector3 directionToTarget = (targetPosition - spawnCenter).normalized;

                // 기본 회전값 (플레이어를 정면으로 바라봄)
                Quaternion baseRotation = Quaternion.LookRotation(Vector3.forward, directionToTarget); // 2D LookAt 방식 (옵션)

                // 부채꼴 각도(angle)만큼 Y축을 회전시켜 최종 회전값을 만듦
                Quaternion finalRotation = baseRotation * Quaternion.Euler(0, 0, angle);

                // 2-3. 위치 계산: (가장 중요한 수정 부분)
                // 1) 생성 위치를 스포너 앞으로 고정된 거리(예: 0.5f)에 배치합니다.
                Vector3 fixedForward = spawnCenter + directionToTarget * 0.5f;

                // 2) 이 고정된 위치에서 'finalRotation'을 이용하여 미사일을 'spawnRadius'만큼 옆으로 펼칩니다.
                // 이렇게 하면 미사일이 플레이어 방향을 향하는 축을 따라 부채꼴로 펼쳐지게 됩니다.
                Vector3 spawnOffset = finalRotation * Vector3.right * spawnRadius * (i - (missileCount - 1f) / 2f);

                // 미사일 간 간격이 균일하게 벌어지도록 조정 (선택 사항)
                // Vector3 spawnOffset = finalRotation * Vector3.right * spawnRadius * (i - (missileCount - 1f) / 2f); 
                // 👆 이 방식 대신, 미사일 개수에 따라 균일한 간격을 사용하면 더 깔끔합니다.

                float spacing = 0.3f;
                float offsetDistance = spacing * i - spawnRadius / 2f;
                Vector3 sideOffset = finalRotation * Vector3.right * offsetDistance;

                // 최종 생성 위치는 스포너 위치가 아닌, 플레이어 방향 앞의 가상 선상입니다.
                Vector3 spawnPosition = spawnCenter + directionToTarget * 0.5f + sideOffset;


                // 2-4. 미사일 생성
                GameObject missileObj = Instantiate(missilePrefab, spawnPosition, finalRotation);
                GuidedMissile missileScript = missileObj.GetComponent<GuidedMissile>();

                if (missileScript != null)
                {
                    // 초기 설정: 이동 정지, 속도 설정
                    missileScript.IsReadyToFire = false;
                    missileScript.missileSpeed = this.missileSpeed;
                    spawnedMissiles.Add(missileScript);
                }

                // 다음 미사일 생성까지 대기
                yield return new WaitForSeconds(spawnDelay);
            }

            // 3. 발사 지연 후 동시 발사
            yield return new WaitForSeconds(fireDelay);

            // 모든 미사일 동시 발사 명령
            foreach (var missile in spawnedMissiles)
            {
                if (missile != null)
                {
                    missile.IsReadyToFire = true; // 발사 허용
                }
            }

        }
    }
}
