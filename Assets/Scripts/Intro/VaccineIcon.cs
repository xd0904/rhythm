using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VaccineIcon : MonoBehaviour
{
    [Header("더블클릭 설정")]
    [Tooltip("더블클릭 인식 시간 (초)")]
    public float doubleClickTime = 0.3f;
    
    [Tooltip("더블클릭 시 열릴 게임오브젝트")]
    public GameObject targetObject;

    [Tooltip("더블클릭 시 열릴 게임오브젝트")]
    public GameObject targetObject2;

    [Tooltip("바이러스 알람 울리고 난 후에 켜질 수 있게 하려고 그 오브젝트 감지")]
    public GameObject Object;

    [Tooltip("창 애니메이션 지속 시간")]
    public float animationDuration = 0.5f;

    private float lastClickTime = 0f;
    private int clickCount = 0;
    private int trueCount = 0;

    private string sceneName; // 전역 변수로 선언 (여기가 중요!)

    private void Start()
    {
        sceneName = SceneManager.GetActiveScene().name;
    }

    private void Update()
    {
        if(sceneName == "Intro")
        {
            // GameOver에서 돌아온 경우 즉시 활성화
            if (GameSequenceManager.ReturnFromGameOver && trueCount == 0)
            {
                trueCount = 1;
                Debug.Log("[VaccineIcon] GameOver 복귀 - 즉시 활성화");
            }

            // Object가 null이 아닐 때만 체크
            if (Object != null && Object.activeSelf == true)
            {
                trueCount = 1;
            }
        }
   
        
    }

    void OnMouseDown()
    {
        float timeSinceLastClick = Time.time - lastClickTime;
        
        if (timeSinceLastClick <= doubleClickTime)
        {
            // 더블클릭!
            clickCount++;

            if (clickCount >= 2 && trueCount == 0 && sceneName == "Game2")
            {
                OnDoubleClick();
                clickCount = 0;
            }
            else if (clickCount >= 2 && trueCount == 1 && sceneName == "Game2")
            {
                OnDoubleClick2();
                clickCount = 0;
            }

            if (clickCount >= 2 && trueCount == 1)
            {
                OnDoubleClick();
                clickCount = 0;
            }
            
            

        }
        else
        {
            // 첫 클릭
            clickCount = 1;
        }
        
        lastClickTime = Time.time;
    }
    private void OnDoubleClick()
    {
        if (sceneName == "Game2")
        {
            trueCount = 1;
        }
        
        Debug.Log("[VaccineIcon] 더블클릭 감지!");
        
        if (targetObject != null)
        {
            targetObject.SetActive(true);
            Debug.Log($"[VaccineIcon] {targetObject.name} 활성화");
        }
        else
        {
            Debug.LogWarning("[VaccineIcon] Target Object가 설정되지 않았습니다!");
        }
    }

   private void OnDoubleClick2()
{
    Debug.Log("[VaccineIcon] 더블클릭 감지!");

    if (targetObject2 != null)
    {
        // 바로 켜지지 않고 애니메이션으로 켜기
        StartCoroutine(OpenWindowAnimated(targetObject2, 0.3f, 0.5f)); // duration, 시작 scale
        Debug.Log($"[VaccineIcon] {targetObject2.name} 애니메이션 활성화");
    }
    else
    {
        Debug.LogWarning("[VaccineIcon] Target Object2가 설정되지 않았습니다!");
    }
}

    private System.Collections.IEnumerator OpenWindowAnimated(GameObject window, float animationDuration, float startScale)
    {
        window.SetActive(true);

        // 실제 Scale값 가져오기
        Vector3 targetScale = window.transform.localScale;
        Vector3 initialScale = targetScale * startScale;
        window.transform.localScale = initialScale;

        // SpriteRenderer가 있으면 알파도 적용
        SpriteRenderer sr = window.GetComponent<SpriteRenderer>();
        Color originalColor = Color.white;
        if (sr != null)
        {
            originalColor = sr.color;
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
        }

        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;

            // Ease-out (빠르게 시작해서 천천히 끝)
            float easedT = 1f - Mathf.Pow(1f - t, 3f);

            // Scale 적용
            window.transform.localScale = Vector3.Lerp(initialScale, targetScale, easedT);

            // Alpha 적용
            if (sr != null)
            {
                Color c = sr.color;
                c.a = easedT;
                sr.color = c;
            }

            yield return null;
        }

        // 최종값 정확히 맞추기
        window.transform.localScale = targetScale;
        if (sr != null)
        {
            sr.color = originalColor;
        }
    }

}
