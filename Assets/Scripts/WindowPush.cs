using UnityEngine;

public class WindowPush : MonoBehaviour
{
    public Transform window;
    public Transform player;
    public float windowPushFactor = 0.05f; // â �и� ����
    public float maxPushPerFrame = 0.05f;  // �� ������ �ִ� �̵���

    private BoxCollider2D windowCollider;
    private Vector2 windowHalfSize;
    private Vector2 playerHalfSize;

    void Start()
    {
        windowCollider = window.GetComponent<BoxCollider2D>();
        windowHalfSize = windowCollider.size * 0.5f;

        BoxCollider2D playerCollider = player.GetComponent<BoxCollider2D>();
        playerHalfSize = playerCollider.size * 0.5f;

        // Z�� �����ϰ� �ʱ�ȭ
        window.position = new Vector3(window.position.x, window.position.y, 0f);
    }

    void LateUpdate()
    {
        Vector2 playerLocalPos = player.position - window.position;
        Vector2 min = -windowHalfSize + playerHalfSize;
        Vector2 max = windowHalfSize - playerHalfSize;

        Vector2 push = Vector2.zero;

        // X��
        if (playerLocalPos.x < min.x) push.x = -maxPushPerFrame;
        else if (playerLocalPos.x > max.x) push.x = maxPushPerFrame;

        // Y��
        if (playerLocalPos.y < min.y) push.y = -maxPushPerFrame;
        else if (playerLocalPos.y > max.y) push.y = maxPushPerFrame;

        // â �̵�
        if (push != Vector2.zero)
        {
            window.position += new Vector3(push.x * windowPushFactor, push.y * windowPushFactor, 0f);
        }
    }
}


