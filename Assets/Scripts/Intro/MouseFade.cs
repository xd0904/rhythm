using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MouseFade : MonoBehaviour
{
    [Header("투명해질 Sprite")]
    public SpriteRenderer fadeOutSprite;
    [Header("선명해질 Sprite")]
    public SpriteRenderer fadeInSprite;

    [Header("Fade 속도")]
    public float fadeSpeed = 0.2f;

    void Start()
    {
        // 초기 Alpha 설정
        SetAlpha(fadeOutSprite, 1f); // 완전 선명
        SetAlpha(fadeInSprite, 0f);  // 완전 투명

        // 오브젝트 활성화
        fadeOutSprite.gameObject.SetActive(true);
        fadeInSprite.gameObject.SetActive(true);

        
    }

    public void SibalClick()
    {
        StartCoroutine(FadeCoroutine());
    }

    void SetAlpha(SpriteRenderer spr, float alpha)
    {
        if (spr != null)
        {
            Color c = spr.color;
            c.a = alpha;
            spr.color = c;
        }
    }

    IEnumerator FadeCoroutine()
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed;
            t = Mathf.Clamp01(t);

            // 투명해질 Sprite
            SetAlpha(fadeOutSprite, 1f - t);
            // 선명해질 Sprite
            SetAlpha(fadeInSprite, t);

            yield return null;
        }

        // 정확한 끝 값
        SetAlpha(fadeOutSprite, 0f);
        SetAlpha(fadeInSprite, 1f);
    }
}
