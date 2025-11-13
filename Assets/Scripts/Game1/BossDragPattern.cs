using UnityEngine;
using System.Collections;

public class BossDragPattern : MonoBehaviour
{
    [Header("타이밍 설정")]
    public float patternStartTime = 96f; // 1분 36초
    public float patternEndTime = 108.8f; // 1분 48.8초
    public float patternStartTime2 = 121.6f; // 2분 1.6초 (두 번째 구간)
    public float patternEndTime2 = 134.4f; // 2분 14.4초 (두 번째 구간)
    public float beatInterval = 0.4f; // BPM 150 기준 (60/150)
    
    [Header("대상 오브젝트")]
    public Transform mouseCursor; // 보스 마우스 커서
    public Camera mainCamera;
    public Transform gameWindow; // 게임 창
    
    [Header("드래그 영역 설정")]
    public GameObject dragAreaPrefab; // 드래그 영역 프리팹 (빨간색 반투명 사각형)
    public Sprite explosionSprite; // 터질 때 드래그 영역 이미지
    public Color dragAreaColor = new Color(1f, 0f, 0f, 0.5f); // 빨간색 반투명
    public GameObject explosionEffectPrefab; // 폭발 이펙트 프리팹 (사진처럼)
    public GameObject background; // 배경 오브젝트
    
    private bool patternStarted = false;
    private Vector3 gameWindowCenter;
    private Bounds gameWindowBounds;
    private Color originalBackgroundColor;
    private Vector3 originalCameraPosition; // 카메라 원래 위치 저장

    [Tooltip("에러 사운드")]
    public AudioClip Error;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        // ⚠️ 카메라 원래 위치 저장
        if (mainCamera != null)
        {
            originalCameraPosition = mainCamera.transform.position;
            Debug.Log($"[BossDragPattern] 카메라 원래 위치 저장: {originalCameraPosition}");
        }
        
