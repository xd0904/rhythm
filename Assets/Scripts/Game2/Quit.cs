using UnityEngine;

public class Quit : MonoBehaviour
{

    [Tooltip("중지 누르고 꺼지기")]
    public GameObject targetObject;
    public GameObject targetObject2;

    [Tooltip("나감 사운드")]
    public AudioClip Exit;

    public void QuitButton()
    {
        if (targetObject != null)
        {
            SoundManager.Instance.PlaySFX(Exit);
            targetObject.SetActive(false);
            Debug.Log($"[VaccineIcon] {targetObject.name} 꺼짐");
        }
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
