using UnityEngine;

public class MenuShake : MonoBehaviour
{
    [Header("타겟 오브젝트")]
    [Tooltip("흔들릴 게임오브젝트")]
    public Transform targetObject;

    [Header("움직임 설정")]
    [Range(0f, 100f)]
    [Tooltip("마우스 움직임에 대한 반응 강도")]
    public float moveStrength = 10f;

    [Range(0f, 1f)]
    [Tooltip("부드러운 움직임 속도 (0: 즉시, 1: 매우 느림)")]
    public float smoothSpeed = 0.1f;

    [Range(0f, 100f)]
    [Tooltip("최대 이동 거리 제한")]
    public float maxDistance = 20f;

    [Header("반전 설정")]
    [Tooltip("X축 반전 (true: 마우스 반대방향)")]
    public bool invertX = true;

    [Tooltip("Y축 반전 (true: 마우스 반대방향)")]
    public bool invertY = true;

    private Vector3 initialPosition;
    private Vector3 targetPosition;
    private Vector2 screenCenter;

    void Start()
    {
        // 타겟이 없으면 자기 자신 사용
        if (targetObject == null)
        {
            targetObject = transform;
        }

        // 초기 위치 저장
        initialPosition = targetObject.localPosition;

        // 화면 중심 계산
        screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
    }

    void Update()
    {
        // 마우스 위치 가져오기
        Vector2 mousePosition = Input.mousePosition;

        // 화면 중심으로부터 마우스의 상대적 위치 계산 (-1 ~ 1 범위로 정규화)
        float offsetX = (mousePosition.x - screenCenter.x) / screenCenter.x;
        float offsetY = (mousePosition.y - screenCenter.y) / screenCenter.y;

        // 반전 적용
        if (invertX) offsetX = -offsetX;
        if (invertY) offsetY = -offsetY;

        // 타겟 위치 계산 (초기 위치 + 오프셋)
        Vector3 offset = new Vector3(offsetX * moveStrength, offsetY * moveStrength, 0f);

        // 최대 거리 제한
        if (offset.magnitude > maxDistance)
        {
            offset = offset.normalized * maxDistance;
        }

        targetPosition = initialPosition + offset;

        // 부드럽게 이동 (Lerp)
        targetObject.localPosition = Vector3.Lerp(
            targetObject.localPosition,
            targetPosition,
            1f - smoothSpeed
        );
    }

    // Inspector에서 값 변경 시 초기 위치 업데이트
    void OnValidate()
    {
        if (targetObject != null && Application.isPlaying)
        {
            initialPosition = targetObject.localPosition;
        }
    }

    // 기즈모로 최대 이동 범위 표시 (Scene 뷰에서만 보임)
    void OnDrawGizmosSelected()
    {
        if (targetObject == null) return;

        Gizmos.color = Color.yellow;
        Vector3 center = Application.isPlaying ? initialPosition : targetObject.localPosition;
        Gizmos.DrawWireSphere(targetObject.parent ? targetObject.parent.TransformPoint(center) : center, maxDistance);
    }
}
