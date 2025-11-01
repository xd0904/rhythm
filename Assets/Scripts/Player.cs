using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 5f; // 이동 속도

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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
    }
}
