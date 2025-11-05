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

    [Tooltip("빨갛게 변하는 시간 (초)")]
    public float fadeToRedDuration = 2f;

    [Tooltip("Glitch 지속 시간 (초)")]
    public float glitchDuration = 1.5f;

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
            
            // GameOver에서 돌아왔으면 즉시 처리
            if (GameSequenceManager.ReturnFromGameOver)
            {
                Debug.Log("[ExitButton] GameOver에서 돌아옴 - 즉시 처리");
                GameSequenceManager.ReturnFromGameOver = false; // 플래그 리셋
                
                // 버튼과 텍스트 즉시 숨기기
                isSequenceRunning = true;
                if (normalButton != null) normalButton.SetActive(false);
                if (hoveredButton != null) hoveredButton.SetActive(false);
                if (activeButton != null) activeButton.SetActive(false);
                if (textObject != null) textObject.SetActive(false);
                
                // Collider 비활성화
                Collider2D collider = GetComponent<Collider2D>();
                if (collider != null) collider.enabled = false;
                
                // GameSequenceManager에 즉시 통보
                if (GameSequenceManager.Instance != null)
                {
                    GameSequenceManager.Instance.StartFromBackgroundRestore();
                }
                
                return;
            }
            
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

        // 마지막 메시지면 점점 빨갛게 하고 시퀀스 시작
        if (clickCount == clickMessages.Length)
        {
            StartCoroutine(FadeToRedAndStartSequence());
        }
    }

    private System.Collections.IEnumerator FadeToRedAndStartSequence()
    {
        // 타이핑과 빨갛게 변하기를 동시에 시작
        StartCoroutine(FadeToRed());
        
        // 타이핑 효과가 진행되는 동안 대기 (NONONONO 텍스트가 더 많이 나오도록)
        if (typingEffect != null)
        {
            // 타이핑 시간 계산: (글자수 * 타이핑 속도)의 약 60% 정도에서 Glitch로 끊김
            string lastMessage = clickMessages[clickMessages.Length - 1];
            float typingTime = lastMessage.Length * typingEffect.typingSpeed * 0.6f;
            yield return new WaitForSeconds(typingTime);
            
            // 타이핑 강제 중단
            if (typingEffect != null)
            {
                typingEffect.StopAllCoroutines();
            }
        }

        // Glitch 효과 (텍스트 Glitch) - 끝날 때까지 대기
        yield return StartCoroutine(ApplyTextGlitch());

        // 버튼 비활성화
        isSequenceRunning = true;
        if (normalButton != null) normalButton.SetActive(false);
        if (hoveredButton != null) hoveredButton.SetActive(false);
        if (activeButton != null) activeButton.SetActive(false);
        
        // Collider 비활성화
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        Debug.Log("[ExitButton] Glitch 완료, 모스부호 시퀀스 시작");

        // Glitch 끝난 후에 모스부호 시퀀스 시작
        if (GameSequenceManager.Instance != null)
        {
            GameSequenceManager.Instance.StartSequence(textObject);
        }
        else
        {
            Debug.LogError("[ExitButton] GameSequenceManager를 찾을 수 없습니다!");
        }
    }

    private System.Collections.IEnumerator FadeToRed()
    {
        float elapsed = 0f;
        Color startColor = Color.white;
        Color endColor = Color.red;

        // 점점 빨갛게
        while (elapsed < fadeToRedDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeToRedDuration;
            Color currentColor = Color.Lerp(startColor, endColor, t);
            SetTextColor(currentColor);
            yield return null;
        }

        SetTextColor(Color.red);
    }

    private System.Collections.IEnumerator ApplyTextGlitch()
    {
        // 텍스트 깨지고 글리치 효과
        string originalText = "";
        if (tmpText != null) originalText = tmpText.text;
        else if (legacyText != null) originalText = legacyText.text;

        float glitchElapsed = 0f;
        while (glitchElapsed < glitchDuration)
        {
            // 더 강한 Glitch - 거의 항상 깨진 상태
            if (Random.value > 0.1f) // 90% 확률로 깨진 텍스트
            {
                // 더 많은 글자를 랜덤 문자로 변경
                string glitchedText = "";
                for (int i = 0; i < originalText.Length; i++)
                {
                    if (Random.value > 0.3f) // 70% 확률로 글자 변경
                    {
                        // 더 다양한 문자 사용 (특수문자 포함)
                        char[] glitchChars = { '!', '@', '#', '$', '%', '^', '&', '*', '?', '~', '|', '<', '>', '{', '}', '[', ']', 
                                               'N', 'O', 'X', 'E', 'R', 'R', 'O', 'R', '█', '▓', '▒', '░' };
                        glitchedText += glitchChars[Random.Range(0, glitchChars.Length)];
                    }
                    else
                    {
                        glitchedText += originalText[i];
                    }
                }
                SetTextDirectly(glitchedText);
            }
            else
            {
                SetTextDirectly(""); // 완전히 사라짐
            }

            yield return new WaitForSeconds(0.02f); // 더 빠르게 깜빡임
            glitchElapsed += 0.02f;
        }

        // 마지막에 완전히 깨진 텍스트로 고정
        string finalGlitch = "";
        char[] finalChars = { '█', '▓', '▒', '░', 'E', 'R', 'R', 'O', 'R', '!', '?', '#', '@', '*' };
        for (int i = 0; i < originalText.Length; i++)
        {
            finalGlitch += finalChars[Random.Range(0, finalChars.Length)];
        }
        SetTextDirectly(finalGlitch);
    }

    private void SetTextDirectly(string text)
    {
        if (tmpText != null) tmpText.text = text;
        else if (legacyText != null) legacyText.text = text;
    }

    private void SetTextColor(Color color)
    {
        if (tmpText != null) tmpText.color = color;
        else if (legacyText != null) legacyText.color = color;
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
