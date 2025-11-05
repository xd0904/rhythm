using UnityEngine;

public class WindowPush : MonoBehaviour
{
    public Transform window;
    public Transform player;

    private BoxCollider2D windowCollider;
    private Vector2 windowHalfSize;
    private Vector2 playerHalfSize;

    void Start()
    {
        windowCollider = window.GetComponent<BoxCollider2D>();
        windowHalfSize = windowCollider.size * 0.5f;

        BoxCollider2D playerCollider = player.GetComponent<BoxCollider2D>();
        playerHalfSize = playerCollider.size * 0.5f;

        // Z값 초기화 (깊이 좌표 안전하게)
        window.position = new Vector3(window.position.x, window.position.y, 0f);
    }

    void LateUpdate()
    {
        // 플레이어의 로컬 위치 (window 기준)
        Vector2 playerLocalPos = player.position - window.position;

        // window 안에서 플레이어가 가질 수 있는 최소/최대 위치 계산
        Vector2 min = -windowHalfSize + playerHalfSize;
        Vector2 max = windowHalfSize - playerHalfSize;

        // 플레이어의 위치를 경계 안으로 제한
        float clampedX = Mathf.Clamp(playerLocalPos.x, min.x, max.x);
        float clampedY = Mathf.Clamp(playerLocalPos.y, min.y, max.y);

        // 제한된 좌표를 다시 월드 좌표로 변환
        Vector2 clampedPos = new Vector2(clampedX, clampedY) + (Vector2)window.position;

        // 실제 플레이어 위치 갱신
        player.position = new Vector3(clampedPos.x, clampedPos.y, player.position.z);
    }
}


