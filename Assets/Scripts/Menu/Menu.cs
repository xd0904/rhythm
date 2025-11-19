using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Menu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
  
    public Text textComponent;

    [Header("Animation Settings")]
    public float scaleUpFactor = 1.1f;   // Ŀ�� ����
    public float speed = 8f;             // �ִϸ��̼� �ӵ�
    public Color hoverColor = Color.gray;

    [Header("Fade Settings")]
    public GameObject fadeBackground;    // 페이드 아웃용 배경
    public float fadeDuration = 1f;      // 페이드 시간

    private Vector3 originalScale;
    private Color originalColor;
    private bool isHovered = false;
    private bool isTransitioning = false;

    void Awake()
    {
        // 페이드 배경 초기화 (완전 불투명)
        if (fadeBackground != null)
        {
            // Image 컴포넌트 체크
            Image fadeImage = fadeBackground.GetComponent<Image>();
            if (fadeImage != null)
            {
                Color c = fadeImage.color;
                c.a = 1f;
                fadeImage.color = c;
            }
            
            // SpriteRenderer 컴포넌트 체크
            SpriteRenderer fadeSprite = fadeBackground.GetComponent<SpriteRenderer>();
            if (fadeSprite != null)
            {
                Color c = fadeSprite.color;
                c.a = 1f;
                fadeSprite.color = c;
            }

            fadeBackground.SetActive(true);
        }
    }

    void Start()
    {
        originalScale = transform.localScale;
        originalColor = textComponent.color;
        
        // 페이드인 시작
        if (fadeBackground != null)
        {
            StartCoroutine(FadeInAtStart());
        }
    }

    /// <summary>
    /// 씬 시작 시 페이드인
    /// </summary>
    IEnumerator FadeInAtStart()
    {
        if (fadeBackground == null)
        {
            yield break;
        }

        // Image 또는 SpriteRenderer 찾기
        Image fadeImage = fadeBackground.GetComponent<Image>();
        SpriteRenderer fadeSprite = fadeBackground.GetComponent<SpriteRenderer>();

        if (fadeImage == null && fadeSprite == null)
        {
            yield break;
        }

        // 페이드 인 (불투명 → 투명)
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            // Image 사용
            if (fadeImage != null)
            {
                Color c = fadeImage.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                fadeImage.color = c;
            }

            // SpriteRenderer 사용
            if (fadeSprite != null)
            {
                Color c = fadeSprite.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                fadeSprite.color = c;
            }

            yield return null;
        }

        // 최종 색상 설정 (완전 투명)
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }
        if (fadeSprite != null)
        {
            Color c = fadeSprite.color;
            c.a = 0f;
            fadeSprite.color = c;
        }

        fadeBackground.SetActive(false);
    }

    void Update()
    {
        // ���� ���¿� ���� ũ��� ���� �ε巴�� ����
        Vector3 targetScale = isHovered ? originalScale * scaleUpFactor : originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * speed);

        Color targetColor = isHovered ? hoverColor : originalColor;
        textComponent.color = Color.Lerp(textComponent.color, targetColor, Time.deltaTime * speed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }


    public void OnPlayButton()
    {
        if (!isTransitioning)
        {
            StartCoroutine(FadeOutAndLoadScene("Intro"));
        }
    }

    public void OnQuitButton()
    {
       // Application.Quit(); 
    }

    /// <summary>
    /// 페이드 아웃 후 씬 전환
    /// </summary>
    IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        isTransitioning = true;

        if (fadeBackground == null)
        {
            Debug.LogWarning("[Menu] fadeBackground가 없어서 바로 씬 전환합니다.");
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        // Image 또는 SpriteRenderer 찾기
        Image fadeImage = fadeBackground.GetComponent<Image>();
        SpriteRenderer fadeSprite = fadeBackground.GetComponent<SpriteRenderer>();

        if (fadeImage == null && fadeSprite == null)
        {
            Debug.LogWarning("[Menu] fadeBackground에 Image나 SpriteRenderer 컴포넌트가 없어서 바로 씬 전환합니다.");
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        // 초기 색상을 투명하게 설정
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }
        if (fadeSprite != null)
        {
            Color c = fadeSprite.color;
            c.a = 0f;
            fadeSprite.color = c;
        }

        // 페이드 배경 활성화
        fadeBackground.SetActive(true);

        // 페이드 아웃 (투명 → 불투명)
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            // Image 사용
            if (fadeImage != null)
            {
                Color c = fadeImage.color;
                c.a = Mathf.Lerp(0f, 1f, t);
                fadeImage.color = c;
            }

            // SpriteRenderer 사용
            if (fadeSprite != null)
            {
                Color c = fadeSprite.color;
                c.a = Mathf.Lerp(0f, 1f, t);
                fadeSprite.color = c;
            }

            yield return null;
        }

        // 최종 색상 설정 (완전 불투명)
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 1f;
            fadeImage.color = c;
        }
        if (fadeSprite != null)
        {
            Color c = fadeSprite.color;
            c.a = 1f;
            fadeSprite.color = c;
        }

        // 씬 전환
        SceneManager.LoadScene(sceneName);
    }
}


