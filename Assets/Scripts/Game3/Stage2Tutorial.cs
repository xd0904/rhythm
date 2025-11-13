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
        
        // EventSystem 확인
        UnityEngine.EventSystems.EventSystem eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogError("[Stage2Tutorial] ★★★ EventSystem이 없습니다! UI 클릭이 작동하지 않습니다! ★★★");
        }
        else
        {
            Debug.Log($"[Stage2Tutorial] EventSystem 찾음: {eventSystem.name}");
        }
        
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
            
            xButton.onClick.AddListener(OnXButtonClick);
            Debug.Log("[Stage2Tutorial] X 버튼 onClick 리스너 등록 완료");
            
            // X 버튼의 부모 Canvas Sorting Order 설정 (X가 보이도록)
            Canvas parentCanvas = xButton.GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                parentCanvas.sortingOrder = 100;
                Debug.Log($"[Stage2Tutorial] {parentCanvas.name} Canvas sortingOrder = 100 설정");
                
                // GraphicRaycaster 확인 (클릭 감지에 필수)
                GraphicRaycaster raycaster = parentCanvas.GetComponent<GraphicRaycaster>();
                if (raycaster == null)
                {
                    raycaster = parentCanvas.gameObject.AddComponent<GraphicRaycaster>();
                    Debug.Log("[Stage2Tutorial] GraphicRaycaster 추가");
                }
                Debug.Log($"[Stage2Tutorial] GraphicRaycaster enabled: {raycaster.enabled}");
            }
            
            // X 버튼과 자식들의 Image Raycast Target 확인
            Image btnImage = xButton.GetComponent<Image>();
            if (btnImage != null)
            {
                btnImage.raycastTarget = true;
                Debug.Log($"[Stage2Tutorial] X 버튼 자체 Image raycastTarget = true");
            }
            
            Image[] childImages = xButton.GetComponentsInChildren<Image>();
            foreach (Image img in childImages)
            {
                img.raycastTarget = true;
                Debug.Log($"[Stage2Tutorial] {img.gameObject.name} Image raycastTarget = true");
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
        Debug.Log($"[Stage2Tutorial] {tutorialDuration}초 후 Tutorial 비활성화 예약");
        yield return new WaitForSeconds(tutorialDuration);
        
        if (tutorial != null)
        {
            tutorial.SetActive(false);
            Debug.Log("[Stage2Tutorial] Tutorial SetActive(false)");
        }
    }
    
    void OnXButtonClick()
    {
        Debug.Log("========================================");
        Debug.Log("[Stage2Tutorial] ★★★ X 버튼 클릭됨! ★★★");
        Debug.Log("========================================");
        
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
        // X 버튼 클릭 감지 (Update에서 직접)
        if (xButton != null && xButton.gameObject.activeSelf && Input.GetMouseButtonDown(0))
        {
            // 마우스가 X 버튼 위에 있는지 확인
            RectTransform rectTransform = xButton.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Vector2 localMousePosition;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform, 
                    Input.mousePosition, 
                    null, 
                    out localMousePosition))
                {
                    if (rectTransform.rect.Contains(localMousePosition))
                    {
                        Debug.Log("[Stage2Tutorial] Update에서 X 버튼 클릭 감지!");
                        OnXButtonClick();
                    }
                }
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

    public void PrintClick()
    {
        print("Click received in Stage2Tutorial");
    }
}
