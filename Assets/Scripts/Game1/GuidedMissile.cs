using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(TrailRenderer))]

public class GuidedMissile : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float destroyBoundary = 12f;
    public float homingDuration = 0.3f;
    public float reflectDuration = 0.5f;

    [Header("Visual Effects")]
    public GameObject explosionEffectPrefab;
    public Color idleColor = Color.white;
    public Color launchColor = Color.red;
    public float flashIntensity = 2f;
    public float colorFadeSpeed = 5f;

    [HideInInspector] public Vector3 initialOffset;

    private Vector3 spawnPosition;
    private Vector3 targetDirection;
    private GameObject playerTarget;
    private SpriteRenderer sr;
    private TrailRenderer trail;

    private bool isLaunched = false;
    private float startTime;
    private float reflectTime;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        trail = GetComponent<TrailRenderer>();

        playerTarget = GameObject.FindGameObjectWithTag("Player");

        // ⭐ 1. 생성된 시점의 고정 위치 계산
        if (playerTarget != null)
            spawnPosition = playerTarget.transform.position + initialOffset;
        else
            spawnPosition = transform.position;

        transform.position = spawnPosition;

        // ⭐ 2. 초기 시각 및 랜덤 회전 설정 (이 회전이 발사 시까지 고정됩니다)
        sr.color = idleColor;
        trail.emitting = false;

        // ⭐ 랜덤 회전 설정
        float randomAngle = Random.Range(0f, 360f);
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, randomAngle));
    }

    public void Launch()
    {
        if (isLaunched) return;
        isLaunched = true;
        startTime = Time.time;

        // 1. 발사 시 색상 변화 및 궤적 활성화
        sr.color = launchColor * flashIntensity;
        trail.emitting = true;

        // 폭발 효과 생성
        if (explosionEffectPrefab != null)
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

        // ⭐ 2. 플레이어 방향 계산 (회전은 하지 않습니다. 이동 방향만 설정)
        if (playerTarget != null)
        {
            targetDirection = (playerTarget.transform.position - transform.position).normalized;

            // ⭐⭐⭐ 주의: 발사 시 플레이어를 바라보도록 회전하는 코드를 제거했습니다. ⭐⭐⭐
            // 미사일은 랜덤 회전을 유지한 채 플레이어를 향해 날아갑니다.
        }
        else
        {
            targetDirection = Vector3.down;
        }
    }

    void Update()
    {
        // 1. 색상 페이드 아웃 (발사된 경우에만)
        if (isLaunched)
        {
            sr.color = Color.Lerp(sr.color, launchColor, Time.deltaTime * colorFadeSpeed);
        }

        if (!isLaunched)
        {
            // ⭐ 2. 대기 상태: 위치 고정 및 회전 고정
            transform.position = spawnPosition;
            // 회전 코드가 없으므로, Start()에서 설정된 랜덤 회전을 유지합니다.
            return;
        }

        // =============================================
        // ⭐ 발사 (Launched) 로직
        // =============================================

        // 3. 유도/반사 시간 처리
        bool isReflecting = Time.time < reflectTime + reflectDuration;

        if (!isReflecting && Time.time < startTime + homingDuration && playerTarget != null)
        {
            // 유도 유지 시간 동안 방향 갱신 (⭐⭐⭐ 회전은 하지 않고 이동 방향만 갱신)
            targetDirection = (playerTarget.transform.position - transform.position).normalized;
        }

        // 4. 이동 (회전은 하지 않습니다)
        transform.position += targetDirection * moveSpeed * Time.deltaTime;

        // ⭐⭐⭐ 주의: 이동 방향에 맞춰 미사일 회전 업데이트 하는 코드를 제거했습니다. ⭐⭐⭐
        // 미사일은 발사 시의 랜덤 회전 또는 벽 충돌 시의 반사 회전만 유지합니다.

        // 5. 화면 밖 파괴
        if (Mathf.Abs(transform.position.x) > destroyBoundary ||
            Mathf.Abs(transform.position.y) > destroyBoundary)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. 플레이어 충돌
        if (other.CompareTag("Player"))
        {
            if (explosionEffectPrefab != null)
                Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }

        // 2. 벽 충돌 → 반사 및 파편 효과
        else if (other.CompareTag("Wall"))
        {
            if (explosionEffectPrefab != null)
                Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

            // 벽 충돌 지점의 법선 벡터를 계산하여 방향 반사
            Vector2 normal = other.ClosestPoint(transform.position) - (Vector2)transform.position;
            normal.Normalize();
            targetDirection = Vector3.Reflect(targetDirection, normal);

            // ⭐ 반사 시 미사일의 회전도 변경 (벽 충돌 후 방향을 바라보게 됨)
            float reflectAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, reflectAngle - 90));

            reflectTime = Time.time;
        }
    }
}
