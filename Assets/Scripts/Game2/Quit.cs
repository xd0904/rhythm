using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class Quit : MonoBehaviour
{

    [Tooltip("중지 누르고 꺼지기")]
    public GameObject targetObject;
    public GameObject targetObject2;

    [Tooltip("나감 사운드")]
    public AudioClip Exit;

    [Header("페이드 아웃 설정")]
    [Tooltip("페이드 아웃용 배경 오브젝트")]
    public GameObject fadeBackground;
    [Tooltip("페이드 아웃 지속 시간")]
    public float fadeDuration = 1f;

    public void QuitButton()
    {
        // 페이드아웃 후 Menu 씬으로 이동
        StartCoroutine(FadeOutAndLoadMenu());
    }

    private IEnumerator FadeOutAndLoadMenu()
    {
        // 사운드 재생
        if (Exit != null)
        {
            SoundManager.Instance.PlaySFX(Exit);
        }

        if (fadeBackground == null)
        {
            Debug.LogWarning("[Quit] fadeBackground가 없어서 바로 씬 전환합니다.");
            if (targetObject != null)
            {
                targetObject.SetActive(false);
            }
            SceneManager.LoadScene("Menu");
            yield break;
        }

        // Image 또는 SpriteRenderer 찾기
        Image fadeImage = fadeBackground.GetComponent<Image>();
        SpriteRenderer fadeSprite = fadeBackground.GetComponent<SpriteRenderer>();

        if (fadeImage == null && fadeSprite == null)
        {
            Debug.LogWarning("[Quit] fadeBackground에 Image나 SpriteRenderer 컴포넌트가 없어서 바로 씬 전환합니다.");
            if (targetObject != null)
            {
                targetObject.SetActive(false);
            }
            SceneManager.LoadScene("Menu");
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

        // 오브젝트 비활성화
        if (targetObject != null)
        {
            targetObject.SetActive(false);
            Debug.Log($"[Quit] {targetObject.name} 꺼짐");
        }

        // Menu 씬으로 전환
        SceneManager.LoadScene("Menu");
    }

    public void StopButton()
    {
        // 게임 프로그램 켜기
        if (targetObject != null)
        {
            targetObject.SetActive(true);
            targetObject2.SetActive(false);
            Debug.Log("[Percent] 게임 프로그램 활성화");
        }
    }
}