        // 게임 창 정보 가져오기
        if (gameWindow != null)
        {
            gameWindowBounds = GetWindowBounds();
            gameWindowCenter = gameWindowBounds.center;
            Debug.Log($"[BossDragPattern] 게임 창 중심: {gameWindowCenter}, 크기: {gameWindowBounds.size}");
        }
        

        
        // 배경 원래 색상 저장
        if (background != null)
        {
            SpriteRenderer bgSr = background.GetComponent<SpriteRenderer>();
            if (bgSr != null)
            {
                originalBackgroundColor = bgSr.color;
                Debug.Log($"[BossDragPattern] 배경 오브젝트: {background.name}, 원래 색상: {originalBackgroundColor}");
            }
            else
            {
                Debug.LogError($"[BossDragPattern] {background.name}에 SpriteRenderer가 없습니다!");
            }
        }
        else
        {
            Debug.LogError("[BossDragPattern] Background 오브젝트가 할당되지 않았습니다!");
        }
    }
    
    void Update()
    {
        if (BeatBounce.Instance == null) return;
        
        // ⚠️ 패턴 실행 중에는 카메라 위치 강제 고정
        if (patternStarted && mainCamera != null)
        {
            mainCamera.transform.position = originalCameraPosition;
        }
        
        double musicTime = BeatBounce.Instance.GetMusicTime();
        
        // 1분 36초에 패턴 시작 (첫 번째 구간)
        if (!patternStarted && musicTime >= patternStartTime && musicTime < patternEndTime)
        {
            patternStarted = true;
            Debug.Log($"[BossDragPattern] 패턴 시작 (1차)! musicTime: {musicTime}");
            StartCoroutine(DragPatternSequence());
        }
        
        // 2분 1초에 패턴 다시 시작 (두 번째 구간)
        if (!patternStarted && musicTime >= patternStartTime2 && musicTime < patternEndTime2)
        {
            patternStarted = true;
            Debug.Log($"[BossDragPattern] 패턴 시작 (2차)! musicTime: {musicTime}");
            StartCoroutine(DragPatternSequence2());
        }
        
        // 패턴 종료 (첫 번째 구간)
        if (patternStarted && musicTime >= patternEndTime && musicTime < patternStartTime2)
        {
            patternStarted = false;
            Debug.Log("[BossDragPattern] 패턴 종료 (1차)");
        }
        
        // 패턴 종료 (두 번째 구간)
        if (patternStarted && musicTime >= patternEndTime2)
        {
            patternStarted = false;
            Debug.Log("[BossDragPattern] 패턴 종료 (2차)");
        }
    }
    
    IEnumerator DragPatternSequence()
    {
        // 96초부터 108.8초까지 7번 반복 (28박자 = 11.2초)
        // ⚠️ 2배 느리게: 드래그 2박자 + 폭발/쉬기 2박자 = 총 4박자
        for (int i = 0; i < 7; i++)
        {
            yield return StartCoroutine(CreateDragArea());
        }
        
        Debug.Log("[BossDragPattern] 시퀀스 완료 (1차): 7번 반복, 28박자, 11.2초");
    }
    
    IEnumerator DragPatternSequence2()
    {
        // 121.6초부터 134.4초까지 7번 반복 (28박자 = 11.2초)
        // ⚠️ 2배 느리게: 드래그 2박자 + 폭발/쉬기 2박자 = 총 4박자
        for (int i = 0; i < 7; i++)
        {
            yield return StartCoroutine(CreateDragArea());
        }
        
        Debug.Log("[BossDragPattern] 시퀀스 완료 (2차): 7번 반복, 28박자, 11.2초");
    }
    
    IEnumerator CreateDragArea()
    {
        if (mouseCursor == null || gameWindow == null)
        {
            Debug.LogWarning("[BossDragPattern] 마우스 커서 또는 게임 창이 없습니다!");
            yield break;
        }
        
        // 게임 창 안의 랜덤 시작점
        Vector3 startPos = GetRandomPositionInWindow();
        
        // 게임 창 안의 랜덤 끝점
        Vector3 endPos = GetRandomPositionInWindow();
        
        Debug.Log($"[BossDragPattern] 드래그: {startPos} → {endPos}");
        
        // 1단계: 2박자 동안 드래그 (2배 느리게)
        float dragDuration = beatInterval * 2f; // 2박자 = 0.8초
        GameObject dragArea = null;
        
        yield return StartCoroutine(DragToPosition(startPos, endPos, dragDuration, (area) => dragArea = area));
        
        // 2단계: 2박자 동안 터짐 (폭발 + 쉬기) (2배 느리게)
        if (dragArea != null)
        {
            yield return StartCoroutine(ExplodeDragArea(dragArea, dragDuration));
        }
    }
    
    IEnumerator DragToPosition(Vector3 startPos, Vector3 endPos, float duration, System.Action<GameObject> onAreaCreated)
    {
        if (mouseCursor == null) yield break;
        
        // 프리팹 원본 크기 가져오기
        Vector3 originalScale = dragAreaPrefab != null ? dragAreaPrefab.transform.localScale : new Vector3(2f, 2f, 1f);
        
        Debug.Log($"[BossDragPattern] 프리팹 원본 스케일: {originalScale}");
        
        // 보스 시작 위치: 게임창 안의 랜덤 위치
        Vector3 bossStartPos = GetRandomPositionInWindow();
        bossStartPos.z = 0f; // ⚠️ Z값 고정 (누적 방지)
        
        Debug.Log($"[BossDragPattern] 보스 시작 위치(월드): {bossStartPos}");
        
        mouseCursor.position = bossStartPos;
        
        // 드래그 영역 생성 위치: 보스가 왼쪽 위 끝에 오도록 계산
        // 프리팹 중심 = 보스위치 + (원본너비/2, -원본높이/2)
        Vector3 dragAreaCenter = bossStartPos + new Vector3(originalScale.x / 2f, -originalScale.y / 2f, 0f);
        
        Debug.Log($"[BossDragPattern] 드래그 영역 중심: {dragAreaCenter}");
        
        GameObject dragArea = null;
        if (dragAreaPrefab != null)
        {
            dragArea = Instantiate(dragAreaPrefab, dragAreaCenter, Quaternion.identity);
        }
        else
        {
            dragArea = CreateDefaultDragArea(dragAreaCenter);
        }
        
        onAreaCreated?.Invoke(dragArea);
        
        // 프리팹을 0 크기로 시작
        dragArea.transform.localScale = Vector3.zero;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 프리팹이 점점 커짐 (0 → 원본 크기, 오른쪽 아래로만 확장)
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            dragArea.transform.localScale = originalScale * smoothT;
            
            // 프리팹 중심 업데이트: 보스 위치를 기준으로 오른쪽 아래로 이동
            Vector3 currentScale = dragArea.transform.localScale;
            Vector3 newCenter = bossStartPos + new Vector3(currentScale.x / 2f, -currentScale.y / 2f, 0f);
            dragArea.transform.position = newCenter;
            
            // ⚠️ 보스도 같이 이동: 프리팹 크기의 3.3배 거리만큼 대각선 이동
            // 왼쪽 위 → 오른쪽 아래 (X: 3.3배, Y: 3.6배)
            Vector3 bossOffset = new Vector3(currentScale.x * 3.3f * smoothT, -currentScale.y * 3.6f * smoothT, 0f);
            Vector3 newBossPos = bossStartPos + bossOffset;
            newBossPos.z = 0f; // ⚠️ Z값 고정 (누적 방지)
            mouseCursor.position = newBossPos;
            
            yield return null;
        }
        
        // 최종 크기
        dragArea.transform.localScale = originalScale;
        Vector3 finalCenter = bossStartPos + new Vector3(originalScale.x / 2f, -originalScale.y / 2f, 0f);
        dragArea.transform.position = finalCenter;
        
        // 최종 보스 위치: X 3.3배, Y 3.6배 (더 아래로)
        Vector3 finalBossOffset = new Vector3(originalScale.x * 3.3f, -originalScale.y * 3.6f, 0f);
        Vector3 finalBossPos = bossStartPos + finalBossOffset;
        finalBossPos.z = 0f; // ⚠️ Z값 고정 (누적 방지)
        mouseCursor.position = finalBossPos;
        
        Debug.Log($"[BossDragPattern] 드래그 완료: 왼쪽 위 → 오른쪽 아래");
    }
    
    Vector3 GetTopLeftOfPrefab(GameObject prefab, Vector3 center)
    {
        if (prefab == null) return center;
        
        Vector3 scale = prefab.transform.localScale;
        // ⚠️ 프리팹의 정확한 왼쪽 위 끝으로 이동 (진짜 드래그처럼)
        Vector3 topLeft = center + new Vector3(-scale.x / 2f, scale.y / 2f, 0f);
        return topLeft;
    }
    
    Vector3 GetBottomRightOfPrefab(GameObject prefab, Vector3 center)
    {
        if (prefab == null) return center;
        
        Vector3 scale = prefab.transform.localScale;
        // ⚠️ 프리팹의 정확한 오른쪽 아래 끝으로 이동 (진짜 드래그처럼)
        Vector3 bottomRight = center + new Vector3(scale.x / 2f, -scale.y / 2f, 0f);
        return bottomRight;
    }
    
    IEnumerator ExplodeDragArea(GameObject dragArea, float duration)
    {
        if (dragArea == null) yield break;
        
        Vector3 explosionCenter = dragArea.transform.position;
        
        Debug.Log($"[BossDragPattern] 폭발 시작! 위치: {explosionCenter}");

        SoundManager.Instance.PlaySFX(Error);

        // 1. 드래그 영역 이미지를 폭발 이미지로 변경
        if (explosionSprite != null)
        {
            SpriteRenderer dragSr = dragArea.GetComponent<SpriteRenderer>();
            if (dragSr != null)
            {
                dragSr.sprite = explosionSprite;
                Debug.Log("[BossDragPattern] 드래그 영역 이미지를 폭발 이미지로 변경");
            }
        }
        
        // 2. 배경을 빨갛게
        StartCoroutine(FlashBackground(Color.red, duration));
        
        // 3. 랜덤 정사각형들 따다닥 소환 (30~50개)
        int squareCount = Random.Range(30, 50);
        for (int i = 0; i < squareCount; i++)
        {
            SpawnRandomSquare(explosionCenter);
            
            // 10개씩 소환하고 약간 대기 (따다닥 효과)
            if (i % 10 == 9)
            {
                yield return new WaitForSeconds(0.02f);
            }
        }
        
        Debug.Log($"[BossDragPattern] {squareCount}개 사각형 소환 완료");
        
        // 4. 나머지 duration 대기
        yield return new WaitForSeconds(duration - 0.06f);
        
        // 5. 드래그 영역 삭제
        Destroy(dragArea);
        
        Debug.Log("[BossDragPattern] 폭발 완료, 원래대로 복구");
    }
    
    IEnumerator FlashBackground(Color flashColor, float duration)
    {
        Debug.Log("[BossDragPattern] ========== FlashBackground 시작 ==========");
        
        if (background == null)
        {
            Debug.LogError("[BossDragPattern] background가 null입니다!");
            yield break;
        }
        
        SpriteRenderer bgSr = background.GetComponent<SpriteRenderer>();
        if (bgSr == null)
        {
            Debug.LogError($"[BossDragPattern] {background.name}에 SpriteRenderer가 없습니다!");
            yield break;
        }
        
        // 배경을 빨간색으로 변경하고 order를 0으로 올림
        Color redColor = new Color(1f, 0f, 0f, 1f);
        bgSr.color = redColor;
        bgSr.sortingOrder = 0;
        Debug.Log($"[BossDragPattern] {background.name} 색상 빨간색 + sortingOrder 0으로 변경");
        
        // duration 동안 대기
        yield return new WaitForSeconds(duration);
        
        // sortingOrder를 -5로 내려서 숨김
        bgSr.sortingOrder = -5;
        Debug.Log($"[BossDragPattern] {background.name} sortingOrder -5로 변경 (숨김)");
    }
    
    void SpawnRandomSquare(Vector3 center)
    {
        GameObject square = new GameObject("ExplosionSquare");
        
        // 랜덤 위치 (게임창 전체에 퍼뜨리기)
        Bounds bounds = gameWindowBounds;
        Vector3 randomPos = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            -5f // Z값을 -5로 (맨 앞에 표시)
        );
        square.transform.position = randomPos;
        
        // SpriteRenderer 추가
        SpriteRenderer sr = square.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        
        // 정확한 색상 3가지 (흰색, 밝은 회색, 어두운 회색)
        Color[] colors = new Color[]
        {
            Color.white,                        // 하양
            new Color(0.8f, 0.8f, 0.8f),       // 밝은 회색
            new Color(0.4f, 0.4f, 0.4f)        // 어두운 회색
        };
        sr.color = colors[Random.Range(0, colors.Length)];
        sr.sortingOrder = 0; // Order in Layer 0
        
        // 크기 100배 크게 (200.0 ~ 600.0)
        float randomSize = Random.Range(200.0f, 600.0f);
        square.transform.localScale = Vector3.one * randomSize;
        
        // 4분의 2박자 후 자동 삭제
        Destroy(square, beatInterval / 2f);
    }
    
    GameObject CreateDefaultDragArea(Vector3 position)
    {
        GameObject area = new GameObject("DragArea");
        area.transform.position = position;
        
        SpriteRenderer sr = area.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = dragAreaColor;
        sr.sortingOrder = 1;
        
        return area;
    }
    
    
    Sprite CreateSquareSprite()
    {
        // 1x1 흰색 텍스처 생성
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        
        // 스프라이트 생성
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
    }
    
    Vector3 GetRandomPositionInWindow()
    {
        if (gameWindow == null) return Vector3.zero;
        
        Bounds bounds = gameWindowBounds;
        
        // 게임 창 안의 랜덤 위치 (마진 추가로 완전히 안에 있도록)
        float margin = 0.5f;
        float randomX = Random.Range(bounds.min.x + margin, bounds.max.x - margin);
        float randomY = Random.Range(bounds.min.y + margin, bounds.max.y - margin);
        
        // ⚠️ 평행이동 오프셋 적용: (-3.5, -0.2) → (-4.45, 0.95)
        // X: -0.95, Y: +1.15
        Vector3 offset = new Vector3(-0.95f, 1.15f, 0f);
        
        return new Vector3(randomX, randomY, 0f) + offset;
    }
    
    Bounds GetWindowBounds()
    {
        if (gameWindow == null) return new Bounds(Vector3.zero, new Vector3(6f, 4f, 0f));
        
        // Collider2D가 있으면 사용
        Collider2D col = gameWindow.GetComponent<Collider2D>();
        if (col != null)
        {
            return col.bounds;
        }
        
        // RectTransform인 경우 (UI)
        RectTransform rect = gameWindow.GetComponent<RectTransform>();
        if (rect != null)
        {
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Bounds bounds = new Bounds(corners[0], Vector3.zero);
            foreach (Vector3 corner in corners)
            {
                bounds.Encapsulate(corner);
            }
            return bounds;
        }
        
        // 기본값
        return new Bounds(Vector3.zero, new Vector3(6f, 4f, 0f));
    }
}
