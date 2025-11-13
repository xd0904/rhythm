using UnityEngine;

public class EyeBullet : MonoBehaviour
{
    private float speed;
    private float rotationSpeed;
    private Vector2 direction;
    private float currentAngle;

    public void Initialize(float bulletSpeed, float bulletRotationSpeed, Vector3 targetPosition)
    {
        speed = bulletSpeed;
        rotationSpeed = bulletRotationSpeed;

        // 전달받은 targetPosition(플레이어 위치) 방향으로 초기 방향 설정
        direction = (targetPosition - transform.position).normalized;
        currentAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 5초 후 자동 삭제
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        // 탄막 자체는 회전 (시각 효과)
        currentAngle += rotationSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0, 0, currentAngle);

        // 이동은 발사 시점의 방향으로 직진
        transform.position += (Vector3)direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어와 충돌 시
        if (collision.CompareTag("Player"))
        {
            Destroy(gameObject);
        }

        // 화면 밖으로 나가면 삭제
        if (collision.CompareTag("Border"))
        {
            Destroy(gameObject);
        }
    }
}