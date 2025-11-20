using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class GameSequenceManager : MonoBehaviour
{
    public static GameSequenceManager Instance { get; private set; }
    public static bool ReturnFromGameOver = false; // GameOver에서 돌아왔는지 체크


    [Header("배경화면 설정")]
    [Tooltip("배경화면 게임오브젝트")]
    public GameObject backgroundObject;
    
    [Tooltip("테두리 게임오브젝트")]
    public GameObject borderObject;
    
    [Header("백신 프로그램 설정")]
    [Tooltip("백신 아이콘")]
    public GameObject vaccineIcon;
    
    [Tooltip("백신 알람 스크립트")]
    public VaccineAlarm vaccineAlarm;
    
    [Tooltip("백신 Percent 프로그램 (GameOver 복귀 시 바로 표시)")]
    public GameObject percentProgram;

    [Header("Glitch 효과 설정")]
    [Tooltip("Glitch Material (배경에 적용할 Material)")]
    public Material glitchMaterial;
    
    [Tooltip("Glitch 효과 지속 시간 (초)")]
    public float glitchDuration = 2f;
    
    [Tooltip("Glitch 강도 (Noise 값, 높을수록 강함)")]
    public float glitchNoise = 500f;
    
    [Range(0f, 0.1f)]
    [Tooltip("Glitch 크기 (Size 값, 0~0.1 범위, 높을수록 효과 큼)")]
    public float glitchSize = 0.1f;

    [Header("시퀀스 타이밍")]
    [Tooltip("마지막 메시지 후 대기 시간")]
    public float delayBeforeBlackScreen = 2f;
    
    [Tooltip("NO 글씨 표시 시간")]
    public float noTextDisplayDuration = 4.8f;

    [Header("끌 오브젝트 설정")]
    [Tooltip("모스부호 시작 전 끌 오브젝트들")]
    public GameObject[] objectsToDisable;
    
    [Tooltip("깜빡임 후 꺼질 프로그램들")]
    public GameObject[] programsToClose;
    
    [Tooltip("Error 다 뜬 후 꺼질 오브젝트들")]
    public GameObject[] objectsToCloseAfterErrors;
    
    [Tooltip("GameOver에서 돌아왔을 때 꺼질 오브젝트들 (바이러스.exe 아이콘 등)")]
    public GameObject[] objectsToCloseOnReturn;
    
    [Tooltip("GameOver에서 돌아왔을 때 꺼질 프로그램들 (바이러스 창 등)")]
    public GameObject[] programsToCloseOnReturn;

    [Header("Error 프로그램 설정")]
    [Tooltip("랜덤하게 나타날 Error 프로그램들")]
    public GameObject[] errorPrograms;
    
    [Tooltip("NO 글씨 GameObject")]
    public GameObject noTextObject;
    
    [Tooltip("Error 생성 간격")]
    public float errorSpawnInterval = 0.1f;
    
    [Tooltip("총 생성할 Error 개수")]
    public int errorSpawnCount = 15;
    
    [Range(0f, 1f)]
    [Tooltip("Error 투명도 감소량")]
    public float errorFadeAmount = 0.01f;
    
    [Range(0f, 2f)]
    [Tooltip("Error 최소 크기 배율")]
    public float errorMinScale = 0.5f;
    
    [Range(0f, 2f)]
    [Tooltip("Error 최대 크기 배율")]
    public float errorMaxScale = 1.5f;

    [Header("Audio Clips")]
    [Tooltip("모스부호 사운드")]
    public AudioClip morseClip;

    [Tooltip("점프스퀘어 NO 사운드")]
    public AudioClip JumpSquare_NO;

    [Tooltip("에러 사운드")]
    public AudioClip Error;

    [Tooltip("글리치 화면전환 사운드")]
    public AudioClip Glitch_disappear;

    [Tooltip("알림 사운드")]
    public AudioClip Alert;

    [Header("Percent 프로그램")]
    [Tooltip("백신 Percent 프로그램")]
    public Percent percentScript;

    private TextMeshProUGUI morseTextTMP;
    private Text morseTextLegacy;
    private GameObject morseTextObject; // ExitButton에서 받아옴
    private Texture backgroundTexture; // 배경 텍스처 저장

    void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // percentScript 자동 연결 (Inspector에 안 넣었을 경우)
        if (percentScript == null && percentProgram != null)
        {
            percentScript = percentProgram.GetComponent<Percent>();
            if (percentScript == null)
            {
                Debug.LogError("[GameSequenceManager] percentProgram에 Percent 스크립트 없음!");
            }
        }
    }

    void Start()
    {
        // 배경 텍스처 미리 저장
        if (backgroundObject != null)
        {
            SaveBackgroundTexture();
        }

    }

    private void SaveBackgroundTexture()
    {
        SpriteRenderer spriteRenderer = backgroundObject.GetComponent<SpriteRenderer>();
        Image image = backgroundObject.GetComponent<Image>();
        RawImage rawImage = backgroundObject.GetComponent<RawImage>();

        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            backgroundTexture = spriteRenderer.sprite.texture;
            Debug.Log($"[GameSequenceManager] 배경 텍스처 저장 (SpriteRenderer): {backgroundTexture.name}");
        }
        else if (image != null && image.sprite != null)
        {
            backgroundTexture = image.sprite.texture;
            Debug.Log($"[GameSequenceManager] 배경 텍스처 저장 (Image): {backgroundTexture.name}");
        }
        else if (rawImage != null && rawImage.texture != null)
        {
            backgroundTexture = rawImage.texture;
            Debug.Log($"[GameSequenceManager] 배경 텍스처 저장 (RawImage): {backgroundTexture.name}");
        }
    }

    /// <summary>
    /// 전체 시퀀스 시작
    /// </summary>
    public void StartSequence(GameObject textObject)
    {
        morseTextObject = textObject;
        
        // 모스부호 텍스트 컴포넌트 찾기
        if (morseTextObject != null)
        {
            morseTextTMP = morseTextObject.GetComponent<TextMeshProUGUI>();
            morseTextLegacy = morseTextObject.GetComponent<Text>();
        }
        
        Debug.Log("[GameSequenceManager] 시퀀스 시작!");
        StartCoroutine(ExecuteSequence());
    }
    
    /// <summary>
    /// GameOver에서 돌아왔을 때 - 배경화면 복구 시점부터 시작
    /// </summary>
    public void StartFromBackgroundRestore()
    {
        Debug.Log("[GameSequenceManager] 배경화면 복구 시점부터 시작!");
        
        // BGM 피치만 복원 (GameOver에서 낮아진 피치만 1.0으로 복구)
        if (SoundManager.Instance != null && SoundManager.Instance.BGMSource != null)
        {
            SoundManager.Instance.BGMSource.pitch = 1.0f;
            Debug.Log("[GameSequenceManager] BGM 피치 1.0으로 복원 (재생은 Game1에서)");
        }
        
        // 바이러스.exe 아이콘 즉시 삭제
        if (objectsToCloseOnReturn != null && objectsToCloseOnReturn.Length > 0)
        {
            Debug.Log($"[GameSequenceManager] objectsToCloseOnReturn 배열 크기: {objectsToCloseOnReturn.Length}");
            foreach (GameObject obj in objectsToCloseOnReturn)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                    Debug.Log($"[GameSequenceManager] ✓ {obj.name} 아이콘 즉시 삭제!");
                }
                else
                {
                    Debug.LogWarning("[GameSequenceManager] ✗ null 오브젝트 발견 (Inspector에서 할당 확인 필요)");
                }
            }
        }
        else
        {
            Debug.LogError("[GameSequenceManager] ✗✗✗ objectsToCloseOnReturn이 비어있음! Inspector에서 바이러스.exe 아이콘을 할당하세요!");
        }
        
        // 바이러스 프로그램 창 즉시 삭제
        if (programsToCloseOnReturn != null && programsToCloseOnReturn.Length > 0)
        {
            Debug.Log($"[GameSequenceManager] programsToCloseOnReturn 배열 크기: {programsToCloseOnReturn.Length}");
            foreach (GameObject prog in programsToCloseOnReturn)
            {
                if (prog != null)
                {
                    prog.SetActive(false);
                    Debug.Log($"[GameSequenceManager] ✓ {prog.name} 프로그램 즉시 삭제!");
                }
                else
                {
                    Debug.LogWarning("[GameSequenceManager] ✗ null 프로그램 발견 (Inspector에서 할당 확인 필요)");
                }
            }
        }
        else
        {
            Debug.LogError("[GameSequenceManager] ✗✗✗ programsToCloseOnReturn이 비어있음! Inspector에서 바이러스 프로그램 창을 할당하세요!");
        }
        
        // 배경화면 즉시 복구
        ChangeBackgroundColor(Color.white);
        
        // 백신 아이콘 즉시 활성화
        if (vaccineIcon != null)
        {
            vaccineIcon.SetActive(true);
            Debug.Log("[GameSequenceManager] 백신 아이콘 활성화");
        }

        // 백신 알람 즉시 표시
        if (vaccineAlarm != null && vaccineAlarm.gameObject != null)
        {
            vaccineAlarm.gameObject.SetActive(true);
            vaccineAlarm.TriggerAnimation();
            Debug.Log("[GameSequenceManager] 백신 알람 즉시 트리거!");
        }
        
        // Game1 씬 미리 로드 시작 (백그라운드)
        StartCoroutine(PreloadGame1Scene());
    }
    
    /// <summary>
    /// Game1 씬을 백그라운드에서 미리 로드 (끊김 방지)
    /// </summary>
    private System.Collections.IEnumerator PreloadGame1Scene()
    {
        Debug.Log("[GameSequenceManager] Game1 씬 백그라운드 로드 시작");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Game1");
        asyncLoad.allowSceneActivation = false; // 아직 전환하지 않음
        
        // 90%까지 로드
        while (!asyncLoad.isDone && asyncLoad.progress < 0.9f)
        {
            yield return null;
        }
        
        Debug.Log("[GameSequenceManager] Game1 씬 로드 완료 (대기 중)");
    }

    private System.Collections.IEnumerator ExecuteSequence()
    {
        // 딜레이 없이 바로 시작
        
        // 1. 지정된 오브젝트들 비활성화
        DisableObjects(objectsToDisable);
        
        // 백신 아이콘 비활성화
        if (vaccineIcon != null)
        {
            vaccineIcon.SetActive(false);
            Debug.Log("[GameSequenceManager] 백신 아이콘 비활성화");
        }

        // 2. 배경화면 검은색으로
        ChangeBackgroundColor(Color.black);

        // 3. 테두리 활성화
        if (borderObject != null)
        {
            borderObject.SetActive(true);
            Debug.Log("[GameSequenceManager] 테두리 활성화");
        }


        // 5. 모스부호 표시
        yield return StartCoroutine(ShowMorseCode());

        // 6. 3번 깜빡이기
        yield return StartCoroutine(BlinkScreen());

        // 7. 프로그램들 끄기
        DisableObjects(programsToClose);

        // 8. 1초 정적
        yield return new WaitForSeconds(1f);

        // 9. Error 프로그램들 생성
        yield return StartCoroutine(SpawnErrorPrograms());

        Debug.Log("[GameSequenceManager] 시퀀스 완료!");
    }

    private System.Collections.IEnumerator ShowMorseCode()
    {
        string morseText = "ㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤ???ㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤ";
        string morseCode = ".-.  ..-  -.     .-  .--  .-  -.--  -.-.--";

        // 텍스트 색상을 빨간색으로
        SetMorseTextColor(Color.red);

        // ??? 표시
        SetMorseText(morseText);
        yield return new WaitForSeconds(3.2f);

        // SoundManager를 통해 오디오 재생 시작
        if (SoundManager.Instance != null && morseClip != null)
        {
            SoundManager.Instance.PlaySFX(morseClip);
        }

        // 오디오 길이 가져오기 (없으면 기본 타이밍 사용)
        float audioDuration = 0f;
        if (morseClip != null)
        {
            audioDuration = morseClip.length;
        }

        // 모스부호 글자 수
        int totalChars = morseCode.Length;
        
        // 오디오가 있으면 오디오 길이에 맞춰서, 없으면 기본 0.1초 간격
        float charInterval = audioDuration > 0f ? audioDuration / totalChars : 0.1f;

        // 모스부호 한 글자씩 표시
        string currentMorse = morseText + "\n";
        for (int i = 0; i <= morseCode.Length; i++)
        {
            currentMorse = morseText + "\n" + morseCode.Substring(0, i);
            SetMorseText(currentMorse);
            
            yield return new WaitForSeconds(charInterval);
        }

        yield return new WaitForSeconds(4.3f);
    }

    private System.Collections.IEnumerator BlinkScreen()
    {
        if (backgroundObject != null)
        {
            SpriteRenderer spriteRenderer = backgroundObject.GetComponent<SpriteRenderer>();
            Image image = backgroundObject.GetComponent<Image>();
            RawImage rawImage = backgroundObject.GetComponent<RawImage>();

            for (int i = 0; i < 3; i++)
            {
                // 흰색
                if (spriteRenderer != null) spriteRenderer.color = Color.white;
                else if (image != null) image.color = Color.white;
                else if (rawImage != null) rawImage.color = Color.white;

                yield return new WaitForSeconds(0.3f);

                // 검은색
                if (spriteRenderer != null) spriteRenderer.color = Color.black;
                else if (image != null) image.color = Color.black;
                else if (rawImage != null) rawImage.color = Color.black;

                yield return new WaitForSeconds(0.3f);
            }
        }

        Debug.Log("[GameSequenceManager] 깜빡임 완료");
    }

    private System.Collections.IEnumerator SpawnErrorPrograms()
    {
        // NO 글씨 활성화 (한 개만)
        if (noTextObject != null)
        {
            noTextObject.SetActive(true);
            SoundManager.Instance.PlaySFX(JumpSquare_NO);
            Debug.Log("[GameSequenceManager] NO 글씨 활성화");
        }

        yield return new WaitForSeconds(noTextDisplayDuration);

        if (errorPrograms == null || errorPrograms.Length == 0)
        {
            Debug.LogWarning("[GameSequenceManager] Error 프로그램이 설정되지 않았습니다!");
            yield break;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        Transform parent = canvas != null ? canvas.transform : null;

        List<GameObject> spawnedErrors = new List<GameObject>();

        // Error 생성
        for (int i = 0; i < errorSpawnCount; i++)
        {
            // 이전 Error들 투명도 감소
            foreach (GameObject prevError in spawnedErrors)
            {
                if (prevError != null) FadeError(prevError, errorFadeAmount);
            }

            // 랜덤 Error 선택
            int randomIndex = Random.Range(0, errorPrograms.Length);
            GameObject errorProgram = errorPrograms[randomIndex];

            if (errorProgram != null)
            {
                GameObject spawnedError = Instantiate(errorProgram, parent);
                spawnedErrors.Add(spawnedError);

                // 랜덤 위치 설정
                RectTransform rectTransform = spawnedError.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    // UI Canvas 오브젝트인 경우
                    float randomX = Random.Range(-800f, 800f);
                    float randomY = Random.Range(-400f, 400f);
                    rectTransform.anchoredPosition = new Vector2(randomX, randomY);
                    Debug.Log($"[GameSequenceManager] Error {i + 1} UI 위치: ({randomX:F1}, {randomY:F1})");
                }
                else
                {
                    // World Space 오브젝트인 경우
                    float randomX = Random.Range(-8f, 8f);
                    float randomY = Random.Range(-4f, 4f);
                    spawnedError.transform.position = new Vector3(randomX, randomY, 0f);
                    Debug.Log($"[GameSequenceManager] Error {i + 1} World 위치: ({randomX:F1}, {randomY:F1})");
                }

                // 랜덤 크기
                float randomScale = Random.Range(errorMinScale, errorMaxScale);
                Vector3 originalScale = spawnedError.transform.localScale;
                spawnedError.transform.localScale = originalScale * randomScale;

                SoundManager.Instance.PlaySFX(Error);
                spawnedError.SetActive(true);
            }

            yield return new WaitForSeconds(errorSpawnInterval);
        }

        Debug.Log("[GameSequenceManager] Error 생성 완료");

        // 1초 대기
        yield return new WaitForSeconds(1f);

        SoundManager.Instance.PlaySFX(Glitch_disappear);

        // 현재 화면 캡처 후 Glitch 효과 (Error들이 보이는 상태에서)
        yield return StartCoroutine(ApplyGlitchEffect());

        // Error 프로그램들 삭제
        foreach (GameObject error in spawnedErrors)
        {
            if (error != null) Destroy(error);
        }

        // 지정된 오브젝트들 비활성화
        DisableObjects(objectsToCloseAfterErrors);

        // 배경화면 복구
        ChangeBackgroundColor(Color.white);

        if (percentScript != null)
        {
            percentScript.beforeProduction = false;
            Debug.Log("[GameSequenceManager] percentScript.beforeProduction = false; 적용 완료");
        }
        else
        {
            Debug.LogWarning("[GameSequenceManager] percentScript가 null이라 beforeProduction을 바꿀 수 없음");
        }

        // 백신 아이콘 활성화
        if (vaccineIcon != null)
        {
            vaccineIcon.SetActive(true);
            Debug.Log("[GameSequenceManager] 백신 아이콘 활성화");
        }

        // 7초 대기
        yield return new WaitForSeconds(7f);

        // 백신 알람 애니메이션 시작
        if (vaccineAlarm != null)
        { 

            SoundManager.Instance.PlaySFX(Alert);
            vaccineAlarm.TriggerAnimation();
            Debug.Log("[GameSequenceManager] 백신 알람 애니메이션 트리거");
        }
    }

    private System.Collections.IEnumerator ApplyGlitchEffect()
    {
        if (backgroundObject == null || glitchMaterial == null)
        {
            Debug.LogWarning("[GameSequenceManager] Glitch 효과를 위한 설정이 없습니다.");
            yield break;
        }

        // 한 프레임 대기 (화면 렌더링 완료를 위해)
        yield return new WaitForEndOfFrame();

        // 현재 화면 캡처
        Texture2D screenCapture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenCapture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenCapture.Apply();
        Debug.Log($"[GameSequenceManager] 화면 캡처 완료: {Screen.width}x{Screen.height}");

        // 배경 활성화
        bool wasActive = backgroundObject.activeSelf;
        if (!wasActive)
        {
            backgroundObject.SetActive(true);
            Debug.Log("[GameSequenceManager] Glitch를 위해 배경 활성화");
        }

        // 배경을 하얀색으로 (Glitch가 보이도록)
        ChangeBackgroundColor(Color.white);

        // 원본 Material 백업
        Material originalMaterial = null;
        SpriteRenderer spriteRenderer = backgroundObject.GetComponent<SpriteRenderer>();
        Image image = backgroundObject.GetComponent<Image>();
        RawImage rawImage = backgroundObject.GetComponent<RawImage>();

        // SpriteRenderer의 경우 Order in Layer를 최상위로 올림
        int originalSortingOrder = 0;
        if (spriteRenderer != null)
        {
            originalSortingOrder = spriteRenderer.sortingOrder;
            spriteRenderer.sortingOrder = 1000; // 모든 것보다 위에
            Debug.Log($"[GameSequenceManager] Sorting Order 변경: {originalSortingOrder} → 1000");
        }

        // Glitch Material 복사
        Material glitchInstance = new Material(glitchMaterial);

        // 캡처한 화면을 Glitch Material에 전달
        glitchInstance.SetTexture("_MainTexture", screenCapture);
        Debug.Log("[GameSequenceManager] 캡처한 화면을 Glitch Material에 설정");

        // 배경의 현재 컴포넌트에 Glitch Material 적용
        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.material;
            spriteRenderer.material = glitchInstance;
        }
        else if (image != null)
        {
            originalMaterial = image.material;
            image.material = glitchInstance;
        }
        else if (rawImage != null)
        {
            originalMaterial = rawImage.material;
            rawImage.material = glitchInstance;
        }

        // Glitch 파라미터 설정
        glitchInstance.SetFloat("_Noise", glitchNoise);
        glitchInstance.SetFloat("_Size", glitchSize);
        Debug.Log($"[GameSequenceManager] Glitch 파라미터 - Noise: {glitchNoise}, Size: {glitchSize}");

        Debug.Log("[GameSequenceManager] Glitch 효과 시작");

        // Glitch 효과 지속
        yield return new WaitForSeconds(glitchDuration);

        // 원본 Material 복구
        if (spriteRenderer != null)
        {
            spriteRenderer.material = originalMaterial;
        }
        else if (image != null)
        {
            image.material = originalMaterial;
        }
        else if (rawImage != null)
        {
            rawImage.material = originalMaterial;
        }

        // SpriteRenderer의 Sorting Order 원래대로 복구
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = originalSortingOrder;
            Debug.Log($"[GameSequenceManager] Sorting Order 복구: {originalSortingOrder}");
        }

        // 배경을 다시 검은색으로
        ChangeBackgroundColor(Color.black);

        // 배경을 원래 상태로 복구
        if (!wasActive)
        {
            backgroundObject.SetActive(false);
            Debug.Log("[GameSequenceManager] Glitch 후 배경 다시 비활성화");
        }

        Debug.Log("[GameSequenceManager] Glitch 효과 종료");
        
        // 다음 프레임까지 대기 후 리소스 정리 (Material이 더 이상 사용되지 않도록)
        yield return null;
        
        // 복사한 Material과 텍스처 삭제
        if (glitchInstance != null) Destroy(glitchInstance);
        if (screenCapture != null) Destroy(screenCapture);
    }

    private void DisableObjects(GameObject[] objects)
    {
        if (objects == null) return;

        foreach (GameObject obj in objects)
        {
            if (obj != null && obj.activeSelf)
            {
                obj.SetActive(false);
                Debug.Log($"[GameSequenceManager] {obj.name} 즉시 비활성화!");
            }
        }
    }

    private void ChangeBackgroundColor(Color color)
    {
        if (backgroundObject == null) return;

        SpriteRenderer spriteRenderer = backgroundObject.GetComponent<SpriteRenderer>();
        Image image = backgroundObject.GetComponent<Image>();
        RawImage rawImage = backgroundObject.GetComponent<RawImage>();

        if (spriteRenderer != null) spriteRenderer.color = color;
        else if (image != null) image.color = color;
        else if (rawImage != null) rawImage.color = color;

        Debug.Log($"[GameSequenceManager] 배경화면 색상: {color}");
    }

    private void SetMorseText(string text)
    {
        if (morseTextTMP != null) morseTextTMP.text = text;
        else if (morseTextLegacy != null) morseTextLegacy.text = text;
    }

    private void SetMorseTextColor(Color color)
    {
        if (morseTextTMP != null) morseTextTMP.color = color;
        else if (morseTextLegacy != null) morseTextLegacy.color = color;
    }

    private void FadeError(GameObject error, float fadeAmount)
    {
        CanvasGroup canvasGroup = error.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = Mathf.Max(0f, canvasGroup.alpha - fadeAmount);
            return;
        }

        Image[] images = error.GetComponentsInChildren<Image>();
        foreach (Image img in images)
        {
            Color color = img.color;
            color.a = Mathf.Max(0f, color.a - fadeAmount);
            img.color = color;
        }

        SpriteRenderer[] sprites = error.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sprite in sprites)
        {
            Color color = sprite.color;
            color.a = Mathf.Max(0f, color.a - fadeAmount);
            sprite.color = color;
        }
    }
}
