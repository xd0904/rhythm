using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MouseFade : MonoBehaviour
{
    [Header("�������� Sprite")]
    public SpriteRenderer fadeOutSprite;
    [Header("�������� Sprite")]
    public SpriteRenderer fadeInSprite;

    [Header("Fade �ӵ�")]
    public float fadeSpeed = 0.2f;

    void Start()
    {
        // �ʱ� Alpha ����
        SetAlpha(fadeOutSprite, 1f); // ���� ����
        SetAlpha(fadeInSprite, 0f);  // ���� ����

        // ������Ʈ Ȱ��ȭ
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

            // �������� Sprite
            SetAlpha(fadeOutSprite, 1f - t);
            // �������� Sprite
            SetAlpha(fadeInSprite, t);

            yield return null;
        }

        // ��Ȯ�� �� ��
        SetAlpha(fadeOutSprite, 0f);
        SetAlpha(fadeInSprite, 1f);
    }
}
