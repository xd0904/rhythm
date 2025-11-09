using UnityEngine;

public class WindowPush : MonoBehaviour
{
    public Transform window;
    public Transform player;

    private Player playerScript;

    void Start()
    {
        // Player 스크립트 참조 가져오기
        if (player != null)
        {
            playerScript = player.GetComponent<Player>();
        }

        // Z�� �ʱ�ȭ (���� ��ǥ �����ϰ�)
        window.position = new Vector3(window.position.x, window.position.y, 0f);
    }

    void LateUpdate()
    {
        // Player 스크립트가 없으면 실행하지 않음
        if (playerScript == null || player == null) return;

        // Player.cs의 경계 값 사용
        float minX = playerScript.minX;
        float maxX = playerScript.maxX;
        float minY = playerScript.minY;
        float maxY = playerScript.maxY;

        // 플레이어의 위치를 경계 안으로 제한
        Vector3 playerPos = player.position;
        float clampedX = Mathf.Clamp(playerPos.x, minX, maxX);
        float clampedY = Mathf.Clamp(playerPos.y, minY, maxY);

        // 제한된 플레이어 위치 적용
        player.position = new Vector3(clampedX, clampedY, playerPos.z);
    }
}


