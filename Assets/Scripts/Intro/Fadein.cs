using UnityEngine;
using UnityEngine.UI;

public class Fadein : MonoBehaviour
{
    [Header("페이드인 설정")]
    [Tooltip("페이드인 시작 전 대기 시간 (초)")]
    public float fadeInDelay = 0.5f;
    
    [Tooltip("페이드인 지속 시간 (초)")]
    public float fadeInDuration = 1f;

    void Awake()
    {
        // 초기 알파값을 0으로 설정 (투명)
        RawImage rawImage = GetComponent<RawImage>();
        Image image = GetComponent<Image>();
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        else if (rawImage != null)
        {
            Color color = rawImage.color;
            color.a = 0f;
            rawImage.color = color;
        }
        else if (image != null)
        {
            Color color = image.color;
            color.a = 0f;
            image.color = color;
        }
        else if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;
        }
    }

    void Start()
    {
        StartCoroutine(FadeIn());
    }

    private System.Collections.IEnumerator FadeIn()
    {
        // 대기 시간
        yield return new WaitForSeconds(fadeInDelay);

        // 컴포넌트 찾기
        RawImage rawImage = GetComponent<RawImage>();
        Image image = GetComponent<Image>();
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup != null)
        {
            // CanvasGroup 페이드인 (이미 Awake에서 0으로 설정됨)
            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }
        else if (rawImage != null)
        {
            // RawImage 페이드인 (이미 Awake에서 0으로 설정됨)
            Color color = rawImage.color;

            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Clamp01(elapsedTime / fadeInDuration);
                rawImage.color = color;
                yield return null;
            }
            color.a = 1f;
            rawImage.color = color;
        }
        else if (image != null)
        {
            // Image 페이드인 (이미 Awake에서 0으로 설정됨)
            Color color = image.color;

            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Clamp01(elapsedTime / fadeInDuration);
                image.color = color;
                yield return null;
            }
            color.a = 1f;
            image.color = color;
        }
        else if (spriteRenderer != null)
        {
            // SpriteRenderer 페이드인 (이미 Awake에서 0으로 설정됨)
            Color color = spriteRenderer.color;

            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Clamp01(elapsedTime / fadeInDuration);
                spriteRenderer.color = color;
                yield return null;
            }
            color.a = 1f;
            spriteRenderer.color = color;
        }
    }
}
