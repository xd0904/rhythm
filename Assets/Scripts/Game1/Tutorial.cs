using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    [Header("플레이어 설정")]
    public Transform player; // 플레이어 Transform
    
    [Header("WASD 텍스트 설정")]
    public RectTransform textW; // 북쪽 (위)
    public RectTransform textA; // 서쪽 (왼쪽)
    public RectTransform textS; // 남쪽 (아래)
    public RectTransform textD; // 동쪽 (오른쪽)
    public RectTransform textSpace; // SPACE (S의 아래쪽, 2배 거리)
    
    [Header("Canvas 설정")]
    public Canvas canvas; // Canvas (Screen Space - Camera 모드 필수)
    
    [Header("텍스트 거리 설정")]
    public float textDistance = 100f; // 플레이어로부터 텍스트까지의 거리 (UI 스크린 픽셀 단위)
    
    [Header("텍스트 색상 설정")]
    public Color normalColor = new Color(0.251f, 0.251f, 0.251f, 1f); // #404040
    public Color pressedColor = new Color(0.784f, 0.784f, 0.784f, 1f); // #C8C8C8
    
    [Header("타이밍 설정")]
    public float tutorialStartTime = 0f; // 튜토리얼 시작 시간
    public float tutorialEndTime = 6.3f; // 튜토리얼 종료 시간
    
    private Camera mainCamera;
    private Text textWComponent;
    private Text textAComponent;
    private Text textSComponent;
    private Text textDComponent;
    private Text textSpaceComponent;
    private bool tutorialEnded = false;
    
    void Start()
    {
        Debug.Log("[Tutorial] Start() 호출됨");
        
        mainCamera = Camera.main;
        Debug.Log($"[Tutorial] Main Camera: {(mainCamera != null ? mainCamera.name : "NULL")}");
        
        // Canvas 자동 찾기
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindFirstObjectByType<Canvas>();
            }
        }
        
        Debug.Log($"[Tutorial] Canvas: {(canvas != null ? canvas.name : "NULL")}");
        
        // Canvas가 Screen Space - Camera 모드인지 확인
        if (canvas != null)
        {
            Debug.Log($"[Tutorial] Canvas Render Mode: {canvas.renderMode}");
            
            if (canvas.renderMode != RenderMode.ScreenSpaceCamera)
            {
                Debug.LogWarning("[Tutorial] Canvas의 Render Mode를 'Screen Space - Camera'로 설정해주세요!");
            }
            
            // Canvas의 카메라가 설정되어 있는지 확인
            if (canvas.worldCamera == null)
            {
                canvas.worldCamera = mainCamera;
                Debug.Log("[Tutorial] Canvas에 Main Camera를 자동으로 할당했습니다.");
            }
            else
            {
                Debug.Log($"[Tutorial] Canvas World Camera: {canvas.worldCamera.name}");
            }
        }
        
        // 플레이어 자동 찾기
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log($"[Tutorial] Player 자동 찾기 완료: {player.name}");
            }
            else
            {
                Debug.LogWarning("[Tutorial] Player를 찾을 수 없습니다!");
            }
        }
        else
        {
            Debug.Log($"[Tutorial] Player 설정됨: {player.name}");
        }
        
        // 텍스트 오브젝트 확인 및 Sorting Order 설정
        SetTextSortingOrder(textW, "W");
        SetTextSortingOrder(textA, "A");
        SetTextSortingOrder(textS, "S");
        SetTextSortingOrder(textD, "D");
        SetTextSortingOrder(textSpace, "SPACE");
        
        // Text 컴포넌트 가져오기
        if (textW != null) textWComponent = textW.GetComponent<Text>();
        if (textA != null) textAComponent = textA.GetComponent<Text>();
        if (textS != null) textSComponent = textS.GetComponent<Text>();
        if (textD != null) textDComponent = textD.GetComponent<Text>();
        if (textSpace != null) textSpaceComponent = textSpace.GetComponent<Text>();
        
        // 초기 색상 설정
        if (textWComponent != null) 
        {
            textWComponent.color = normalColor;
            Debug.Log($"[Tutorial] W 색상 설정: {normalColor}");
        }
        if (textAComponent != null) 
        {
            textAComponent.color = normalColor;
            Debug.Log($"[Tutorial] A 색상 설정: {normalColor}");
        }
        if (textSComponent != null) 
        {
            textSComponent.color = normalColor;
            Debug.Log($"[Tutorial] S 색상 설정: {normalColor}");
        }
        if (textDComponent != null) 
        {
            textDComponent.color = normalColor;
            Debug.Log($"[Tutorial] D 색상 설정: {normalColor}");
        }
        if (textSpaceComponent != null) 
        {
            textSpaceComponent.color = normalColor;
            Debug.Log($"[Tutorial] SPACE 색상 설정: {normalColor}");
        }
    }
    
    void SetTextSortingOrder(RectTransform textRect, string name)
    {
        if (textRect != null)
        {
            Debug.Log($"[Tutorial] Text {name}: {textRect.name}");
            
            // Canvas가 있으면 Sorting Order 설정
            Canvas textCanvas = textRect.GetComponent<Canvas>();
            if (textCanvas == null)
            {
                textCanvas = textRect.gameObject.AddComponent<Canvas>();
            }
            
            textCanvas.overrideSorting = true;
            textCanvas.sortingOrder = 1000; // 매우 높은 값으로 설정하여 모든 것 위에 표시
            
            Debug.Log($"[Tutorial] Text {name} Sorting Order 설정: 1000");
        }
        else
        {
            Debug.Log($"[Tutorial] Text {name}: NULL");
        }
    }

    void Update()
    {
        if (BeatBounce.Instance == null) return;
        
        double musicTime = BeatBounce.Instance.GetMusicTime();
        
        // 6.3초 이후에 Tutorial 비활성화
        if (!tutorialEnded && musicTime >= tutorialEndTime)
        {
            tutorialEnded = true;
            gameObject.SetActive(false);
            Debug.Log("[Tutorial] 튜토리얼 종료: Tutorial 비활성화");
            return;
        }
        
        // 0초~6.3초 사이가 아니면 업데이트하지 않음
        if (musicTime < tutorialStartTime || musicTime >= tutorialEndTime)
        {
            return;
        }
        
        if (player == null || mainCamera == null || canvas == null) return;
        
        // 키 입력에 따른 색상 변경
        UpdateTextColor(textWComponent, Input.GetKey(KeyCode.W));
        UpdateTextColor(textAComponent, Input.GetKey(KeyCode.A));
        UpdateTextColor(textSComponent, Input.GetKey(KeyCode.S));
        UpdateTextColor(textDComponent, Input.GetKey(KeyCode.D));
        UpdateTextColor(textSpaceComponent, Input.GetKey(KeyCode.Space));
        
        // 플레이어의 월드 좌표를 스크린 좌표로 변환
        Vector3 playerWorldPos = player.position;
        Vector2 playerScreenPos = RectTransformUtility.WorldToScreenPoint(mainCamera, playerWorldPos);
        
        // Canvas의 RectTransform
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        
        // 각 텍스트를 플레이어 주변에 배치
        if (textW != null)
        {
            // 북쪽 (위)
            Vector2 screenPos = playerScreenPos + Vector2.up * textDistance;
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, canvas.worldCamera, out localPos);
            textW.anchoredPosition = localPos;
        }
        
        if (textA != null)
        {
            // 서쪽 (왼쪽)
            Vector2 screenPos = playerScreenPos + Vector2.left * textDistance;
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, canvas.worldCamera, out localPos);
            textA.anchoredPosition = localPos;
        }
        
        if (textS != null)
        {
            // 남쪽 (아래)
            Vector2 screenPos = playerScreenPos + Vector2.down * textDistance;
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, canvas.worldCamera, out localPos);
            textS.anchoredPosition = localPos;
        }
        
        if (textD != null)
        {
            // 동쪽 (오른쪽)
            Vector2 screenPos = playerScreenPos + Vector2.right * textDistance;
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, canvas.worldCamera, out localPos);
            textD.anchoredPosition = localPos;
        }
        
        if (textSpace != null)
        {
            // SPACE (S의 아래쪽, 2배 거리)
            Vector2 screenPos = playerScreenPos + Vector2.down * textDistance * 2f;
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, canvas.worldCamera, out localPos);
            textSpace.anchoredPosition = localPos;
        }
    }
    
    void UpdateTextColor(Text textComponent, bool isPressed)
    {
        if (textComponent != null)
        {
            textComponent.color = isPressed ? pressedColor : normalColor;
        }
    }
}
