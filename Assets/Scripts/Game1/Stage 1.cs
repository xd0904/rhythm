using UnityEngine;

public class Stage1 : MonoBehaviour
{
    [Header("오브젝트 참조")]
    public Transform window;
    public Transform player;

    [Header("움직임 설정")]
    public float windowFollowSpeed = 5f; // 창이 마우스를 따라가는 속도
    public float playerFollowSpeed = 2f; // 플레이어가 창을 따라오는 속도
    public float windowCatchDuration = 2f; // 창이 마우스를 따라가는 시간
    public float windowReleaseDuration = 1f; // 창이 멈춰있는 시간

    [Header("마우스 시뮬레이션")]
    public Vector2[] pathPoints; // 마우스가 이동할 좌표들
    public float moveDurationPerPoint = 2f; // 각 구간 이동 시간

    private Vector3 simulatedMousePos;
    private int currentTargetIndex = 0;
    private float moveTimer = 0f;
    private bool windowFollowing = true; // 창이 지금 마우스를 따라가는 중인지
    private float followTimer = 0f;

    void Start()
    {
        if (pathPoints.Length > 0)
            simulatedMousePos = pathPoints[0];
    }

    void Update()
    {
        // 1?? 시간에 따라 마우스 자동 이동
        if (pathPoints.Length > 1)
        {
            moveTimer += Time.deltaTime;
            float t = moveTimer / moveDurationPerPoint;
            simulatedMousePos = Vector2.Lerp(pathPoints[currentTargetIndex], pathPoints[(currentTargetIndex + 1) % pathPoints.Length], t);

            if (t >= 1f)
            {
                moveTimer = 0f;
                currentTargetIndex = (currentTargetIndex + 1) % pathPoints.Length;
            }
        }

        // 2?? 창이 마우스를 따라갔다가 멈췄다가 반복
        followTimer += Time.deltaTime;
        if (windowFollowing && followTimer >= windowCatchDuration)
        {
            windowFollowing = false;
            followTimer = 0f;
        }
        else if (!windowFollowing && followTimer >= windowReleaseDuration)
        {
            windowFollowing = true;
            followTimer = 0f;
        }

        if (windowFollowing)
        {
            window.position = Vector3.Lerp(window.position, simulatedMousePos, Time.deltaTime * windowFollowSpeed);
        }

        // 플레이어는 자유롭게 이동 (Player.cs에서 제어)
    }
}
