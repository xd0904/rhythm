using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class ErrorText : MonoBehaviour
{
    public Text gaugeText;
    public Text gaugeText2;
    void Start()
    {
        StartCoroutine(Text());
    }

    IEnumerator Text()
    {
        yield return new WaitForSeconds(2);

        gaugeText.text = "1";
        gaugeText2.text = "21830";

    }
}
