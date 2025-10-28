using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ExitButton : MonoBehaviour
{
    [Header("버튼 상태 게임오브젝트")]
    [Tooltip("기본 상태 버튼")]
    public GameObject normalButton;
    
    [Tooltip("호버 상태 버튼")]
    public GameObject hoveredButton;
    
    [Tooltip("클릭 상태 버튼")]
    public GameObject activeButton;

    [Header("텍스트 설정")]
    [Tooltip("바뀔 텍스트 게임오브젝트")]
    public GameObject textObject;

    [Header("배경화면 설정")]
    [Tooltip("배경화면 게임오브젝트 (색상을 바꿀 대상)")]
    public GameObject backgroundObject;
    
    [Tooltip("테두리 게임오브젝트 (배경 꺼매질 때 활성화)")]
    public GameObject borderObject;
    
    [Tooltip("마지막 메시지 후 대기 시간 (초)")]
    public float delayBeforeBlackScreen = 2f;

    [Header("모스부호 시작 전 끌 오브젝트")]
    [Tooltip("모스부호 시퀀스가 시작될 때 비활성화할 게임오브젝트들")]
    public GameObject[] objectsToDisable;

    [Header("최종 시퀀스 설정")]
    [Tooltip("깜빡임 후 꺼질 프로그램 GameObjects")]
    public GameObject[] programsToClose;
    
    [Tooltip("Error 다 뜬 후 꺼질 GameObject들")]
    public GameObject[] objectsToCloseAfterErrors;
    
    [Tooltip("랜덤하게 나타날 Error 프로그램들")]
    public GameObject[] errorPrograms;
    
    [Tooltip("NO 글씨 GameObject (Error 프로그램과 함께 표시)")]
    public GameObject noTextObject;
    
    [Tooltip("NO 글씨가 표시된 후 대기 시간 (초)")]
    public float noTextDisplayDuration = 2f;
    
    [Tooltip("Error 프로그램 생성 간격 (초)")]
    public float errorSpawnInterval = 0.1f;
    
    [Tooltip("총 생성할 Error 프로그램 개수")]
    public int errorSpawnCount = 15;

    [Tooltip("Error 프로그램 최소 크기 배율")]
    public float errorMinScale = 0.5f;
    
    [Tooltip("Error 프로그램 최대 크기 배율")]
    public float errorMaxScale = 1.5f;

    [Tooltip("Error 생성 시 이전 Error들 투명도 감소량 (0~1, 작을수록 천천히 투명)")]
    public float errorFadeAmount = 0.01f;

    [Header("메시지 설정")]
    [Tooltip("클릭할 때마다 표시될 메시지들 (순서대로)")]
    [TextArea(2, 5)]
    public string[] clickMessages = new string[]
    {
        "no",
        "Dont close",
        "NONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONONON"
    };

    private int clickCount = 0;
    private TextMeshProUGUI tmpText;
    private Text legacyText;
    private TypingEffect typingEffect;
    private bool isLastMessageShown = false;
    private bool isLastMessageRed = false;
    private bool isSequenceRunning = false; // 시퀀스 실행 중인지 체크

    void Start()
    {
        // 시작 시 기본 상태만 활성화
        SetButtonState(ButtonState.Normal);
        Debug.Log($"[ExitButton] Start - 초기화 완료");
        Debug.Log($"[ExitButton] 이 오브젝트({gameObject.name})에 Collider 2D가 있는지 확인하세요!");
        
        // 텍스트 컴포넌트 찾기
        if (textObject != null)
        {
            tmpText = textObject.GetComponent<TextMeshProUGUI>();
            legacyText = textObject.GetComponent<Text>();
            typingEffect = textObject.GetComponent<TypingEffect>();
            
            // 첫 번째 메시지 바로 표시 (clickCount는 0부터 시작)
            if (clickMessages != null && clickMessages.Length > 0)
            {
                if (typingEffect != null)
                {
                    typingEffect.SetText(clickMessages[0]);
                }
                else
                {
                    SetTextDirectly(clickMessages[0]);
                }
                clickCount = 1; // 첫 메시지를 표시했으므로 1로 설정
            }
        }
    }

    private void SetTextDirectly(string text)
    {
        if (tmpText != null)
        {
            tmpText.text = text;
        }
        else if (legacyText != null)
        {
            legacyText.text = text;
        }
    }

    // 마우스가 들어왔을 때 (이 GameObject의 Collider에만 반응)
    void OnMouseEnter()
    {
        if (isSequenceRunning) return; // 시퀀스 실행 중이면 무시
        
        Debug.Log("[ExitButton] OnMouseEnter - 마우스 들어옴");
        SetButtonState(ButtonState.Hovered);
    }

    // 마우스가 나갔을 때
    void OnMouseExit()
    {
        if (isSequenceRunning) return; // 시퀀스 실행 중이면 무시
        
        Debug.Log("[ExitButton] OnMouseExit - 마우스 나감");
        SetButtonState(ButtonState.Normal);
    }

    // 마우스 클릭 시작
    void OnMouseDown()
    {
        if (isSequenceRunning) return; // 시퀀스 실행 중이면 무시
        
        Debug.Log("[ExitButton] OnMouseDown - 클릭 시작");
        SetButtonState(ButtonState.Active);
    }

    // 마우스 클릭 해제
    void OnMouseUp()
    {
        if (isSequenceRunning) return; // 시퀀스 실행 중이면 무시
        
        Debug.Log("[ExitButton] OnMouseUp - 클릭 해제");
        
        // 텍스트 변경
        ChangeText();
        
        // 마우스가 아직 버튼 위에 있으면 호버 상태로
        SetButtonState(ButtonState.Hovered);
    }

    private void ChangeText()
    {
        if (textObject == null) return;
        if (clickMessages == null || clickMessages.Length == 0) return;

        // 클릭 횟수가 메시지 개수를 넘으면 무시
        if (clickCount >= clickMessages.Length)
        {
            return;
        }

        // 현재 클릭 횟수에 해당하는 메시지 표시
        string newText = clickMessages[clickCount];
        
        clickCount++;
        
        Debug.Log($"[ExitButton] 텍스트 변경: {clickCount}번째 클릭, {clickCount}번째 메시지 표시");

        // TypingEffect가 있으면 타이핑 효과로 변경
        if (typingEffect != null)
        {
            typingEffect.SetText(newText);
        }
        else if (tmpText != null)
        {
            tmpText.text = newText;
        }
        else if (legacyText != null)
        {
            legacyText.text = newText;
        }

        // 마지막 메시지(3번째, NONONONO)면 점점 빨갛게 하고 자동으로 시퀀스 시작
        if (clickCount == clickMessages.Length)
        {
            StartCoroutine(FadeToRedAndStartSequence());
        }
    }

    private System.Collections.IEnumerator FadeToRedAndStartSequence()
    {
        float duration = 2f; // 빨갛게 변하는 시간
        float elapsed = 0f;
        Color startColor = Color.white;
        Color endColor = Color.red;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            Color currentColor = Color.Lerp(startColor, endColor, t);
            SetTextColor(currentColor);
            yield return null;
        }

        SetTextColor(Color.red);
        Debug.Log("[ExitButton] 텍스트가 빨간색으로 변경 완료, 시퀀스 시작");

        // 시퀀스 시작
        isLastMessageShown = true;
        isSequenceRunning = true;
        StartCoroutine(StartBlackScreenSequence());
    }

    private void SetTextColor(Color color)
    {
        if (tmpText != null)
        {
            tmpText.color = color;
        }
        else if (legacyText != null)
        {
            legacyText.color = color;
        }
    }

    private System.Collections.IEnumerator StartBlackScreenSequence()
    {
        Debug.Log("[ExitButton] 마지막 메시지 후 대기 시작");
        
        // 버튼 시각적 요소들 모두 끄기 (버튼 클릭 불가능하게)
        if (normalButton != null) normalButton.SetActive(false);
        if (hoveredButton != null) hoveredButton.SetActive(false);
        if (activeButton != null) activeButton.SetActive(false);
        
        // Collider 비활성화 (마우스 이벤트 차단하면서 코루틴 계속 실행)
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
            Debug.Log("[ExitButton] Collider 비활성화");
        }
        
        // 대기
        yield return new WaitForSeconds(delayBeforeBlackScreen);

        // 지정된 게임오브젝트들 비활성화
        if (objectsToDisable != null)
        {
            foreach (GameObject obj in objectsToDisable)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                    Debug.Log($"[ExitButton] {obj.name} 비활성화");
                }
            }
        }

        // 배경화면을 검은색으로
        if (backgroundObject != null)
        {
            ChangeBackgroundToBlack();
        }

        // 테두리 활성화
        if (borderObject != null)
        {
            borderObject.SetActive(true);
            Debug.Log("[ExitButton] 테두리 활성화");
        }

        // 모스부호 시퀀스 시작
        yield return StartCoroutine(ShowMorseCode());

        Debug.Log("[ExitButton] 모스부호 완료, 깜빡임 시작");
        
        // 3번 깜빡이기
        yield return StartCoroutine(BlinkScreen());

        Debug.Log("[ExitButton] 깜빡임 완료!");
        
        // 완전히 검은 화면으로
        Debug.Log("[ExitButton] 시퀀스 완료 - 검은 화면");
    }

    private void ChangeBackgroundToBlack()
    {
        SpriteRenderer spriteRenderer = backgroundObject.GetComponent<SpriteRenderer>();
        Image image = backgroundObject.GetComponent<Image>();
        RawImage rawImage = backgroundObject.GetComponent<RawImage>();

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.black;
        }
        else if (image != null)
        {
            image.color = Color.black;
        }
        else if (rawImage != null)
        {
            rawImage.color = Color.black;
        }
    }

    private System.Collections.IEnumerator ShowMorseCode()
    {
        string morseText = "ㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤ???ㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤㅤ";
        string morseCode = ".-.  ..-  -.     .-  .--  .-  -.--  -.-.--";

        // 텍스트 색상을 빨간색으로
        SetTextColor(Color.red);

        // ??? 표시
        if (typingEffect != null)
        {
            typingEffect.SetText(morseText);
        }
        else
        {
            SetTextDirectly(morseText);
        }

        yield return new WaitForSeconds(1f);

        // SoundManager를 통해 오디오 재생 시작
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayMorseSound();
        }

        // 오디오 길이 가져오기 (없으면 기본 타이밍 사용)
        float audioDuration = 0f;
        if (SoundManager.Instance != null)
        {
            audioDuration = SoundManager.Instance.GetMorseClipLength();
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
            SetTextDirectly(currentMorse);
            
            yield return new WaitForSeconds(charInterval);
        }

        // 오디오가 끝날 때까지 대기 (아직 재생 중이면)
        if (SoundManager.Instance != null && SoundManager.Instance.IsMorsePlaying())
        {
            yield return new WaitWhile(() => SoundManager.Instance.IsMorsePlaying());
        }

        yield return new WaitForSeconds(1f);
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

                yield return new WaitForSeconds(0.2f);

                // 검은색
                if (spriteRenderer != null) spriteRenderer.color = Color.black;
                else if (image != null) image.color = Color.black;
                else if (rawImage != null) rawImage.color = Color.black;

                yield return new WaitForSeconds(0.2f);
            }
        }
        else
        {
            Debug.LogWarning("[ExitButton] Background Object가 없어서 깜빡임 스킵");
        }

        // 깜빡임 후 프로그램들 끄기
        if (programsToClose != null && programsToClose.Length > 0)
        {
            foreach (GameObject program in programsToClose)
            {
                if (program != null)
                {
                    program.SetActive(false);
                    Debug.Log($"[ExitButton] {program.name} 프로그램 종료");
                }
            }
        }
        else
        {
            Debug.LogWarning("[ExitButton] 꺼질 프로그램이 설정되지 않았습니다!");
        }

        // 1초 정적
        Debug.Log("[ExitButton] 1초 정적 대기 시작");
        yield return new WaitForSeconds(1f);
        Debug.Log("[ExitButton] 1초 정적 대기 완료!");

        Debug.Log("[ExitButton] SpawnErrorPrograms 호출 직전!");
        // Error 프로그램들 랜덤 생성
        yield return StartCoroutine(SpawnErrorPrograms());
        Debug.Log("[ExitButton] SpawnErrorPrograms 완료!");
    }

    private System.Collections.IEnumerator SpawnErrorPrograms()
    {
        Debug.Log("[ExitButton] SpawnErrorPrograms 시작!");
        
        // NO 글씨 먼저 활성화
        if (noTextObject != null)
        {
            noTextObject.SetActive(true);
            Debug.Log("[ExitButton] NO 글씨 활성화 완료!");
        }
        else
        {
            Debug.LogWarning("[ExitButton] NO Text Object가 설정되지 않았습니다!");
        }

        // NO 글씨가 표시된 상태로 대기
        Debug.Log($"[ExitButton] NO 글씨 {noTextDisplayDuration}초 대기 시작");
        yield return new WaitForSeconds(noTextDisplayDuration);
        Debug.Log("[ExitButton] NO 글씨 대기 완료, Error 프로그램 생성 시작!");

        if (errorPrograms == null || errorPrograms.Length == 0)
        {
            Debug.LogWarning("[ExitButton] Error 프로그램이 설정되지 않았습니다!");
            yield break;
        }

        Debug.Log($"[ExitButton] Error 프로그램 {errorSpawnCount}개 생성 시작!");

        // Canvas 찾기
        Canvas canvas = FindFirstObjectByType<Canvas>();
        Transform parent = canvas != null ? canvas.transform : null;

        // 생성된 Error 프로그램들을 추적하기 위한 리스트
        List<GameObject> spawnedErrors = new List<GameObject>();

        for (int i = 0; i < errorSpawnCount; i++)
        {
            // 이전에 생성된 모든 Error들의 투명도 감소
            foreach (GameObject prevError in spawnedErrors)
            {
                if (prevError != null)
                {
                    FadeError(prevError, errorFadeAmount);
                }
            }

            // 랜덤으로 Error 프로그램 선택
            int randomIndex = Random.Range(0, errorPrograms.Length);
            GameObject errorProgram = errorPrograms[randomIndex];

            if (errorProgram != null)
            {
                // Error 프로그램 복제 (프리팹이든 씬 오브젝트든)
                GameObject spawnedError = Instantiate(errorProgram, parent);
                spawnedErrors.Add(spawnedError);
                
                // 원래 크기를 기준으로 랜덤 배율 적용
                float randomScale = Random.Range(errorMinScale, errorMaxScale);
                Vector3 originalScale = spawnedError.transform.localScale;
                spawnedError.transform.localScale = originalScale * randomScale;
                
                // 랜덤 위치 설정
                RectTransform rectTransform = spawnedError.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    // Canvas 내에서 랜덤 위치
                    float randomX = Random.Range(-800f, 800f);
                    float randomY = Random.Range(-400f, 400f);
                    rectTransform.anchoredPosition = new Vector2(randomX, randomY);
                    Debug.Log($"[ExitButton] Error {i + 1} UI 생성: 위치({randomX}, {randomY}), 크기({randomScale:F2})");
                }
                else
                {
                    // World Space라면
                    Transform errorTransform = spawnedError.transform;
                    float randomX = Random.Range(-8f, 8f);
                    float randomY = Random.Range(-4f, 4f);
                    errorTransform.position = new Vector3(randomX, randomY, 0f);
                    Debug.Log($"[ExitButton] Error {i + 1} World 생성: 위치({randomX}, {randomY}), 크기({randomScale:F2})");
                }

                spawnedError.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"[ExitButton] Error 프로그램[{randomIndex}]이 null입니다!");
            }

            yield return new WaitForSeconds(errorSpawnInterval);
        }

        Debug.Log("[ExitButton] 모든 Error 프로그램 생성 완료!");

        // 1초 대기
        yield return new WaitForSeconds(1f);

        // 생성된 모든 Error 프로그램 삭제
        Debug.Log("[ExitButton] 생성된 Error 프로그램들 삭제 시작");
        foreach (GameObject error in spawnedErrors)
        {
            if (error != null)
            {
                Destroy(error);
            }
        }
        Debug.Log("[ExitButton] 모든 Error 프로그램 삭제 완료");

        // 지정된 오브젝트들 끄기
        if (objectsToCloseAfterErrors != null && objectsToCloseAfterErrors.Length > 0)
        {
            foreach (GameObject obj in objectsToCloseAfterErrors)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                    Debug.Log($"[ExitButton] {obj.name} 비활성화");
                }
            }
        }

        // 배경화면 색 하얀색으로 복구
        if (backgroundObject != null)
        {
            ChangeBackgroundToWhite();
            Debug.Log("[ExitButton] 배경화면 색상 하얀색으로 복구");
        }
    }

    private void FadeError(GameObject error, float fadeAmount)
    {
        // CanvasGroup으로 투명도 조정
        CanvasGroup canvasGroup = error.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = Mathf.Max(0f, canvasGroup.alpha - fadeAmount);
            return;
        }

        // Image 컴포넌트
        Image[] images = error.GetComponentsInChildren<Image>();
        foreach (Image img in images)
        {
            Color color = img.color;
            color.a = Mathf.Max(0f, color.a - fadeAmount);
            img.color = color;
        }

        // SpriteRenderer 컴포넌트
        SpriteRenderer[] sprites = error.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sprite in sprites)
        {
            Color color = sprite.color;
            color.a = Mathf.Max(0f, color.a - fadeAmount);
            sprite.color = color;
        }
    }

    private void ChangeBackgroundToWhite()
    {
        SpriteRenderer spriteRenderer = backgroundObject.GetComponent<SpriteRenderer>();
        Image image = backgroundObject.GetComponent<Image>();
        RawImage rawImage = backgroundObject.GetComponent<RawImage>();

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
        else if (image != null)
        {
            image.color = Color.white;
        }
        else if (rawImage != null)
        {
            rawImage.color = Color.white;
        }
    }

    private void SetButtonState(ButtonState state)
    {
        Debug.Log($"[ExitButton] SetButtonState - {state} 상태로 변경");
        
        if (normalButton != null)
        {
            normalButton.SetActive(state == ButtonState.Normal);
        }
        
        if (hoveredButton != null)
        {
            hoveredButton.SetActive(state == ButtonState.Hovered);
        }
        
        if (activeButton != null)
        {
            activeButton.SetActive(state == ButtonState.Active);
        }
    }

    private enum ButtonState
    {
        Normal,
        Hovered,
        Active
    }
}

