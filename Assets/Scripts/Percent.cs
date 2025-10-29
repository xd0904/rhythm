using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Percent : MonoBehaviour
{
    public Image gaugeImage;
    public Text gaugeText;
    public Text gaugeText2;
    public Text gaugeText3;
    public Text gaugeText4;
    public float fillSpeed = 0.3f; // 1초에 0.3씩 (약 3.3초에 100%)
    private bool isFilling = false;

    public GameObject Object;
    public GameObject Object2;
    public GameObject Object3;
    public GameObject Object4;
    

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
        gaugeText3.text = "2";
        gaugeText4.text = "1단계";
        isFilling = false;

        Debug.Log("게이지가 50%에 도달했습니다!");

        Object4.SetActive(true);
        
    }
}
