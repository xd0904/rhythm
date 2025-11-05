using UnityEngine;

public class GuidedMissile : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float destroyBoundary = 12f; // 화면 밖 경계
    public float homingDuration = 0.3f; // 유도 유지 시간

    // 외부에서 설정될 초기 오프셋 (회전 계산에만 사용)
    [HideInInspector] public Vector3 initialOffset;

    private Vector3 targetDirection;
    private GameObject playerTarget;
    private float startTime;
    private bool isLaunched = false; // 이동 대기 플래그

    private Vector3 spawnPosition; // ⭐ 탄막의 고정 위치

    void Start()
    {
        // 1. 플레이어 오브젝트를 찾습니다.
        playerTarget = GameObject.FindGameObjectWithTag("Player");

        // ⭐ 2. 현재 탄막이 생성된 월드 위치를 spawnPosition에 저장합니다.
        spawnPosition = transform.position;
    }

    // ⭐ PatternController에서 호출될 공격 시작 함수
    public void Launch()
    {
        isLaunched = true;
        startTime = Time.time; // 발사 시간 기록

        // 공격 명령을 받은 시점의 플레이어 위치를 향해 방향 설정
        if (playerTarget != null)
        {
            targetDirection = (playerTarget.transform.position - transform.position).normalized;
        }
        else
        {
            targetDirection = Vector3.down;
        }
    }

    void Update()
    {
        if (playerTarget == null) return;

        if (!isLaunched)
        {
            // =============================================
            // ⭐ 대기 상태: 위치 고정 및 플레이어 바라보기
            // =============================================

            // ⭐ 위치를 생성 당시의 위치(spawnPosition)로 강제 고정
            transform.position = spawnPosition;

            // 플레이어를 바라보도록 회전
            Vector3 dir = playerTarget.transform.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));

            return; // 발사되지 않았으면 여기서 Update 종료
        }

        // =============================================
        // ⭐ 발사 (Launched) 로직 (isLaunched == true일 때만 실행)
        // =============================================

        // 1. 유도 (Homing) 로직: 유도 시간 동안 방향 갱신
        if (Time.time < startTime + homingDuration)
        {
            targetDirection = (playerTarget.transform.position - transform.position).normalized;

            float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
        }

        // 2. 이동 로직
        transform.position += targetDirection * moveSpeed * Time.deltaTime;

        // 3. 화면 밖 파괴 로직
        if (Mathf.Abs(transform.position.x) > destroyBoundary ||
            Mathf.Abs(transform.position.y) > destroyBoundary)
        {
            Destroy(gameObject);
        }
    }

    // 4. 충돌 파괴 로직 (플레이어 피격)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // ⭐ (여기서 플레이어 피격 처리 로직을 호출)
            Destroy(gameObject);
        }
    }
}
