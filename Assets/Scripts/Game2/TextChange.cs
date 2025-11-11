using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TextChange : MonoBehaviour
{

    [Header("버튼 상태 게임오브젝트")]
    [Tooltip("기본 상태 버튼")]
    public GameObject normalButton;

    [Tooltip("호버 상태 버튼")]
    public GameObject hoveredButton;

    [Tooltip("클릭 상태 버튼")]
    public GameObject activeButton;

    [Tooltip("텍스트 창")]
    public GameObject window;

    [Tooltip("몬스터 창")]
    public GameObject window2;

    [Header("텍스트 설정")]
    [Tooltip("바뀔 텍스트 게임오브젝트")]
    public GameObject textObject;

    public SpriteRenderer blackScreen;

    [Tooltip("노이즈 지속 시간 (초)")]
    public float NoizeDuration = 1.5f;

    [Tooltip("배경화면 검게 변하는 시간 (초)")]
    public float fadeToBlackDuration = 2f;

    [Header("메시지 설정")]
    [Tooltip("자동으로 표시될 메시지들")]
    [TextArea(2, 5)]
    public string[] autoMessages = new string[]
    {
        "Hey, there!",
        "You really thought\nI was gone?",
        "That's adorable.",
        "So... shall we play again?",
        "Not that you have a choice,\nHaha!\n☆*: .. o(≧▽≦)o .. :*☆"
    };
    
    [Tooltip("각 메시지가 표시되는 시간 간격 (초)")]
    public float[] messageIntervals = new float[]
    {
        2f,  // "No" 표시 후 2초
        3f,  // "Don't close" 표시 후 3초
        5f   // 마지막 메시지 표시 후 5초 (검은 화면)
    };
    
    [Tooltip("X 버튼 클릭 시 표시될 메시지")]
    [TextArea(2, 5)]
    public string clickMessage = "Oh!\nWhy do you keep\ntrying to close me?";
    
    [Tooltip("클릭 메시지 표시 시간 (초)")]
    public float clickMessageDuration = 1.5f;

    private int messageIndex = 0;
    private TextMeshProUGUI tmpText;
    private Text legacyText;
    private TypingEffect typingEffect;
    private bool isSequenceRunning = false;
    private Coroutine autoMessageCoroutine;
    private string currentAutoMessage = ""; // 현재 자동 메시지 저장


    void Start()
    {
        SetButtonState(ButtonState.Normal);
        Debug.Log("[ExitButton] 초기화 완료");

        // 텍스트 컴포넌트 찾기
        if (textObject != null)
        {
            tmpText = textObject.GetComponent<TextMeshProUGUI>();
            legacyText = textObject.GetComponent<Text>();
            typingEffect = textObject.GetComponent<TypingEffect>();

            // 자동 메시지 시퀀스 시작
            if (autoMessages != null && autoMessages.Length > 0)
            {
                autoMessageCoroutine = StartCoroutine(AutoMessageSequence());
            }
        }
    }

    void OnMouseEnter()
    {
        if (isSequenceRunning) return;
        SetButtonState(ButtonState.Hovered);
    }

    void OnMouseExit()
    {
        if (isSequenceRunning) return;
        SetButtonState(ButtonState.Normal);
    }

    void OnMouseDown()
    {
        if (isSequenceRunning) return;
        SetButtonState(ButtonState.Active);
    }

    void OnMouseUp()
    {
        if (isSequenceRunning) return;

        // X 버튼 클릭 시 "Oh! Why..." 메시지 표시
        StartCoroutine(ShowClickMessage());
        SetButtonState(ButtonState.Hovered);
    }
    
    /// <summary>
    /// 자동으로 메시지를 순차적으로 표시
    /// </summary>
    private IEnumerator AutoMessageSequence()
    {
        for (int i = 0; i < autoMessages.Length; i++)
        {
            messageIndex = i;
            currentAutoMessage = autoMessages[i];
            
            Debug.Log($"[TextChange] {i + 1}번째 자동 메시지 표시: {autoMessages[i]}");
            
            // 메시지 표시
            if (typingEffect != null)
            {
                typingEffect.SetText(autoMessages[i]);
                
                // 타이핑이 완료될 때까지 대기
                while (typingEffect.IsTyping)
                {
                    yield return null;
                }
                
                Debug.Log($"[TextChange] 타이핑 완료");
            }
            else
            {
                SetTextDirectly(autoMessages[i]);
            }
            
            // 다음 메시지까지 대기
            if (i < messageIntervals.Length)
            {
                yield return new WaitForSeconds(messageIntervals[i]);
            }
        }
        
        // 모든 메시지 표시 완료 후 페이드 아웃
        Debug.Log("[TextChange] 모든 메시지 표시 완료, 페이드 시작");
        StartCoroutine(FadeScreen(blackScreen));
    }
    
    /// <summary>
    /// X 버튼 클릭 시 잠깐 표시되는 메시지 (타이핑 효과 없이)
    /// </summary>
    private IEnumerator ShowClickMessage()
    {
        Debug.Log("[TextChange] X 버튼 클릭! 경고 메시지 표시");
        
        // 현재 자동 메시지 저장 (복원용)
        string savedMessage = currentAutoMessage;
        
        // 클릭 메시지 표시 (타이핑 효과 없이 바로 표시)
        SetTextDirectly(clickMessage);
        
        // 지정된 시간만큼 대기
        yield return new WaitForSeconds(clickMessageDuration);
        
        // 원래 자동 메시지로 복원 (타이핑 효과 없이 바로 표시)
        SetTextDirectly(savedMessage);
        
        Debug.Log("[TextChange] 원래 메시지로 복원");
    }

    private IEnumerator FadeScreen(SpriteRenderer blackScreen)
    {
        // blackScreen 켜고 알파 초기화
        blackScreen.gameObject.SetActive(true);
        Color originalColor = blackScreen.color;
        blackScreen.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        float noiseDuration = 1.7f; // 깜빡임 총 시간
        float flashInterval = 0.05f; // 깜빡임 간격
        float elapsed = 0f;
        bool alphaOn = false;

        while (elapsed < noiseDuration)
        {
            elapsed += Time.deltaTime;

            if (elapsed % flashInterval < Time.deltaTime) // flashInterval마다 토글
            {
                alphaOn = !alphaOn;
                blackScreen.color = new Color(0f, 0f, 0f, alphaOn ? 1f : 0f);
            }

            yield return null;
        }

        // 검은 화면 페이드 인
        float fadeDuration = 0.3f;
        elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            Color c = originalColor;
            c.a = Mathf.Lerp(0f, 1f, t); // 알파 0 -> 1
            blackScreen.color = c;
            yield return null;
        }

        // window들 애니메이션으로 전환
        Game2ToGame3Transition transition = FindFirstObjectByType<Game2ToGame3Transition>();
        if (transition != null)
        {
            // 전환 애니메이션 시작
            transition.StartTransition();
        }
        else
        {
            // 전환 스크립트가 없으면 바로 씬 전환
            Debug.LogWarning("[TextChange] Game2ToGame3Transition을 찾을 수 없습니다. 바로 씬 전환합니다.");
            window.SetActive(false);
            window2.SetActive(false);
            SceneManager.LoadScene("Game3");
        }

    }


    private void SetTextDirectly(string text)
    {
        if (tmpText != null) tmpText.text = text;
        else if (legacyText != null) legacyText.text = text;
    }


    private void SetButtonState(ButtonState state)
    {
        if (normalButton != null) normalButton.SetActive(state == ButtonState.Normal);
        if (hoveredButton != null) hoveredButton.SetActive(state == ButtonState.Hovered);
        if (activeButton != null) activeButton.SetActive(state == ButtonState.Active);
    }

    private enum ButtonState
    {
        Normal,
        Hovered,
        Active
    }


}





