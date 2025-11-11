using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScreenFlipController : MonoBehaviour
{
    [Header("플립 설정")]
    public Camera mainCamera;
    public float flipDuration = 1.5f; // 플립 애니메이션 시간
    public float dragThreshold = 100f; // 드래그 거리 임계값 (픽셀)
    public float autoFlipTime = 44f; // 자동 플립 시간 (44초)
    
    [Header("게임 오브젝트")]
    public Transform gameObjectsParent; // 모든 게임 오브젝트의 부모 (Canvas 제외)
    
    [Header("마우스 커서")]
    public Transform mouseCursor; // 마우스 오브젝트 (플립되면 안 됨)
    
    [Header("드래그 효과")]
    public Color dragLineColor = new Color(0.3f, 0.8f, 1f, 0.8f); // 청록색
    public float dragLineWidth = 5f;
    
    [Header("마우스 이동")]
    public float mouseMoveStartTime = 42f; // 마우스 이동 시작 시간 (44초 2초 전)
    public float mouseMoveDuration = 1.5f; // 마우스 이동 시간

    private bool isFlipped = false;
    private bool isFlipping = false;
    private Vector3 dragStartPos;
    private bool isDragging = false;
    private bool autoFlipTriggered = false;
    private bool mouseMoveTriggered = false;
    
    // 드래그 시각 효과용
    private GameObject dragLineObject;
    private SpriteRenderer dragLineRenderer;
    private List<GameObject> dragParticles = new List<GameObject>();

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        // 타이밍 강제 설정 (Inspector 값 무시)
        autoFlipTime = 44f;
        mouseMoveStartTime = 42f;
        mouseMoveDuration = 1.5f;
        
        Debug.Log($"[ScreenFlip] 타이밍 설정: 마우스 이동={mouseMoveStartTime}초, 플립={autoFlipTime}초");
    }

    void Update()
    {
        if (BeatBounce.Instance == null) return; // BeatBounce 없으면 대기
        
        double musicTime = BeatBounce.Instance.GetMusicTime();
        
        // 음악이 아직 시작 안 했으면 대기 (음수 또는 0)
        if (musicTime <= 0)
        {
            // Debug.Log($"[ScreenFlip] 음악 대기 중... musicTime: {musicTime}");
            return;
        }
        
        // 42초에 마우스를 오른쪽으로 이동
        if (!mouseMoveTriggered && musicTime >= mouseMoveStartTime)
        {
            Debug.Log($"[ScreenFlip] 마우스 이동 트리거! musicTime: {musicTime}, 목표: {mouseMoveStartTime}");
            mouseMoveTriggered = true;
            StartCoroutine(MoveMouseToRight());
        }
        
        // 44초에 자동 플립
        if (!autoFlipTriggered && musicTime >= autoFlipTime)
        {
            Debug.Log($"[ScreenFlip] 플립 트리거! musicTime: {musicTime}, 목표: {autoFlipTime}");
            autoFlipTriggered = true;
            StartCoroutine(FlipScreen());
            return;
        }
        
        // 이미 플립 중이면 입력 무시
        if (isFlipping) return;
        
        // 마우스 왼쪽 버튼 눌렀을 때
        if (Input.GetMouseButtonDown(0))
        {
            // 화면 오른쪽 끝 영역인지 체크 (화면 너비의 90% 이상)
            if (Input.mousePosition.x > Screen.width * 0.9f)
            {
                dragStartPos = Input.mousePosition;
                isDragging = true;
                CreateDragLine();
            }
        }
        
        // 드래그 중
        if (isDragging && Input.GetMouseButton(0))
        {
            UpdateDragVisuals();
            
            float dragDistance = dragStartPos.x - Input.mousePosition.x;
            float dragProgress = Mathf.Clamp01(dragDistance / dragThreshold);
            
            // 드래그 진행도에 따라 화면 프리뷰 회전
            UpdateFlipPreview(dragProgress);
            
            // 왼쪽으로 충분히 드래그했으면 플립 실행
            if (dragDistance > dragThreshold)
            {
                isDragging = false;
                DestroyDragVisuals();
                StartCoroutine(FlipScreen());
            }
        }
        
        // 마우스 버튼 떼면 드래그 취소 (리셋)
        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                isDragging = false;
                DestroyDragVisuals();
                StartCoroutine(ResetFlipPreview());
            }
        }
    }
    
    IEnumerator MoveMouseToRight()
    {
        if (mouseCursor == null)
        {
            Debug.LogWarning("[ScreenFlip] 마우스 커서가 할당되지 않았습니다!");
            yield break;
        }
        
        Vector3 startPos = mouseCursor.position;
        
        // 화면 오른쪽 끝 위치 계산 (약간 안쪽으로)
        Vector3 screenRightPos = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width * 0.95f, Screen.height * 0.5f, 10f));
        screenRightPos.z = startPos.z; // Z 위치는 유지
        
        float elapsed = 0f;
        
        Debug.Log($"[ScreenFlip] 마우스를 오른쪽으로 이동 시작: {startPos} → {screenRightPos}");
        
        while (elapsed < mouseMoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / mouseMoveDuration;
            
            // Ease-in-out 곡선으로 부드럽게 이동
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            mouseCursor.position = Vector3.Lerp(startPos, screenRightPos, smoothT);
            
            yield return null;
        }
        
        // 최종 위치 확정
        mouseCursor.position = screenRightPos;
        
        Debug.Log($"[ScreenFlip] 마우스 이동 완료! 위치: {mouseCursor.position}");
    }

    void CreateDragLine()
    {
        // 드래그 라인 오브젝트 생성
        dragLineObject = new GameObject("DragLine");
        dragLineRenderer = dragLineObject.AddComponent<SpriteRenderer>();
        
        // 1픽셀 텍스처 생성
        Texture2D lineTexture = new Texture2D(1, 1);
        lineTexture.SetPixel(0, 0, dragLineColor);
        lineTexture.Apply();
        
        Sprite lineSprite = Sprite.Create(lineTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        dragLineRenderer.sprite = lineSprite;
        dragLineRenderer.sortingOrder = 1000; // 최상위에 표시
        
        dragLineObject.transform.position = mainCamera.ScreenToWorldPoint(new Vector3(dragStartPos.x, dragStartPos.y, 10f));
    }
    
    void UpdateDragVisuals()
    {
        if (dragLineObject == null) return;
        
        Vector3 startWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(dragStartPos.x, dragStartPos.y, 10f));
        Vector3 currentWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
        
        // 드래그 라인 위치 및 크기 업데이트
        Vector3 midPoint = (startWorldPos + currentWorldPos) / 2f;
        float distance = Vector3.Distance(startWorldPos, currentWorldPos);
        
        dragLineObject.transform.position = midPoint;
        dragLineObject.transform.localScale = new Vector3(distance, dragLineWidth, 1f);
        
        // 드래그 진행도에 따라 파티클 생성
        float dragDistance = dragStartPos.x - Input.mousePosition.x;
        if (dragDistance > 0 && dragDistance % 10 < 2) // 10픽셀마다
        {
            CreateDragParticle(currentWorldPos);
        }
    }
    
    void CreateDragParticle(Vector3 position)
    {
        GameObject particle = new GameObject("DragParticle");
        SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();
        
        // 1픽셀 텍스처 생성
        Texture2D particleTexture = new Texture2D(1, 1);
        particleTexture.SetPixel(0, 0, dragLineColor);
        particleTexture.Apply();
        
        Sprite particleSprite = Sprite.Create(particleTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 20f);
        sr.sprite = particleSprite;
        sr.sortingOrder = 999;
        
        particle.transform.position = position;
        particle.transform.localScale = Vector3.one * 2f;
        
        dragParticles.Add(particle);
        StartCoroutine(AnimateDragParticle(particle));
    }
    
    IEnumerator AnimateDragParticle(GameObject particle)
    {
        if (particle == null) yield break;
        
        SpriteRenderer sr = particle.GetComponent<SpriteRenderer>();
        float lifetime = 0.5f;
        float elapsed = 0f;
        Vector3 startScale = particle.transform.localScale;
        
        while (elapsed < lifetime && particle != null)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / lifetime;
            
            // 크기 감소 및 페이드아웃
            particle.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
            
            Color color = sr.color;
            color.a = 1f - progress;
            sr.color = color;
            
            yield return null;
        }
        
        if (particle != null)
        {
            Destroy(particle);
        }
    }
    
    void DestroyDragVisuals()
    {
        // 드래그 라인 제거
        if (dragLineObject != null)
        {
            Destroy(dragLineObject);
            dragLineObject = null;
        }
        
        // 모든 파티클 제거
        foreach (GameObject particle in dragParticles)
        {
            if (particle != null)
            {
                Destroy(particle);
            }
        }
        dragParticles.Clear();
    }
    
    void UpdateFlipPreview(float progress)
    {
        // 드래그 진행도에 따라 화면을 부분적으로 회전
        Transform[] children = gameObjectsParent != null ? gameObjectsParent.GetComponentsInChildren<Transform>() : new Transform[0];
        
        float startRotation = isFlipped ? 180f : 0f;
        float targetRotation = isFlipped ? 0f : 180f;
        float currentRotation = Mathf.Lerp(startRotation, targetRotation, progress);
        
        Quaternion targetQuat = Quaternion.Euler(0f, currentRotation, 0f);
        
        foreach (Transform child in children)
        {
            if (child != null && child != gameObjectsParent)
            {
                // 자식의 원래 로컬 회전 유지하며 Y축 회전 적용
                child.rotation = Quaternion.Euler(0f, currentRotation, 0f) * Quaternion.Euler(child.localEulerAngles);
            }
        }
    }
    
    IEnumerator ResetFlipPreview()
    {
        // 드래그를 중단했을 때 원래 상태로 부드럽게 복귀
        float duration = 0.3f;
        float elapsed = 0f;
        
        Transform[] children = gameObjectsParent != null ? gameObjectsParent.GetComponentsInChildren<Transform>() : new Transform[0];
        Quaternion[] startRotations = new Quaternion[children.Length];
        
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] != null)
            {
                startRotations[i] = children[i].rotation;
            }
        }
        
        float targetRotation = isFlipped ? 180f : 0f;
        Quaternion targetQuat = Quaternion.Euler(0f, targetRotation, 0f);
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i] != null && children[i] != gameObjectsParent)
                {
                    children[i].rotation = Quaternion.Slerp(startRotations[i], targetQuat, t);
                }
            }
            
            yield return null;
        }
        
        // 최종 상태 확정
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] != null && children[i] != gameObjectsParent)
            {
                children[i].rotation = targetQuat;
            }
        }
    }

    IEnumerator FlipScreen()
    {
        isFlipping = true;
        
        float elapsed = 0f;
        float startRotation = isFlipped ? 180f : 0f;
        float targetRotation = isFlipped ? 0f : 180f;
        
        // 자식 오브젝트들의 원래 로컬 회전 저장
        Transform[] children = gameObjectsParent != null ? gameObjectsParent.GetComponentsInChildren<Transform>() : new Transform[0];
        Quaternion[] originalRotations = new Quaternion[children.Length];
        
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] != gameObjectsParent) // 부모 자신 제외
            {
                originalRotations[i] = children[i].rotation;
            }
        }
        
        // 플립 애니메이션
        while (elapsed < flipDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flipDuration;
            
            // Ease-in-out 곡선
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            // Y축 회전 (0 → 180 또는 180 → 0)
            float currentRotation = Mathf.Lerp(startRotation, targetRotation, smoothT);
            Quaternion targetQuat = Quaternion.Euler(0f, currentRotation, 0f);
            
            // 모든 자식 오브젝트들을 각각 회전
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i] != null && children[i] != gameObjectsParent)
                {
                    // 원래 회전에 Y축 회전 추가
                    children[i].rotation = originalRotations[i] * targetQuat;
                }
            }
            
            yield return null;
        }
        
        // 최종 상태 설정
        Quaternion finalQuat = Quaternion.Euler(0f, targetRotation, 0f);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] != null && children[i] != gameObjectsParent)
            {
                children[i].rotation = originalRotations[i] * finalQuat;
            }
        }
        
        isFlipped = !isFlipped;
        isFlipping = false;
        
        Debug.Log($"[ScreenFlip] 화면 플립 완료! 현재 상태: {(isFlipped ? "좌우반전" : "정상")}");
    }
    
    // 외부에서 강제로 플립 (키 입력 등)
    public void ToggleFlip()
    {
        if (!isFlipping)
        {
            StartCoroutine(FlipScreen());
        }
    }
}
