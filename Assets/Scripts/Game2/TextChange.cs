using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class TextChange : MonoBehaviour
{
    public Text gaugeText;

    [Tooltip("에러창 게임오브젝트")]
    public GameObject errorWindow; 
    public GameObject VaccineWindow;

    void Start()
    {
        StartCoroutine(Text());
    }

    IEnumerator Text()
    {
        yield return new WaitForSeconds(2);

        gaugeText.text = "21830";


    }

   
}
