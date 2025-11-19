using UnityEngine;
using UnityEngine.UI;

public class Fadeout : MonoBehaviour
{
    [Header("페이드아웃 설정")]
    [Tooltip("페이드아웃 시작 전 대기 시간 (초)")]
    public float fadeOutDelay = 0.5f;
    
    [Tooltip("페이드아웃 지속 시간 (초)")]
    public float fadeOutDuration = 1f;

    void Awake()
    {
        // 초기 알파값을 1로 설정 (불투명)
        RawImage rawImage = GetComponent<RawImage>();
        Image image = GetComponent<Image>();
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
        else if (rawImage != null)
        {
            Color color = rawImage.color;
            color.a = 1f;
            rawImage.color = color;
        }
        else if (image != null)
        {
            Color color = image.color;
            color.a = 1f;
            image.color = color;
        }
        else if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }
    }

    void Start()
    {
        StartCoroutine(FadeOut());
    }

    private System.Collections.IEnumerator FadeOut()
    {
        // 대기 시간
        yield return new WaitForSeconds(fadeOutDelay);

        // 컴포넌트 찾기
        RawImage rawImage = GetComponent<RawImage>();
        Image image = GetComponent<Image>();
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup != null)
        {
            // CanvasGroup 페이드아웃
            canvasGroup.alpha = 1f;
            float elapsedTime = 0f;
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = 1f - Mathf.Clamp01(elapsedTime / fadeOutDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }
        else if (rawImage != null)
        {
            // RawImage 페이드아웃
            Color color = rawImage.color;
            color.a = 1f;
            rawImage.color = color;

            float elapsedTime = 0f;
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                color.a = 1f - Mathf.Clamp01(elapsedTime / fadeOutDuration);
                rawImage.color = color;
                yield return null;
            }
            color.a = 0f;
            rawImage.color = color;
        }
        else if (image != null)
        {
            // Image 페이드아웃
            Color color = image.color;
            color.a = 1f;
            image.color = color;

            float elapsedTime = 0f;
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                color.a = 1f - Mathf.Clamp01(elapsedTime / fadeOutDuration);
                image.color = color;
                yield return null;
            }
            color.a = 0f;
            image.color = color;
        }
        else if (spriteRenderer != null)
        {
            // SpriteRenderer 페이드아웃
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;

            float elapsedTime = 0f;
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                color.a = 1f - Mathf.Clamp01(elapsedTime / fadeOutDuration);
                spriteRenderer.color = color;
                yield return null;
            }
            color.a = 0f;
            spriteRenderer.color = color;
        }
    }
}
