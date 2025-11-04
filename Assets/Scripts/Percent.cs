using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class Percent : MonoBehaviour
{
    public Image gaugeImage;
    public Text gaugeText;
    public Text gaugeText2;
    public Text gaugeText3;
    public Text gaugeText4;
    public float fillSpeed = 0.05f; // 1초에 0.3씩 (약 3.3초에 100%)
    private bool isFilling = false;

    public GameObject Object;
    public GameObject Object2;
    public GameObject Object3;
    // public GameObject Object4;
    
    [Header("5% 도달 시 변경될 이미지들")]
    [Tooltip("빨간색 퍼센트바 이미지")]
    public Image redGaugeImage;
    
    [Tooltip("바탕화면 알약 아이콘 (변경될 대상)")]
    public Image desktopVaccineIcon;
    
    [Tooltip("빨간색 바탕화면 알약 아이콘 스프라이트")]
    public Sprite redDesktopVaccineSprite;
    
    [Tooltip("프로그램 알약 아이콘 (변경될 대상)")]
    public Image programVaccineIcon;
    
    [Tooltip("빨간색 프로그램 알약 아이콘 스프라이트")]
    public Sprite redProgramVaccineSprite;
    
    [Tooltip("정상 마우스 GameObject")]
    public GameObject normalMouse;
    
    [Tooltip("빨간 마우스 GameObject")]
    public GameObject redMouse;
    
    [Tooltip("에러창 게임오브젝트")]
    public GameObject errorWindow;
    
    [Tooltip("게임 프로그램 게임오브젝트")]
    public GameObject gameProgram;
    
    [Tooltip("에러창 후 게임 프로그램 뜨기까지 대기 시간 (초)")]
    public float gameProgramDelay = 1.5f;
    
    [Header("글리치 효과 설정")]
    [Tooltip("글리치 효과 지속 시간 (초)")]
    public float glitchDuration = 3f;
    
    [Tooltip("에러창 뜨기까지 대기 시간 (초)")]
    public float errorWindowDelay = 1f;
    
    [Header("씬 전환 설정")]
    [Tooltip("전환할 씬 이름")]
    public string nextSceneName = "Game1";
    
    [Tooltip("게임 프로그램 뜬 후 씬 전환까지 대기 시간 (초)")]
    public float sceneTransitionDelay = 2f;

    public void OnStartButtonClicked()
    {
        if (!isFilling)
        {
            StartCoroutine(FillGauge());

            Object.SetActive(true);
            Object2.SetActive(true);
            Object3.SetActive(false);
        }
    }

    IEnumerator FillGauge()
    {
        isFilling = true;

        while (gaugeImage.fillAmount < 0.05f)
        {
            gaugeImage.fillAmount += Time.deltaTime * fillSpeed;
            float percent = gaugeImage.fillAmount * 100f;
            gaugeText.text = Mathf.RoundToInt(percent) + "%";
            gaugeText2.text = Mathf.RoundToInt(percent) + "%";
            gaugeText3.text = Mathf.RoundToInt(percent) + "%";
            gaugeText4.text = Mathf.RoundToInt(percent) + "%";
            yield return null;
        }

        gaugeImage.fillAmount = 0.05f;
        gaugeText.text = "5%";
        gaugeText2.text = "1024";
        gaugeText4.text = "1단계";
        isFilling = false;

        Debug.Log("게이지가 5%에 도달했습니다!");

        // 마우스 커서 빨간색으로 변경 (먼저 실행)
        ChangeToRedCursor();
        
        // "발견된 위험 요소" 텍스트 글리치 효과 (이미지 변경 전에 시작)
        yield return ApplyTextGlitch();
        
        // 이미지들 빨간색으로 변경
        ChangeToRedImages();
        
        // 1초 대기 후 에러창 띄우기
        yield return new WaitForSeconds(errorWindowDelay);
        
        if (errorWindow != null)
        {
            errorWindow.SetActive(true);
            Debug.Log("[Percent] 에러창 활성화");
        }
        
        // 에러창 뜨고 1~2초 대기 후 게임 프로그램 띄우기
        yield return new WaitForSeconds(gameProgramDelay);
        
        // 다른 오브젝트들 끄기
        if (Object != null) Object.SetActive(false);
        if (Object2 != null) Object2.SetActive(false);
        if (errorWindow != null) errorWindow.SetActive(false);
        
        Debug.Log("[Percent] 다른 창들 비활성화");
        
        // 게임 프로그램 켜기
        if (gameProgram != null)
        {
            gameProgram.SetActive(true);
            Debug.Log("[Percent] 게임 프로그램 활성화");
        }
        
        // 빨간 마우스 다시 활성화 (gameProgram에 의해 꺼졌을 수 있음)
        if (redMouse != null)
        {
            redMouse.SetActive(true);
            Mouse redMouseScript = redMouse.GetComponent<Mouse>();
            if (redMouseScript != null)
            {
                redMouseScript.enabled = false; // 움직임은 계속 비활성화
            }
            Debug.Log("[Percent] 빨간 마우스 재활성화 (고정 상태 유지)");
        }
        
        // 마우스 위치 저장 및 씬 전환
        yield return new WaitForSeconds(sceneTransitionDelay);
        SaveMousePositionAndLoadScene();
        
        //Object4.SetActive(true);
    }
    
    private void ChangeToRedImages()
    {
        // 초록색 게이지바 숨기고 빨간색 게이지바 켜기
        if (gaugeImage != null && redGaugeImage != null)
        {
            // GameObject를 끄지 않고 이미지만 투명하게
            Color transparent = gaugeImage.color;
            transparent.a = 0f;
            gaugeImage.color = transparent;
            
            redGaugeImage.gameObject.SetActive(true);
            redGaugeImage.fillAmount = 0.05f;
            Debug.Log("[Percent] 게이지바 빨간색으로 변경");
        }
        
        // 바탕화면 알약 아이콘 변경
        if (desktopVaccineIcon != null && redDesktopVaccineSprite != null)
        {
            desktopVaccineIcon.sprite = redDesktopVaccineSprite;
            Debug.Log("[Percent] 바탕화면 알약 아이콘 빨간색으로 변경");
        }
        
        // 프로그램 알약 아이콘 변경
        if (programVaccineIcon != null && redProgramVaccineSprite != null)
        {
            programVaccineIcon.sprite = redProgramVaccineSprite;
            Debug.Log("[Percent] 프로그램 알약 아이콘 빨간색으로 변경");
        }
    }
    
    private void ChangeToRedCursor()
    {
        // 정상 마우스 끄고 빨간 마우스 켜기
        if (normalMouse != null && redMouse != null)
        {
            // 정상 마우스의 현재 위치 저장
            Vector3 lastMousePosition = normalMouse.transform.position;
            
            normalMouse.SetActive(false);
            redMouse.SetActive(true);
            
            // 빨간 마우스를 마지막 위치로 설정
            redMouse.transform.position = lastMousePosition;
            
            // 빨간 마우스의 Mouse 스크립트 비활성화 (움직임 방지)
            Mouse redMouseScript = redMouse.GetComponent<Mouse>();
            if (redMouseScript != null)
            {
                redMouseScript.enabled = false;
            }
            
            Debug.Log("[Percent] 마우스 커서 빨간색으로 변경 및 고정");
        }
    }
    
    private IEnumerator ApplyTextGlitch()
    {
        if (gaugeText3 == null)
        {
            Debug.LogWarning("[Percent] gaugeText3이 없습니다!");
            yield break;
        }
        
        string originalText = gaugeText3.text;
        string glitchChars = "!@#$%^&*?~|<>{}[]NOXERR??▒?";
        
        float elapsed = 0f;
        
        Debug.Log("[Percent] 텍스트 글리치 효과 시작");
        
        while (elapsed < glitchDuration)
        {
            // 90% 확률로 글리치 상태
            if (Random.value > 0.1f)
            {
                string glitchedText = "";
                
                for (int i = 0; i < originalText.Length; i++)
                {
                    // 70% 확률로 각 글자를 랜덤 특수문자로 변경
                    if (Random.value > 0.3f)
                    {
                        glitchedText += glitchChars[Random.Range(0, glitchChars.Length)];
                    }
                    else
                    {
                        glitchedText += originalText[i];
                    }
                }
                
                gaugeText3.text = glitchedText;
            }
            else
            {
                gaugeText3.text = originalText;
            }
            
            elapsed += 0.02f;
            yield return new WaitForSeconds(0.02f);
        }
        
        // 마지막 프레임은 랜덤 ERROR 문자로 고정
        string finalGlitch = "";
        for (int i = 0; i < originalText.Length; i++)
        {
            finalGlitch += glitchChars[Random.Range(0, glitchChars.Length)];
        }
        gaugeText3.text = finalGlitch;
        
        yield return new WaitForSeconds(0.1f);
        
        // 원래 텍스트로 복구
        gaugeText3.text = originalText;
        
        Debug.Log("[Percent] 텍스트 글리치 효과 완료");
    }
    
    /// <summary>
    /// 마우스 위치를 저장하고 다음 씬으로 전환
    /// </summary>
    private void SaveMousePositionAndLoadScene()
    {
        // MousePositionData 싱글톤이 없으면 생성
        if (MousePositionData.Instance == null)
        {
            GameObject dataObject = new GameObject("MousePositionData");
            MousePositionData data = dataObject.AddComponent<MousePositionData>();
            Debug.Log("[Percent] MousePositionData 새로 생성");
        }
        
        // 약간의 지연을 두고 저장 (Awake 실행 보장)
        StartCoroutine(SaveAndLoad());
    }
    
    private IEnumerator SaveAndLoad()
    {
        // 한 프레임 대기 (MousePositionData.Awake 실행 완료 보장)
        yield return null;
        
        Debug.Log($"[Percent] SaveAndLoad 시작 - MousePositionData.Instance: {(MousePositionData.Instance != null ? "존재" : "NULL")}");
        
        // 현재 빨간 마우스의 위치만 저장 (마우스 자체는 Game1 씬의 것 사용)
        if (redMouse != null && redMouse.activeSelf)
        {
            Vector3 position;
            
            // RectTransform이면 anchoredPosition 사용, 아니면 position 사용
            RectTransform rectTransform = redMouse.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                position = rectTransform.anchoredPosition;
                Debug.Log($"[Percent] UI 마우스 위치 저장: {position}");
            }
            else
            {
                // World 좌표를 Screen 좌표로 변환한 후 UI 좌표로 변환
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    Vector3 screenPos = mainCamera.WorldToScreenPoint(redMouse.transform.position);
                    
                    // Screen 좌표를 Canvas의 anchoredPosition으로 변환
                    // Canvas 중심 기준 좌표로 변환 (1920x1080 기준)
                    float canvasWidth = 1920f;
                    float canvasHeight = 1080f;
                    position = new Vector3(
                        screenPos.x - canvasWidth / 2f,
                        screenPos.y - canvasHeight / 2f,
                        0
                    );
                    Debug.Log($"[Percent] World→UI 좌표 변환: {redMouse.transform.position} → Screen:{screenPos} → UI:{position}");
                }
                else
                {
                    position = redMouse.transform.position;
                    Debug.LogWarning("[Percent] 카메라를 찾을 수 없어 World 좌표 그대로 사용");
                }
            }
            
            MousePositionData.Instance.SaveMousePosition(position, true);
            Debug.Log($"[Percent] 빨간 마우스 위치 저장: {position}");
            
            // 저장 직후 확인
            Vector3 check = MousePositionData.Instance.GetSavedMousePosition();
            bool checkRed = MousePositionData.Instance.IsRedMouse();
            Debug.Log($"[Percent] 저장 직후 확인 - 위치: {check}, 빨간마우스: {checkRed}");
        }
        else if (normalMouse != null && normalMouse.activeSelf)
        {
            MousePositionData.Instance.SaveMousePosition(normalMouse.transform.position, false);
            Debug.Log($"[Percent] 정상 마우스 위치 저장: {normalMouse.transform.position}");
        }
        
        // 씬 전환
        Debug.Log($"[Percent] {nextSceneName} 씬으로 전환합니다");
        SceneManager.LoadScene(nextSceneName);
    }
}
