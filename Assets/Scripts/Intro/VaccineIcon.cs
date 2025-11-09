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
            targetObject2.SetActive(true);
            Debug.Log($"[VaccineIcon] {targetObject2.name} 활성화");
        }
        else
        {
            Debug.LogWarning("[VaccineIcon] Target Object2가 설정되지 않았습니다!");
        }
    }
}
