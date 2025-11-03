using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 5f; // 이동 속도
    
    [Header("경계 설정")]
    public float minX = -8f;  // 왼쪽 경계
    public float maxX = 8f;   // 오른쪽 경계
    public float minY = -4.5f; // 아래쪽 경계
    public float maxY = 4.5f;  // 위쪽 경계

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Rigidbody2D 설정 강제 조정
        if (rb != null)
        {
            rb.gravityScale = 0f;        // 중력 완전히 끄기
            rb.linearDamping = 0f;       // 선형 감쇠 끄기
            rb.angularDamping = 0f;      // 회전 감쇠 끄기
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // 회전 고정
        }
    }

    void Update()
    {
        // 입력 받기
        float moveX = Input.GetAxisRaw("Horizontal"); // A, D
        float moveY = Input.GetAxisRaw("Vertical");   // W, S

        moveInput = new Vector2(moveX, moveY).normalized; // 대각선 이동 시 속도 보정
    }

    void FixedUpdate()
    {
        // 이동 적용
        rb.linearVelocity = moveInput * speed;
        
        // 위치 제한 (창 밖으로 못 나가게)
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }
}
