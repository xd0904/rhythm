using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TypingEffect : MonoBehaviour
{
    [Header("타이핑 설정")]
    [Tooltip("한 글자당 타이핑 속도 (초)")]
    public float typingSpeed = 0.05f;
    
    [Tooltip("시작 전 대기 시간 (초)")]
    public float startDelay = 0.2f;

    private TextMeshProUGUI tmpText;
    private Text legacyText;
    private string fullText;
    private string currentText = "";

    void Start()
    {
        // TextMeshPro 또는 레거시 Text 컴포넌트 찾기
        tmpText = GetComponent<TextMeshProUGUI>();
        legacyText = GetComponent<Text>();

        // 원본 텍스트 저장
        if (tmpText != null)
        {
            fullText = tmpText.text;
            tmpText.text = "";
        }
        else if (legacyText != null)
        {
            fullText = legacyText.text;
            legacyText.text = "";
        }

        // 타이핑 시작
        StartCoroutine(TypeText());
    }

    void OnEnable()
    {
        // 다시 켜질 때마다 타이핑 효과 재시작
        if (!string.IsNullOrEmpty(fullText))
        {
            StopAllCoroutines();
            currentText = "";
            if (tmpText != null)
            {
                tmpText.text = "";
            }
            else if (legacyText != null)
            {
                legacyText.text = "";
            }
            StartCoroutine(TypeText());
        }
    }

    private System.Collections.IEnumerator TypeText()
    {
        // 시작 대기
        yield return new WaitForSeconds(startDelay);

        // 한 글자씩 타이핑
        for (int i = 0; i <= fullText.Length; i++)
        {
            currentText = fullText.Substring(0, i);
            
            if (tmpText != null)
            {
                tmpText.text = currentText;
            }
            else if (legacyText != null)
            {
                legacyText.text = currentText;
            }

            yield return new WaitForSeconds(typingSpeed);
        }
    }

    // 외부에서 텍스트를 변경하고 싶을 때 사용
    public void SetText(string newText)
    {
        fullText = newText;
        StopAllCoroutines();
        currentText = "";
        if (tmpText != null)
        {
            tmpText.text = "";
        }
        else if (legacyText != null)
        {
            legacyText.text = "";
        }
        StartCoroutine(TypeText());
    }

    // 타이핑을 즉시 완료
    public void CompleteTyping()
    {
        StopAllCoroutines();
        if (tmpText != null)
        {
            tmpText.text = fullText;
        }
        else if (legacyText != null)
        {
            legacyText.text = fullText;
        }
    }
}
