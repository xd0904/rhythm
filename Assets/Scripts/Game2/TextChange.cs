using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class TextChange : MonoBehaviour
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
