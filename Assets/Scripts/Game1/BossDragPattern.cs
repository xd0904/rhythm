using UnityEngine;
using System.Collections;

public class BossDragPattern : MonoBehaviour
{
    [Header("타이밍 설정")]
    public float patternStartTime = 96f; // 1분 36초
    public float patternEndTime = 108f; // 1분 48초
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
    
    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
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
        
        double musicTime = BeatBounce.Instance.GetMusicTime();
        
        // 1분 36초에 패턴 시작
        if (!patternStarted && musicTime >= patternStartTime && musicTime < patternEndTime)
        {
            patternStarted = true;
            Debug.Log($"[BossDragPattern] 패턴 시작! musicTime: {musicTime}");
            StartCoroutine(DragPatternSequence());
        }
        
        // 패턴 종료
        if (patternStarted && musicTime >= patternEndTime)
        {
            patternStarted = false;
            Debug.Log("[BossDragPattern] 패턴 종료");
        }
    }
    
    IEnumerator DragPatternSequence()
    {
        // 따 따따따 따따 패턴 (6번)
        // 각 "따"마다 드래그 → 터짐
        
        for (int i = 0; i < 6; i++)
        {
            yield return StartCoroutine(CreateDragArea());
            
            // 다음 "따"까지 대기 (1박자)
            if (i < 5) // 마지막 공격 후에는 대기 안 함
            {
                yield return new WaitForSeconds(beatInterval);
            }
        }
        
        Debug.Log("[BossDragPattern] 시퀀스 완료");
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
        
        // 1단계: 4분의 2박자(0.2초) 동안 드래그
        float dragDuration = beatInterval / 2f; // 2/4박자 = 0.2초
        GameObject dragArea = null;
        
        yield return StartCoroutine(DragToPosition(startPos, endPos, dragDuration, (area) => dragArea = area));
        
        // 2단계: 4분의 2박자(0.2초) 동안 터짐
        if (dragArea != null)
        {
            yield return StartCoroutine(ExplodeDragArea(dragArea, dragDuration));
        }
    }
    
    IEnumerator DragToPosition(Vector3 startPos, Vector3 endPos, float duration, System.Action<GameObject> onAreaCreated)
    {
        if (mouseCursor == null) yield break;
        
        // 드래그 영역 생성 (프리팹 원본 크기로 시작, 왼쪽 위에 배치)
        GameObject dragArea = null;
        if (dragAreaPrefab != null)
        {
            dragArea = Instantiate(dragAreaPrefab, startPos, Quaternion.identity);
        }
        else
        {
            dragArea = CreateDefaultDragArea(startPos);
        }
        
        onAreaCreated?.Invoke(dragArea);
        
        // 프리팹 원본 크기 저장
        Vector3 originalScale = dragArea.transform.localScale;
        
        // 보스를 프리팹의 왼쪽 위로 이동 (시작점)
        Vector3 topLeft = GetTopLeftOfPrefab(dragArea, startPos);
        topLeft.z = mouseCursor.position.z;
        mouseCursor.position = topLeft;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 프리팹이 점점 커짐 (1배 → t배)
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            dragArea.transform.localScale = originalScale * smoothT;
            
            // 마우스는 프리팹의 오른쪽 아래로 이동
            Vector3 bottomRight = GetBottomRightOfPrefab(dragArea, startPos);
            bottomRight.z = mouseCursor.position.z;
            mouseCursor.position = bottomRight;
            
            yield return null;
        }
        
        // 최종 크기
        dragArea.transform.localScale = originalScale;
        
        // 마우스를 프리팹의 오른쪽 아래로
        Vector3 finalBottomRight = GetBottomRightOfPrefab(dragArea, startPos);
        finalBottomRight.z = mouseCursor.position.z;
        mouseCursor.position = finalBottomRight;
        
        Debug.Log($"[BossDragPattern] 드래그 완료: 왼쪽 위 → 오른쪽 아래");
    }
    
    Vector3 GetTopLeftOfPrefab(GameObject prefab, Vector3 center)
    {
        if (prefab == null) return center;
        
        Vector3 scale = prefab.transform.localScale;
        Vector3 topLeft = center + new Vector3(-scale.x / 2f, scale.y / 2f, 0f);
        return topLeft;
    }
    
    Vector3 GetBottomRightOfPrefab(GameObject prefab, Vector3 center)
    {
        if (prefab == null) return center;
        
        Vector3 scale = prefab.transform.localScale;
        Vector3 bottomRight = center + new Vector3(scale.x / 2f, -scale.y / 2f, 0f);
        return bottomRight;
    }
    
    IEnumerator ExplodeDragArea(GameObject dragArea, float duration)
    {
        if (dragArea == null) yield break;
        
        Vector3 explosionCenter = dragArea.transform.position;
        
        Debug.Log($"[BossDragPattern] 폭발 시작! 위치: {explosionCenter}");
        
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
        
        return new Vector3(randomX, randomY, 0f);
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
