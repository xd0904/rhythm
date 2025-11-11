using UnityEngine;
using UnityEngine.UI;

public class Exe : MonoBehaviour
{
    [Header("더블클릭 설정")]
    [Tooltip("더블클릭 시 활성화할 게임오브젝트")]
    public GameObject targetObject;

    
    [Tooltip("더블클릭 인식 시간 (초)")]
    public float doubleClickTime = 0.3f;

    [Tooltip("클릭 사운드")]
    public AudioClip ClickSound;

    private Image image;
    private RawImage rawImage;
    private SpriteRenderer spriteRenderer;
    
    private float lastClickTime = 0f;
    private int clickCount = 0;

    void Start()
    {
        // 컴포넌트 찾기
        image = GetComponent<Image>();
        rawImage = GetComponent<RawImage>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 기본 투명도 0으로 설정
        SetAlpha(0f);
    }

    // SpriteRenderer용: 마우스가 들어왔을 때
    void OnMouseEnter()
    {
        // 흰색(FFFFFF)에 투명도 10 (0.1)
        SetColor(new Color(1f, 1f, 1f, 0.1f));
    }

    // SpriteRenderer용: 마우스가 나갔을 때
    void OnMouseExit()
    {
        // 투명도 0
        SetAlpha(0f);
    }

    // SpriteRenderer용: 마우스 클릭 시
    void OnMouseDown()
    {
        float timeSinceLastClick = Time.time - lastClickTime;
        SoundManager.Instance.PlaySFX(ClickSound);
        
        if (timeSinceLastClick <= doubleClickTime)
        {
            clickCount++;
            
            // 더블클릭 감지
            if (clickCount >= 2)
            {
                OnDoubleClick();
                clickCount = 0;
            }
        }
        else
        {
            clickCount = 1;
        }
        
        lastClickTime = Time.time;
    }

    // 더블클릭 시 실행되는 함수
    private void OnDoubleClick()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(true);
        }
    }

    private void SetColor(Color color)
    {
        if (image != null)
        {
            image.color = color;
        }
        else if (rawImage != null)
        {
            rawImage.color = color;
        }
        else if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }

    private void SetAlpha(float alpha)
    {
        if (image != null)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
        else if (rawImage != null)
        {
            Color color = rawImage.color;
            color.a = alpha;
            rawImage.color = color;
        }
        else if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }
}
