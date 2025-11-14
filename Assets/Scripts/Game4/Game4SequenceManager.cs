using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Game4SequenceManager : MonoBehaviour
{
    public static Game4SequenceManager Instance { get; private set; }

    [Header("배경화면 설정")]
    public GameObject backgroundObject;

    [Header("백신 아이콘")]
    public GameObject vaccineIcon;

    [Header("백신 알람 스크립트")]
    public VaccineAlarm vaccineAlarm;

    private Texture backgroundTexture;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (backgroundObject != null)
            SaveBackgroundTexture();

        // 시퀀스 시작
        StartCoroutine(ScreenSequence());
    }

    private void SaveBackgroundTexture()
    {
        SpriteRenderer spriteRenderer = backgroundObject.GetComponent<SpriteRenderer>();
        Image image = backgroundObject.GetComponent<Image>();
        RawImage rawImage = backgroundObject.GetComponent<RawImage>();

        if (spriteRenderer != null && spriteRenderer.sprite != null)
            backgroundTexture = spriteRenderer.sprite.texture;
        else if (image != null && image.sprite != null)
            backgroundTexture = image.sprite.texture;
        else if (rawImage != null && rawImage.texture != null)
            backgroundTexture = rawImage.texture;
    }

    IEnumerator ScreenSequence()
    {
        // 7초 대기
        yield return new WaitForSeconds(7f);

        // 알림창 표시
        if (vaccineAlarm != null)
        {
            vaccineAlarm.TriggerAnimation();
            Debug.Log("[Game4] 알림창 애니메이션 표시");
        }
        else
        {
            Debug.LogWarning("[Game4] vaccineAlarm이 연결되지 않음!");
        }
    }
}
