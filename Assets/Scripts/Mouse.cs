using UnityEngine;

public class Mouse : MonoBehaviour
{
    [Header("마우스 커서 설정")]
    [Tooltip("마우스 커서로 사용할 GameObject (비워두면 자기 자신)")]
    public Transform cursorObject;

    [Header("움직임 설정")]
    [Range(0f, 1f)]
    [Tooltip("부드러운 움직임 속도 (0: 즉시, 1: 매우 부드러움)")]
    public float smoothSpeed = 0.1f;

    [Tooltip("Z 위치 (카메라로부터의 거리)")]
    public float zPosition = 0f;

    [Header("옵션")]
    [Tooltip("시작 시 기본 마우스 커서 숨기기")]
    public bool hideDefaultCursor = true;

    private Camera mainCamera;
    private Vector3 targetPosition;

    void Start()
    {
        // 타겟이 없으면 자기 자신 사용
        if (cursorObject == null)
        {
            cursorObject = transform;
        }

        // 메인 카메라 가져오기
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("[Mouse] Main Camera를 찾을 수 없습니다!");
        }

        // 기본 마우스 커서 숨기기
        if (hideDefaultCursor)
        {
            Cursor.visible = false;
        }

        Debug.Log("[Mouse] 커스텀 마우스 커서 초기화 완료");
    }

    void Update()
    {
        if (mainCamera == null) return;

        // 마우스 위치를 월드 좌표로 변환
        Vector3 mousePos = Input.mousePosition;
        
        // Canvas UI를 사용하는 경우와 World Space를 사용하는 경우 구분
        if (cursorObject.GetComponent<RectTransform>() != null)
        {
            // UI 캔버스의 경우
            Canvas canvas = cursorObject.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                // Screen Space Overlay: 스크린 좌표 그대로 사용
                cursorObject.position = mousePos;
            }
            else
            {
                // World Space Canvas
                mousePos.z = Mathf.Abs(mainCamera.transform.position.z - cursorObject.position.z);
                targetPosition = mainCamera.ScreenToWorldPoint(mousePos);
                
                if (smoothSpeed > 0f)
                {
                    cursorObject.position = Vector3.Lerp(cursorObject.position, targetPosition, 1f - smoothSpeed);
                }
                else
                {
                    cursorObject.position = targetPosition;
                }
            }
        }
        else
        {
            // Sprite나 일반 GameObject의 경우
            mousePos.z = Mathf.Abs(mainCamera.transform.position.z - zPosition);
            targetPosition = mainCamera.ScreenToWorldPoint(mousePos);

            if (smoothSpeed > 0f)
            {
                cursorObject.position = Vector3.Lerp(cursorObject.position, targetPosition, 1f - smoothSpeed);
            }
            else
            {
                cursorObject.position = targetPosition;
            }
        }

        // 디버그: 위치 확인
        // Debug.Log($"[Mouse] Screen: {Input.mousePosition}, World: {cursorObject.position}");
    }

    void OnDestroy()
    {
        // 스크립트가 파괴될 때 기본 마우스 커서 다시 보이기
        Cursor.visible = true;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        // 게임 창이 포커스를 잃으면 마우스 커서 보이기
        if (!hasFocus)
        {
            Cursor.visible = true;
        }
        else if (hideDefaultCursor)
        {
            Cursor.visible = false;
        }
    }
}
