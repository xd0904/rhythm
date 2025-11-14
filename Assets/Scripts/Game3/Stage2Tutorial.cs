using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Stage2Tutorial : MonoBehaviour
{
    [Header("비활성화할 아이콘")]
    public GameObject icon1; // 첫 번째 아이콘
    public GameObject icon2; // 두 번째 아이콘
    
    [Header("X 버튼 설정")]
    public Button xButton; // X 버튼
    public GameObject clickHereText; // "Click Here" 텍스트
    public GameObject arrow; // 화살표
    public Canvas targetCanvas; // 비활성화할 Canvas
    public float canvasDisableDuration = 0.2f; // Canvas 비활성화 시간
    
    [Header("Drug 오브젝트")]
    public GameObject drug; // 0.2초 후 활성화할 Drug 오브젝트
    public float collectDistance = 0.5f; // Player와 Drug 사이 수집 거리
    
    [Header("Tutorial 설정")]
    public GameObject tutorial; // 10초 후 비활성화할 Tutorial GameObject
    public float tutorialDuration = 10f; // Tutorial 표시 시간 (초)
    
    private Transform playerTransform; // Player Transform
    
    void Start()
    {
        Debug.Log("[Stage2Tutorial] Start() 호출됨");
        
        // Player 찾기
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            Debug.Log($"[Stage2Tutorial] Player 찾음: {playerObj.name}");
        }
        else
        {
            Debug.LogWarning("[Stage2Tutorial] Player를 찾을 수 없습니다!");
        }
        
        // EventSystem 확인 (UI 클릭 이벤트에 필수!)
        UnityEngine.EventSystems.EventSystem eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogError("[Stage2Tutorial] ★★★ EventSystem이 없습니다! UI 클릭이 작동하지 않습니다! ★★★");
        }
        else
        {
            Debug.Log($"[Stage2Tutorial] EventSystem 찾음: {eventSystem.name}");
        }
        
        // 시작 시 아이콘 2개 비활성화
        if (icon1 != null)
        {
            icon1.SetActive(false);
            Debug.Log("[Stage2Tutorial] Icon1 비활성화");
        }
        
        if (icon2 != null)
        {
            icon2.SetActive(false);
            Debug.Log("[Stage2Tutorial] Icon2 비활성화");
        }
        
        // Drug 오브젝트 초기 비활성화
        if (drug != null)
        {
            drug.SetActive(false);
            Debug.Log("[Stage2Tutorial] Drug 초기 비활성화");
        }
        
        // X 버튼 클릭 이벤트 등록
        if (xButton != null)
        {
            Debug.Log($"[Stage2Tutorial] X 버튼 찾음: {xButton.name}");
            Debug.Log($"[Stage2Tutorial] X 버튼 Interactable: {xButton.interactable}");
            Debug.Log($"[Stage2Tutorial] X 버튼 GameObject 활성: {xButton.gameObject.activeInHierarchy}");
            
            // Button이 Interactable인지 확인
            if (!xButton.interactable)
            {
                Debug.LogWarning("[Stage2Tutorial] X 버튼이 Interactable=false입니다! true로 설정합니다.");
                xButton.interactable = true;
            }
            
            xButton.onClick.AddListener(OnXButtonClick);
            Debug.Log("[Stage2Tutorial] X 버튼 클릭 이벤트 등록 완료");
            
            // X 버튼 자체에 Image가 있다면 확인
            Image xButtonImage = xButton.GetComponent<Image>();
            if (xButtonImage != null)
            {
                xButtonImage.raycastTarget = true;
                Debug.Log($"[Stage2Tutorial] X 버튼 자체 Image Raycast Target: {xButtonImage.raycastTarget}");
            }
            
            // X 버튼의 자식 Image들도 모두 raycastTarget 활성화
            Image[] childImages = xButton.GetComponentsInChildren<Image>();
            foreach (Image img in childImages)
            {
                img.raycastTarget = true;
                Debug.Log($"[Stage2Tutorial] {img.gameObject.name} Image Raycast Target 활성화");
            }
            
            // X 버튼의 부모 Canvas 찾기 (먼저)
            Canvas parentCanvas = xButton.GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                Debug.Log($"[Stage2Tutorial] X 버튼의 부모 Canvas: {parentCanvas.name}, sortingOrder: {parentCanvas.sortingOrder}");
                
                // 부모 Canvas에 GraphicRaycaster가 있는지 확인
                GraphicRaycaster raycaster = parentCanvas.GetComponent<GraphicRaycaster>();
                if (raycaster == null)
                {
                    Debug.LogWarning("[Stage2Tutorial] 부모 Canvas에 GraphicRaycaster가 없습니다! 추가합니다.");
                    raycaster = parentCanvas.gameObject.AddComponent<GraphicRaycaster>();
                }
                raycaster.enabled = true; // 강제로 활성화
                Debug.Log($"[Stage2Tutorial] GraphicRaycaster 강제 활성화: enabled={raycaster.enabled}");
                
                // 부모 Canvas의 sortingOrder를 높게 설정
                parentCanvas.overrideSorting = true;
                parentCanvas.sortingOrder = 5;
                Debug.Log("[Stage2Tutorial] 부모 Canvas Sorting Order 설정: 5");
            }
            else
            {
                Debug.LogError("[Stage2Tutorial] X 버튼의 부모 Canvas를 찾을 수 없습니다!");
            }
        }
        else
        {
            Debug.LogWarning("[Stage2Tutorial] X 버튼이 할당되지 않았습니다!");
        }
        
        // 10초 후 Tutorial 비활성화
        if (tutorial != null)
        {
            StartCoroutine(DisableTutorialAfterDelay());
        }
    }
    
    IEnumerator DisableTutorialAfterDelay()
    {
        Debug.Log($"[Stage2Tutorial] {tutorialDuration}초 후 Tutorial 비활성화 예약 시작");
        
        float elapsedTime = 0f;
        while (elapsedTime < tutorialDuration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        if (tutorial != null && tutorial.activeSelf)
        {
            tutorial.SetActive(false);
            Debug.Log($"[Stage2Tutorial] {tutorialDuration}초 경과 - Tutorial SetActive(false) 완료");
        }
        else if (tutorial == null)
        {
            Debug.LogWarning("[Stage2Tutorial] Tutorial GameObject가 null입니다!");
        }
        else
        {
            Debug.Log("[Stage2Tutorial] Tutorial이 이미 비활성화 상태입니다.");
        }
    }
    
    void OnXButtonClick()
    {
        Debug.Log("========================================");
        Debug.Log("[Stage2Tutorial] ★★★ X 버튼 클릭됨! ★★★");
        Debug.Log("========================================");
        
        // Canvas enabled만 비활성화 후 재활성화 코루틴 시작
        if (targetCanvas != null)
        {
            Debug.Log($"[Stage2Tutorial] 코루틴 시작 전 - Canvas: {targetCanvas.name}, enabled: {targetCanvas.enabled}");
            StartCoroutine(DisableCanvasTemporarily());
        }
        else
        {
            Debug.LogError("[Stage2Tutorial] targetCanvas가 null입니다!");
        }
    }
    
    IEnumerator DisableCanvasTemporarily()
    {
        // Canvas GameObject SetActive(false)
        targetCanvas.gameObject.SetActive(false);
        Debug.Log("[Stage2Tutorial] Canvas SetActive(false)");
        
        // 0.2초 대기 (실시간 대기)
        yield return new WaitForSecondsRealtime(canvasDisableDuration);
        
        // Canvas GameObject SetActive(true)
        targetCanvas.gameObject.SetActive(true);
        Debug.Log("[Stage2Tutorial] Canvas SetActive(true)");
        
        // X 버튼 비활성화
        if (xButton != null)
        {
            xButton.gameObject.SetActive(false);
            Debug.Log("[Stage2Tutorial] X 버튼 SetActive(false)");
        }
        
        // Click Here 텍스트 비활성화
        if (clickHereText != null)
        {
            clickHereText.SetActive(false);
            Debug.Log("[Stage2Tutorial] Click Here 텍스트 SetActive(false)");
        }
        
        // 화살표 비활성화
        if (arrow != null)
        {
            arrow.SetActive(false);
            Debug.Log("[Stage2Tutorial] 화살표 SetActive(false)");
        }
        
        // Drug 오브젝트 활성화
        if (drug != null)
        {
            drug.SetActive(true);
            Debug.Log("[Stage2Tutorial] Drug SetActive(true)");
        }
    }

    void Update()
    {
        // X 버튼 마우스 클릭 직접 감지 (Button 컴포넌트가 작동 안 할 때를 위한 백업)
        if (xButton != null && xButton.gameObject.activeSelf && Input.GetMouseButtonDown(0))
        {
            // 마우스 위치가 X 버튼 영역 안에 있는지 확인
            RectTransform xButtonRect = xButton.GetComponent<RectTransform>();
            Canvas canvas = xButton.GetComponentInParent<Canvas>();
            Camera cam = null;
            
            // Canvas의 Render Mode에 따라 Camera 설정
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                cam = canvas.worldCamera;
                if (cam == null) cam = Camera.main;
            }
            
            if (xButtonRect != null && RectTransformUtility.RectangleContainsScreenPoint(xButtonRect, Input.mousePosition, cam))
            {
                Debug.Log("[Stage2Tutorial] ★★★ Update에서 X 버튼 클릭 감지! ★★★");
                OnXButtonClick();
            }
        }
        
        // Drug가 활성화되어 있고 Player와 충돌 체크
        if (drug != null && drug.activeSelf && playerTransform != null)
        {
            float distance = Vector3.Distance(playerTransform.position, drug.transform.position);
            
            if (distance <= collectDistance)
            {
                Debug.Log($"[Stage2Tutorial] Player가 Drug 획득! 거리: {distance:F2}");
                drug.SetActive(false);
                Debug.Log("[Stage2Tutorial] Drug SetActive(false)");
            }
        }
    }
}
