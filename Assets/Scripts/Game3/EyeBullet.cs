using UnityEngine;

public class EyeBullet : MonoBehaviour
{

    private float speed;
    private float rotationSpeed;
    private Vector2 direction;
    private float currentAngle;

    public void Initialize(float bulletSpeed, float bulletRotationSpeed)
    {
        speed = bulletSpeed;
        rotationSpeed = bulletRotationSpeed;

        // 플레이어 방향으로 초기 방향 설정
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            direction = (player.transform.position - transform.position).normalized;
            currentAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }
        else
        {
            // 플레이어를 찾지 못하면 아래쪽으로
            direction = Vector2.down;
            currentAngle = -90f;
        }

        // 5초 후 자동 삭제
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        // 빙글빙글 회전
        currentAngle += rotationSpeed * Time.deltaTime;

        // 각도를 방향 벡터로 변환
        float radians = currentAngle * Mathf.Deg2Rad;
        direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));

        // 회전하는 방향으로 이동
        transform.position += (Vector3)direction * speed * Time.deltaTime;

        // 탄막 자체도 회전 (시각 효과)
        transform.rotation = Quaternion.Euler(0, 0, currentAngle);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어와 충돌 시
        if (collision.CompareTag("Player"))
        {
            Destroy(gameObject);
        }

        // 화면 밖으로 나가면 삭제
        if (collision.CompareTag("Border") || collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
