using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExitButton : MonoBehaviour
{
    [Header("버튼 상태 게임오브젝트")]
    [Tooltip("기본 상태 버튼")]
    public GameObject normalButton;
    
    [Tooltip("호버 상태 버튼")]
    public GameObject hoveredButton;
    
    [Tooltip("클릭 상태 버튼")]
    public GameObject activeButton;

    [Header("텍스트 설정")]
    [Tooltip("바뀔 텍스트 게임오브젝트")]
    public GameObject textObject;

    [Header("메시지 설정")]
    [Tooltip("기본 텍스트 (처음 표시될 내용)")]
    [TextArea(2, 4)]
    public string defaultMessage = "";
    
    [Tooltip("클릭할 때마다 표시될 메시지들 (순서대로)")]
    [TextArea(2, 5)]
    public string[] clickMessages = new string[]
    {
        "no",
        "Dont close",
        "NONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONON"
    };

    private int clickCount = 0;
    private TextMeshProUGUI tmpText;
    private Text legacyText;
    private TypingEffect typingEffect;

    void Start()
    {
        // 시작 시 기본 상태만 활성화
        SetButtonState(ButtonState.Normal);
        Debug.Log($"[ExitButton] Start - 초기화 완료");
        Debug.Log($"[ExitButton] 이 오브젝트({gameObject.name})에 Collider 2D가 있는지 확인하세요!");
        
        // 텍스트 컴포넌트 찾기
        if (textObject != null)
        {
            tmpText = textObject.GetComponent<TextMeshProUGUI>();
            legacyText = textObject.GetComponent<Text>();
            typingEffect = textObject.GetComponent<TypingEffect>();
            
            // 기본 메시지 설정
            if (!string.IsNullOrEmpty(defaultMessage))
            {
                SetTextDirectly(defaultMessage);
            }
        }
    }

    private void SetTextDirectly(string text)
    {
        if (tmpText != null)
        {
            tmpText.text = text;
        }
        else if (legacyText != null)
        {
            legacyText.text = text;
        }
    }

    // 마우스가 들어왔을 때 (이 GameObject의 Collider에만 반응)
    void OnMouseEnter()
    {
        Debug.Log("[ExitButton] OnMouseEnter - 마우스 들어옴");
        SetButtonState(ButtonState.Hovered);
    }

    // 마우스가 나갔을 때
    void OnMouseExit()
    {
        Debug.Log("[ExitButton] OnMouseExit - 마우스 나감");
        SetButtonState(ButtonState.Normal);
    }

    // 마우스 클릭 시작
    void OnMouseDown()
    {
        Debug.Log("[ExitButton] OnMouseDown - 클릭 시작");
        SetButtonState(ButtonState.Active);
    }

    // 마우스 클릭 해제
    void OnMouseUp()
    {
        Debug.Log("[ExitButton] OnMouseUp - 클릭 해제");
        
        // 텍스트 변경
        ChangeText();
        
        // 마우스가 아직 버튼 위에 있으면 호버 상태로
        SetButtonState(ButtonState.Hovered);
    }

    private void ChangeText()
    {
        if (textObject == null) return;
        if (clickMessages == null || clickMessages.Length == 0) return;

        // 클릭 횟수에 따라 메시지 선택 (마지막 메시지 이후는 계속 마지막 메시지 반복)
        int messageIndex = Mathf.Min(clickCount, clickMessages.Length - 1);
        string newText = clickMessages[messageIndex];
        
        clickCount++;
        
        Debug.Log($"[ExitButton] 텍스트 변경: {clickCount}번 클릭, {messageIndex + 1}번째 메시지");

        // TypingEffect가 있으면 타이핑 효과로 변경
        if (typingEffect != null)
        {
            typingEffect.SetText(newText);
        }
        else if (tmpText != null)
        {
            tmpText.text = newText;
        }
        else if (legacyText != null)
        {
            legacyText.text = newText;
        }
    }

    private void SetButtonState(ButtonState state)
    {
        Debug.Log($"[ExitButton] SetButtonState - {state} 상태로 변경");
        
        if (normalButton != null)
        {
            normalButton.SetActive(state == ButtonState.Normal);
        }
        
        if (hoveredButton != null)
        {
            hoveredButton.SetActive(state == ButtonState.Hovered);
        }
        
        if (activeButton != null)
        {
            activeButton.SetActive(state == ButtonState.Active);
        }
    }

    private enum ButtonState
    {
        Normal,
        Hovered,
        Active
    }
}

