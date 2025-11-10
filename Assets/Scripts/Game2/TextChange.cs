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
    [Tooltip("클릭할 때마다 표시될 메시지들")]
    [TextArea(2, 5)]
    public string[] clickMessages = new string[]
    {
        "No",
        "Don't close",
        "NONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONON"
    };

    private int clickCount = 0;
    private TextMeshProUGUI tmpText;
    private Text legacyText;
    private TypingEffect typingEffect;
    private bool isSequenceRunning = false;


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


            // 첫 번째 메시지 표시 (정상 시작)
            if (clickMessages != null && clickMessages.Length > 0)
            {
                if (typingEffect != null)
                {
                    typingEffect.SetText(clickMessages[0]);
                }
                else
                {
                    SetTextDirectly(clickMessages[0]);
                }
                clickCount = 1;
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

        ChangeText();
        SetButtonState(ButtonState.Hovered);
    }

    private void ChangeText()
    {
        if (textObject == null) return;
        if (clickMessages == null || clickMessages.Length == 0) return;
        if (clickCount >= clickMessages.Length) return;

        // 현재 메시지 표시
        string newText = clickMessages[clickCount];
        clickCount++;

        Debug.Log($"[ExitButton] {clickCount}번째 메시지 표시");

        // 타이핑 효과로 변경
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

        if (clickCount == clickMessages.Length)
        {

            StartCoroutine(FadeScreen(blackScreen));
        }
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





