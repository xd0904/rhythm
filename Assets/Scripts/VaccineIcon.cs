using UnityEngine;

public class VaccineIcon : MonoBehaviour
{
    [Header("더블클릭 설정")]
    [Tooltip("더블클릭 인식 시간 (초)")]
    public float doubleClickTime = 0.3f;
    
    [Tooltip("더블클릭 시 열릴 게임오브젝트")]
    public GameObject targetObject;

    [Tooltip("바이러스 알람 울리고 난 후에 켜질 수 있게 하려고 그 오브젝트 감지")]
    public GameObject Object;   

    private float lastClickTime = 0f;
    private int clickCount = 0;
    private int trueCount = 0;

    private void Update()
    {
        if(Object.activeSelf == true)
        {
            trueCount = 1;
        }
    }

    void OnMouseDown()
    {
        float timeSinceLastClick = Time.time - lastClickTime;
        
        if (timeSinceLastClick <= doubleClickTime)
        {
            // 더블클릭!
            clickCount++;
            
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
}
